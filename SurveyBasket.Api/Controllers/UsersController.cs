using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SurveyBasket.Api.Authentication.FIlters;

namespace SurveyBasket.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;
    
    [HttpGet("")]
    [HasPermission(Permissions.GetUsers)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllAsync(cancellationToken);
        return Ok(users);
    }

    [HttpGet("{id}")]
    [HasPermission(Permissions.GetUsers)]
    public async Task<IActionResult> Get([FromRoute] string id, CancellationToken cancellationToken)
    {
        var result = await _userService.GetAsync(id, cancellationToken);
        return result.IsSuccess 
            ? Ok(result.Value) 
            : result.ToProblem();
    }

}
