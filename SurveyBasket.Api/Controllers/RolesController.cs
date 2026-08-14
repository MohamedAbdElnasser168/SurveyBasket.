using SurveyBasket.Api.Authentication.FIlters;
using SurveyBasket.Api.Contracts.Roles;

namespace SurveyBasket.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RolesController(IRoleService roleService) : ControllerBase
{
    private readonly IRoleService _roleService = roleService;

    [HttpGet("")]
    [HasPermission(Permissions.GetRoles)]
    public async Task<IActionResult> GetAll([FromQuery] bool? includeDisabled, CancellationToken cancellationToken)
    {
        var roles = await _roleService.GetAllAsync(includeDisabled, cancellationToken);
        return Ok(roles);
    }

    [HttpGet("{id}")]
    [HasPermission(Permissions.GetRoles)]
    public async Task<IActionResult> Get([FromRoute] string id)
    {
        var result = await _roleService.GetAsync(id);
        return result.IsSuccess
            ? Ok(result.Value) 
            : result.ToProblem();
    }

    [HttpPost("")]
    [HasPermission(Permissions.AddRoles)]
    public async Task<IActionResult> Add([FromBody] RoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _roleService.AddAsync(request, cancellationToken);
       
        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value!.Id }, result.Value)
            : result.ToProblem();
    }


    [HttpPut("{id}")]
    [HasPermission(Permissions.UpdatePolls)]
    public async Task<IActionResult> Update([FromRoute] string id, [FromBody] RoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _roleService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }


    [HttpPatch("{id}")]
    [HasPermission(Permissions.UpdatePolls)]
    public async Task<IActionResult> ToggleIsDeleted([FromRoute] string id, CancellationToken cancellationToken)
    {
        var result = await _roleService.ToggleIsDeletedAsync(id, cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }
}
