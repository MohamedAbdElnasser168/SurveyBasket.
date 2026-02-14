using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

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
            var authResult = await _authService.GetTokenAsync(request.Email,request.Password, cancellationToken);
            return authResult is null ? BadRequest("Ivalid Email Or Password") : Ok(authResult);
        }


        // Generate new jwt token and refresh token using refresh token and revoke old refresh token 
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken = default)
        {
            var authResult = await _authService.GetRefreshTokenAsync(request.Token,request.RefreshToken);
            return authResult is null ? BadRequest("Ivalid Token") : Ok(authResult);
        }


        // Revoke refresh token to prevent further use of it
        [HttpPut("revoke-refresh-token")]
        public async Task<IActionResult> RevokeAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken = default)
        {
            var isRevoked = await _authService.RevokeRefreshTokenAsync(request.Token, request.RefreshToken);
            return isRevoked ? Ok():BadRequest("Operation Faild");
        }
    }
}
