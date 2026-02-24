using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SurveyBasket.Api.Abstractions;

namespace SurveyBasket.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService,IOptions<JwtOptions> Jwtoptions) : ControllerBase
    {
        //                                             .value
        private readonly JwtOptions _jwtOptions = Jwtoptions.Value;
        private readonly IAuthService _authService = authService;


        // Login using email and password to get jwt token and refresh token

        [HttpPost("")]
        public async Task<IActionResult> LoginAsync([FromBody]LoginRequest request,CancellationToken cancellationToken=default)
        {
            // For testing purpose only, to test global exception handling middleware
            // testing my GlobalExceptionHandler class by throwing an exception when test the endpoint"
            //throw new Exception("My Exception");

            var authResult = await _authService.GetTokenAsync(request.Email,request.Password, cancellationToken);
            // authResult is a Result<AuthResponse> object that contains the authentication result of the login operation.
            return authResult.IsSuccess
                 ? Ok(authResult.Value)
                 : authResult.ToProblem(StatusCodes.Status404NotFound);
        }


        // Generate new jwt token and refresh token using refresh token and revoke old refresh token 
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken = default)
        {

            var authResult = await _authService.GetRefreshTokenAsync(request.Token,request.RefreshToken);
            return authResult.IsSuccess
                 ? Ok(authResult.Value)
                 :authResult.ToProblem(StatusCodes.Status404NotFound);
        }


        // Revoke refresh token to prevent further use of it
        [HttpPut("revoke-refresh-token")]
        public async Task<IActionResult> RevokeAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken = default)
        {
            var isRevoked = await _authService.RevokeRefreshTokenAsync(request.Token, request.RefreshToken);
            return isRevoked.IsSuccess
                   ? Ok()
                   : isRevoked.ToProblem(StatusCodes.Status404NotFound);
        }
    }
}
