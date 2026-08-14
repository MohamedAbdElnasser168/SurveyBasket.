using Microsoft.AspNetCore.Authorization.Infrastructure;
using SurveyBasket.Api.Contracts.Roles;
using System.Security.Claims;

namespace SurveyBasket.Api.Services;

public class RoleService(RoleManager<ApplicationRole> roleManager,
    ApplicationDbContext context): IRoleService
{
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<RoleResponse>> GetAllAsync(bool? includeDeleted = false, CancellationToken cancellationToken = default) =>
        await _roleManager.Roles            // = includeDisabled == true   ,, includeDisabled.HasValue means that the user has passed a value for includeDisabled, and includeDisabled.Value means that the value is true
              .Where(x => !x.IsDefault && ((includeDeleted.HasValue && includeDeleted.Value) || !x.IsDeleted))
              .AsNoTracking()
              .ProjectToType<RoleResponse>()
              .ToListAsync(cancellationToken);




    public async Task<Result<RoleDetailResponse>> GetAsync(string id)
    {
        if (await _roleManager.FindByIdAsync(id) is not { } role)
            return Result.Failure<RoleDetailResponse>(RoleErrors.RoleNotFound);

        var permissions = await _roleManager.GetClaimsAsync(role);

        var response = new RoleDetailResponse(role.Id, role.Name!, role.IsDeleted, permissions.Select(p => p.Value));
        
        return Result.Success(response);
    }



    public async Task<Result<RoleDetailResponse>> AddAsync(RoleRequest request,CancellationToken cancellationToken = default)
    {

        var roleIsExists = await _roleManager.RoleExistsAsync(request.Name);
        if (roleIsExists)
            return Result.Failure<RoleDetailResponse>(RoleErrors.DublicatedRole);

        var allowedPermissions = Permissions.GetAllPermissions();


        // if he removed the allowed permissions and still has some permissions that are not allowed, return error
        if (request.Permissions.Except(allowedPermissions).Any())
            return Result.Failure<RoleDetailResponse>(RoleErrors.InvalidPermissions);

        // add the role table
        var role = new ApplicationRole
        {
            Name = request.Name,
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };

        var result = await _roleManager.CreateAsync(role);

        // add the role claims table after adding the role, if the role is added successfully, add the permissions to the role claims table
        if (result.Succeeded)
        {
            var permissions = request.Permissions
                .Select(p => new IdentityRoleClaim<string>
                {
                    ClaimType = Permissions.Type,
                    ClaimValue = p,
                    RoleId = role.Id
                });
        
            await _context.RoleClaims.AddRangeAsync(permissions, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new RoleDetailResponse(role.Id, role.Name, role.IsDeleted,request.Permissions);
            return Result.Success(response);
        }

        var error = result.Errors.First();

        return Result.Failure<RoleDetailResponse>(new Error(error.Code,error.Description,StatusCodes.Status400BadRequest));
    }


    public async Task<Result> UpdateAsync(string id, RoleRequest request, CancellationToken cancellationToken = default)
    {

        // first check if the new name is already exists in our roles, if yes return error
        var roleIsExists = await _roleManager.Roles.AnyAsync(x => x.Name == request.Name && x.Id != id, cancellationToken);
        if (roleIsExists)
            return Result.Failure<RoleDetailResponse>(RoleErrors.DublicatedRole);

        // check if the role exists, if not return error
        if (await _roleManager.FindByIdAsync(id) is not { } role)
            return Result.Failure<RoleDetailResponse>(RoleErrors.RoleNotFound);

        // check if the permissions are valid, if not return error
        var allowedPermissions = Permissions.GetAllPermissions();
        // if he removed the allowed permissions and still has some permissions that are not allowed, return error
        if (request.Permissions.Except(allowedPermissions).Any())
            return Result.Failure<RoleDetailResponse>(RoleErrors.InvalidPermissions);


        role.Name = request.Name;
        var result = await _roleManager.UpdateAsync(role);

        if (result.Succeeded)
        {
            // permissions for the selected role, we will compare the current permissions with the new permissions and add or remove the permissions accordingly
            var currentPermissions = await _context.RoleClaims
                .Where(x => x.RoleId == role.Id && x.ClaimType == Permissions.Type)
                .Select(x => x.ClaimValue)
                .ToListAsync(cancellationToken);
            // permissions suppused to be added
            var newPermissions = request.Permissions
                .Except(currentPermissions)
                .Select(p => new IdentityRoleClaim<string>
                {
                    ClaimType = Permissions.Type,
                    ClaimValue = p,
                    RoleId = role.Id
                });

            var removedPermissions = currentPermissions.Except(request.Permissions);

            await _context.RoleClaims
                .Where(x => x.RoleId == role.Id && removedPermissions.Contains(x.ClaimValue))
                .ExecuteDeleteAsync(cancellationToken);

            await _context.RoleClaims.AddRangeAsync(newPermissions, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        /* Except explaination
         * 
         * OLD:
                A B C

           NEW:
                A C D E
           NEW.Except(OLD)
                   ↓
                  D E     new permissions to be added


           OLD.Except(NEW)
                    ↓
                    B     deleted permissions to be removed



           OLD ∩ NEW
                    ↓
                   A C    remaining permissions that are not changed

         */

        /* Flowchart for Update Role
         * Update Role
             │
             ├── الاسم موجود عند Role تانية؟
             │       └── نعم → Error
             │
             ├── الـ Role موجودة؟
             │       └── لا → Error
             │
             ├── الـ Permissions كلها مسموحة؟
             │       └── لا → Error
             │
             ├── Update Role Name
             │
             ├── Get OLD Permissions
             │
             ├── NEW - OLD
             │       └── Permissions → ADD
             │
             ├── OLD - NEW
             │       └── Permissions → DELETE
             │
             └── SaveChanges
         */

        var error = result.Errors.First();

        return Result.Failure<RoleDetailResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));

    }


    public async Task<Result> ToggleIsDeletedAsync(string id, CancellationToken cancellationToken = default)
    {
        if(await _roleManager.FindByIdAsync(id) is not { } role)
            return Result.Failure<RoleDetailResponse>(RoleErrors.RoleNotFound);

        role.IsDeleted = !role.IsDeleted;
        
        var result = await _roleManager.UpdateAsync(role);
        if (result.Succeeded)
            return Result.Success();

        return Result.Failure(RoleErrors.FailedToUpdateRole);

    }

}
