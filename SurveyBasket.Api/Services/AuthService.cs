
using Microsoft.AspNetCore.Identity;
using SurveyBasket.Api.Abstractions;
using SurveyBasket.Api.Errors;
using System.Security.Cryptography;

namespace SurveyBasket.Api.Services
{
    // ApplicationUser is derived from IdentityUser class
    public class AuthService(UserManager<ApplicationUser> userManager,IJwtProvider jwtProvider) : IAuthService
    {
        private readonly IJwtProvider _jwtProvider = jwtProvider;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        private readonly int _refreshTokenExpireDays = 14; 

        public async Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            // check If i have user in db with given email  (Using UserManger(Best) Or ApplicationUser)
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                //return null;
                return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);
            }
            // check If password is correct
            var isValidPassword = await _userManager.CheckPasswordAsync(user, password);
            if (!isValidPassword)
            {
                return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);
            }
            // generate jwt token if password is correct (Using JwtSecurityTokenHandler Or Using JwtSecurityToken)
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






        // Helper method to generate a secure random refresh token
        private static string GenerateRefreshToken()
        {

            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

       
    }
}
