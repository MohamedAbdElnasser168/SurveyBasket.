
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using SurveyBasket.Api.Abstractions;
using SurveyBasket.Api.Errors;
using SurveyBasket.Api.Helpers;
using System.Security.Cryptography;
using System.Text;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace SurveyBasket.Api.Services
{
    // ApplicationUser is derived from IdentityUser class
    public class AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AuthService> logger,
        IJwtProvider jwtProvider,
        IEmailSender emailSender,
        IHttpContextAccessor httpContextAccessor ,
        ApplicationDbContext context ) : IAuthService
    {
        #region Fields

        private readonly IJwtProvider _jwtProvider = jwtProvider;
        private readonly IEmailSender _emailSender = emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly ILogger<AuthService> _logger = logger;
        private readonly ApplicationDbContext _context = context;
        private readonly int _refreshTokenExpireDays = 14;
        #endregion


        // login user and return jwt token if successful, otherwise return failure result with invalid credentials error
        public async Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default)
        {

            // check if the result of find user by email is not null and assign it to user variable, otherwise return failure result with invalid credentials error
            if (await _userManager.FindByEmailAsync(email) is not { } user)
                return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

            if(user.IsDisabled)
                return Result.Failure<AuthResponse>(UserErrors.DisabledUser);

            var result = await _signInManager.PasswordSignInAsync
                (
                    user,
                    password,
                    false,
                    true
                );

            if (result.Succeeded)
            {
                // select user roles and permissions from the database and generate jwt token with them
               
                var (userRoles, userPermissions) = await GetUserRolesAndPermissions(user, cancellationToken);

                // generate jwt token with user info, roles and permissions and return it with the expiration time in seconds
                var (token, expiresIn) = _jwtProvider.GenerateToken(user,userRoles,userPermissions);
                // return auth response with token and user info

                // Generate refresh token
                var refreshToken = GenerateRefreshToken();
                var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpireDays);

                // Here you should save the refresh token and its expiration to the database associated with the user

                user.RefreshTokens.Add(new RefreshToken
                {
                    Token = refreshToken,
                    ExpiresOn = refreshTokenExpiration
                });

                await _userManager.UpdateAsync(user);

                //return new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName,token,expiresIn,refreshToken,refreshTokenExpiration);
                var response = new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, token, expiresIn, refreshToken, refreshTokenExpiration);
                return Result.Success(response);
            }


            var error =// if 
                  result.IsNotAllowed ? UserErrors.EmailNotConfirmed 
                : result.IsLockedOut ? UserErrors.LockedUser // else if 
                : UserErrors.InvalidCredentials; // else 

            return Result.Failure<AuthResponse>(error);

        }


        public async Task<Result> RegisterAsync(Contracts.Authentication.RegisterRequest request, CancellationToken cancellationToken = default)
        {
            var emailExists = await _userManager.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);
            if (emailExists)
            {
                return Result.Failure(UserErrors.DublicatedEmail);
            }

            var user = request.Adapt<ApplicationUser>();
            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                // generate code for email confirmation for a specified user and encode it to base64 url encoding to be used in the email confirmation link and log the code for testing purposes

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                _logger.LogInformation("User {Email} registered successfully. Confirmation code: {Code}", user.Email, code);

                // send email 

                await SendConfirmationEmail(user, code);

                return Result.Success();
            }

            var error = result.Errors.First();
            return Result.Failure<AuthResponse>(new Error(error.Code, error.Description, StatusCodes.Status409Conflict));
            
        }


        public async Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default) 
        {
            if (await _userManager.FindByIdAsync(request.UserId) is not { } user)
                return Result.Failure(UserErrors.InvalidCodde);

            if (user.EmailConfirmed)
                return Result.Failure(UserErrors.DublicatedConfirmation);

            var code = request.Code;

            try
            {
                // Decode the code from Base64 URL encoding to get the original email confirmation token and compare it with the token generated
                // by the user manager for the specified user, if they are equal confirm the email and return success result,
                // otherwise return failure result with invalid code error 
                code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            }
            catch (FormatException)
            {
                return Result.Failure(UserErrors.InvalidCodde);
            }


            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, DefaultRoles.Member);
                return Result.Success();
            }
            
            var error = result.Errors.First();
            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));


        }

       
        public async Task<Result> ResendConfirmationEmailAsync(Contracts.Authentication.ResendConfirmationEmailRequest request, CancellationToken cancellationToken = default)
        {
            if (await _userManager.FindByEmailAsync(request.Email) is not { } user)
                return Result.Success(); // to prevent email enumeration attacks, we return success even if the email is not found

            if (user.EmailConfirmed) 
                return Result.Failure(UserErrors.DublicatedConfirmation);
            

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            _logger.LogInformation("Resent confirmation email to {Email}. Confirmation code: {Code}", user.Email, code);

            // send email

            await SendConfirmationEmail(user, code);

            return Result.Success();
        }



        // generate new jwt token and refresh token using refresh token and revoke the old refresh token 
        public async Task<Result<AuthResponse>> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
        {
            // Validate the refresh token and get the user associated with it
            var userId = _jwtProvider.ValidateToken(token);
            if (userId is null)
            {
                return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials)!;
            }
            // Get the user from the database
            var user = _userManager
                .Users.FirstOrDefault(u => u.Id == userId);

            if (user is null)
            {
                return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials)!;
            }

            if (user.IsDisabled)
                return Result.Failure<AuthResponse>(UserErrors.DisabledUser);

            if (user.LockoutEnd > DateTime.UtcNow)
                return Result.Failure<AuthResponse>(UserErrors.LockedUser);


            // Check if the refresh token is valid and active and get the refresh token from the database
            var userRefreshToken = user.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshToken && rt.IsActive);

            if (userRefreshToken is null)
            {
                return Result.Failure<AuthResponse>(UserErrors.InvalidRefreshTokens)!;
            }

            // Revoke the old refresh token
            userRefreshToken.RevokedOn = DateTime.UtcNow;

            // Get user roles and permissions from the database
            var (userRoles, userPermissions) = await GetUserRolesAndPermissions(user, cancellationToken);

            // Generate new JWT token
            var (newToken, expiresIn) = _jwtProvider.GenerateToken(user, userRoles, userPermissions);


            // Generate refresh token
            var newRefreshToken = GenerateRefreshToken();
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpireDays);

            // Here you should save the refresh token and its expiration to the database associated with the user

            user.RefreshTokens.Add(new RefreshToken
            {
                Token = newRefreshToken,
                ExpiresOn = refreshTokenExpiration
            });

            // Update the user with the new refresh token
            await _userManager.UpdateAsync(user);


            // return auth response with token and user info
            var response = new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, newToken, expiresIn, newRefreshToken, refreshTokenExpiration);
            return Result.Success(response)!;
        }



        // Revoke the refresh token and return true if successful, otherwise return false
        public async Task<Result> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
        {

            // Validate the refresh token and get the user associated with it
            var userId = _jwtProvider.ValidateToken(token);
            if (userId is null)
            {
                return Result.Failure(UserErrors.InvalidRefreshTokens);
            }
            // Get the user from the database
            var user = _userManager
                .Users.FirstOrDefault(u => u.Id == userId);

            if (user is null)
            {
                return Result.Failure(UserErrors.InvalidCredentials);
            }

            // Check if the refresh token is valid and active and get the refresh token from the database
            var userRefreshToken = user.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshToken && rt.IsActive);

            if (userRefreshToken is null)
            {
                return Result.Failure(UserErrors.InvalidRefreshTokens);
            }

            // Revoke the old refresh token
            userRefreshToken.RevokedOn = DateTime.UtcNow;

            // Update the user with the new refresh token
            await _userManager.UpdateAsync(user);

            return Result.Success();


        }




        public async Task<Result> SendResetPasswordCodeAsync(string email)
        {
            // var isEmailExist = await _userManager.Users.AnyAsync(u => u.Email == email);
            if (await _userManager.FindByEmailAsync(email) is not { } user)
                return Result.Success(); // to prevent email enumeration attacks, we return success even if the email is not found


            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            _logger.LogInformation("User {Email} , Reset code: {Code}", user.Email, code);

            await SendResetPasswordEmail(user, code);

            return Result.Success();


        }



        public async Task<Result> ResetPasswordAsync(REsetPasswordRequest request)
        { 
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null || !user.EmailConfirmed)
                return Result.Failure(UserErrors.InvalidCodde);

            IdentityResult result;
            try
            {
                var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));
                result = await _userManager.ResetPasswordAsync(user, code, request.NewPassword);
            }
            catch (FormatException)
            {

                result = IdentityResult.Failed(_userManager.ErrorDescriber.InvalidToken());
            }

            if (result.Succeeded)
                return Result.Success();

            var error = result.Errors.First();
            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status401Unauthorized));

        }
       







        private static string GenerateRefreshToken()
        {

            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        private async Task SendConfirmationEmail(ApplicationUser user,string code)
        {
            var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin;

            var emailBody = EmailBodyBuilder.GenerateEmailBody("EmailConfirmation",
                    new Dictionary<string, string>
                    {
                            { "{{name}}", user.FirstName },
                            { "{{action_url}}", $"{origin}/auth/emailConfirmation?userId={user.Id}&code={code}" }
                    }
                );

            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(user.Email!, "✅ Survey Basket: Email Confirmation", emailBody));

            await Task.CompletedTask;
        }

        private async Task SendResetPasswordEmail(ApplicationUser user, string code)
        {
            // Frontend domain name is stored in the Origin header of the request, we can use it to generate the reset password link
            var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin;

            // 
            var emailBody = EmailBodyBuilder.GenerateEmailBody("ForgetPassword",
                    new Dictionary<string, string>
                    {
                            { "{{name}}", user.FirstName },
                            { "{{action_url}}", $"{origin}/auth/forgetPassword?email={user.Email}&code={code}" }
                    }
                );

            // Use Hangfire to enqueue the email sending task to be executed in the background
            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(user.Email!, "✅ Survey Basket: Change Password", emailBody));
            
            await Task.CompletedTask;
        }

        private async Task<(IEnumerable<string> roles,IEnumerable<string> permissions)> GetUserRolesAndPermissions(ApplicationUser user,CancellationToken cancellationToken)
        {
            // 1 select roles
            var userRoles = await _userManager.GetRolesAsync(user);

            // 2 select permissions (there are many ways to do this, e.g., using a custom method or querying the database directly)

            //var userPermissions = await _context.Roles
            //    .Join(_context.RoleClaims,
            //    role => role.Id,
            //    claim => claim.RoleId,
            //    (role, claim) => new { role, claim }
            //    )
            //    .Where(x => userRoles.Contains(x.role.Name!))
            //    .Select(x => x.claim.ClaimValue!)
            //    .Distinct() // remove duplicates if user has multiple roles with the same permission
            //    .ToListAsync(cancellationToken);

            var userPermissions = await (
                                         from r in _context.Roles
                                         join p in _context.RoleClaims
                                         on r.Id equals p.RoleId
                                         where userRoles.Contains(r.Name!)
                                         select p.ClaimValue!
                                        )
                                        .Distinct()
                                        .ToListAsync(cancellationToken);


            return (userRoles, userPermissions);
        }

    }
}
