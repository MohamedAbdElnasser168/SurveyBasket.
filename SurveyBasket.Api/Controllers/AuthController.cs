using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SurveyBasket.Api.Abstractions;

namespace SurveyBasket.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService, ILogger<AuthController> logger) : ControllerBase
    {

        private readonly IAuthService _authService = authService;
        private readonly ILogger<AuthController> _logger = logger;


        [HttpPost("")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken = default)
        {
            // For testing purpose only, to test global exception handling middleware
            // testing my GlobalExceptionHandler class by throwing an exception when test the endpoint"
            //throw new Exception("My Exception");

            // Log the login attempt with the email address
            _logger.LogInformation("Login attempt for email: {email} and password:{password} ", request.Email, request.Password);


            var result = await _authService.GetTokenAsync(request.Email, request.Password, cancellationToken);
            // authResult is a Result<AuthResponse> object that contains the authentication result of the login operation.
            return result.IsSuccess
                 ? Ok(result.Value)
                 : result.ToProblem();
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken = default)
        {

            var result = await _authService.GetRefreshTokenAsync(request.Token, request.RefreshToken);
            return result.IsSuccess
                 ? Ok(result.Value)
                 : result.ToProblem();
        }

        [HttpPut("revoke-refresh-token")]
        public async Task<IActionResult> RevokeAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _authService.RevokeRefreshTokenAsync(request.Token, request.RefreshToken);
            return result.IsSuccess
                   ? Ok()
                   : result.ToProblem();
        }

        [HttpPost("register")]
        public async Task<IActionResult> register([FromBody] RegisterRequest request, CancellationToken cancellationToken = default)
        {
            // For testing purpose only, to test global exception handling middleware
            // testing my GlobalExceptionHandler class by throwing an exception when test the endpoint"
            //throw new Exception("My Exception");

            // Log the login attempt with the email address
            _logger.LogInformation("Register attempt for email: {email} and password:{password} ", request.Email, request.Password);


            var result = await _authService.RegisterAsync(request, cancellationToken);
            // authResult is a Result<AuthResponse> object that contains the authentication result of the login operation.
            return result.IsSuccess
                 ? Ok()
                 : result.ToProblem();
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _authService.ConfirmEmailAsync(request, cancellationToken);
            return result.IsSuccess
                   ? Ok()
                   : result.ToProblem();
        }

        [HttpPost("resend-confirmation-email")]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationEmailRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _authService.ResendConfirmationEmailAsync(request, cancellationToken);
            return result.IsSuccess
                   ? Ok()
                   : result.ToProblem();
        }

        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequest request)
        {
            var result = await _authService.SendResetPasswordCodeAsync(request.Email);
            return result.IsSuccess
                   ? Ok()
                   : result.ToProblem();
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] REsetPasswordRequest request)
        {
            var result = await _authService.ResetPasswordAsync(request);
            return result.IsSuccess
                   ? Ok()
                   : result.ToProblem();
        }
    }
}
