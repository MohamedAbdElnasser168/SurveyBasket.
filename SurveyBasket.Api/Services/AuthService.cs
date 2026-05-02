
using Microsoft.AspNetCore.Identity;
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
        IHttpContextAccessor httpContextAccessor ) : IAuthService
    {
        #region Fields

        private readonly IJwtProvider _jwtProvider = jwtProvider;
        private readonly IEmailSender _emailSender = emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly ILogger<AuthService> _logger = logger;
        private readonly int _refreshTokenExpireDays = 14;
        #endregion


        public async Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
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
                 return Result.Success();
            
            var error = result.Errors.First();
            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));


        }


        public async Task<Result> ResendConfirmationEmailAsync(ResendConfirmationEmailRequest request, CancellationToken cancellationToken = default)
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


        // login user and return jwt token if successful, otherwise return failure result with invalid credentials error
        public async Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default)
        {

            // check if the result of find user by email is not null and assign it to user variable, otherwise return failure result with invalid credentials error
            if (await _userManager.FindByEmailAsync(email) is not { } user)   
                return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);
           
            
            var result = await _signInManager.PasswordSignInAsync(user, password,false,false);

            if (result.Succeeded)
            {

                var (token, expiresIn) = _jwtProvider.GenerateToken(user);
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

            return Result.Failure<AuthResponse>(result.IsNotAllowed ? UserErrors.EmailNotConfirmed : UserErrors.InvalidCredentials);

        }




        // generate new jwt token and refresh token using refresh token and revoke the old refresh token 
        public async Task<Result<AuthResponse?>> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
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

            // Check if the refresh token is valid and active and get the refresh token from the database
            var userRefreshToken = user.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshToken&& rt.IsActive);

            if (userRefreshToken is null)
            {
                return Result.Failure<AuthResponse>(UserErrors.InvalidRefreshTokens)!;
            }

            // Revoke the old refresh token
            userRefreshToken.RevokedOn = DateTime.UtcNow;



            // Generate new JWT token
            var (newToken, expiresIn) = _jwtProvider.GenerateToken(user);
            

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

            await _emailSender.SendEmailAsync(user.Email!, "SurveyBasket : Email Confirmation", emailBody);
        }

       
    }
}
