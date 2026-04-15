using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SurveyBasket.Api.Abstractions;

namespace SurveyBasket.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService,ILogger<AuthController> logger) : ControllerBase
    {
        
        private readonly IAuthService _authService = authService;
        private readonly ILogger<AuthController> _logger = logger;

        // Login using email and password to get jwt token and refresh token

        [HttpPost("")]
        public async Task<IActionResult> LoginAsync([FromBody]LoginRequest request,CancellationToken cancellationToken=default)
        {
            // For testing purpose only, to test global exception handling middleware
            // testing my GlobalExceptionHandler class by throwing an exception when test the endpoint"
            //throw new Exception("My Exception");

            // Log the login attempt with the email address
            _logger.LogInformation("Login attempt for email: {email} and password:{password} ", request.Email,request.Password);


            var result = await _authService.GetTokenAsync(request.Email,request.Password, cancellationToken);
            // authResult is a Result<AuthResponse> object that contains the authentication result of the login operation.
            return result.IsSuccess
                 ? Ok(result.Value)
                 : result.ToProblem();
        }


        // Generate new jwt token and refresh token using refresh token and revoke old refresh token 
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken = default)
        {

            var result = await _authService.GetRefreshTokenAsync(request.Token,request.RefreshToken);
            return result.IsSuccess
                 ? Ok(result.Value)
                 :result.ToProblem();
        }


        // Revoke refresh token to prevent further use of it
        [HttpPut("revoke-refresh-token")]
        public async Task<IActionResult> RevokeAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _authService.RevokeRefreshTokenAsync(request.Token, request.RefreshToken);
            return result.IsSuccess
                   ? Ok()
                   : result.ToProblem();
        }
    }
}
