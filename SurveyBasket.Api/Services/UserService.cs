using SurveyBasket.Api.Contracts.User;

namespace SurveyBasket.Api.Services;

public class UserService(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext context
    ) : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ApplicationDbContext _context = context;

    // user methods to get user profile, update user profile, change password    
    public async Task<Result<UserProfileResponse>> GetProfileAsync(string userId,CancellationToken cancellationToken = default)
    {
        // 2 options: 1 - call ApplicationDbContext to get user profile, 2 - call userManger  to get user profile( it will get all user data  )

        // 2 - var user = await _userManager.FindByIdAsync(userId);


        // 1-  he will get the data that will be mapped to UserProfileResponse and not all user data that may be needed in other places,
        // so it will be more efficient and faster than the second option That happened because we used ProjectToType
        
        var user = await _userManager.Users
            .Where(u => u.Id == userId)
            .ProjectToType<UserProfileResponse>()
            .SingleAsync(cancellationToken);

        
        return Result.Success(user);

    }

    public async Task<Result> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        //var user = await _userManager.FindByIdAsync(userId);

        //user = request.Adapt(user);
        //await _userManager.UpdateAsync(user!);


        // we can use ExecuteUpdateAsync to update the user profile without getting the user data first, it will be more efficient and faster than the first option because
        // we will not get the user data that may be needed in other places, we will just update the data that we need to update
        await _userManager.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(setters => 
                setters
                .SetProperty(p => p.FirstName, request.FirstName)
                .SetProperty(p => p.LastName, request.LastName)
            );


        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    { 
        var user = await _userManager.FindByIdAsync(userId);
        // change password will check if the current password is correct or not,
        // if it is correct it will change the password to the new password, if it is not correct it will return an error
        var result = await _userManager.ChangePasswordAsync(user!, request.CurrentPassword, request.NewPassword);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();
            return Result.Failure(new Error(error.Code,error.Description,StatusCodes.Status400BadRequest));
        
    }



    // admin methods

    public async Task<IEnumerable<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await (from u in _context.Users
               join ur in _context.UserRoles
                    on u.Id equals ur.UserId
               join r in _context.Roles
                    on ur.RoleId equals r.Id into roles
               where !roles.Any(x => x.Name == DefaultRoles.Member)
               select new
                   {
                       u.Id,
                       u.FirstName,
                       u.LastName,
                       u.Email,
                       u.IsDisabled,
                       Roles = roles.Select(r => r.Name!).ToList()
                   }
               )
            .GroupBy(u => new { u.Id , u.FirstName, u.LastName, u.Email, u.IsDisabled })
            .Select(u=>new UserResponse
                (
                    u.Key.Id,
                    u.Key.FirstName,
                    u.Key.LastName,
                    u.Key.Email,
                    u.Key.IsDisabled,
                    u.SelectMany(x => x.Roles)
                )
            )
            .ToListAsync(cancellationToken);

    public async Task<Result<UserResponse>> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        if (await _userManager.FindByIdAsync(id) is not { } user)
            return Result.Failure<UserResponse>(UserErrors.UserNotFound);

        var roles = await _userManager.GetRolesAsync(user);

        // u can take data from multiple sources and map it to a single object using Mapster
        var response = (user, roles).Adapt<UserResponse>();
        // he will return the default value of the properties that are not mapped, so we need to configure the mapping  file

        return Result.Success(response);


    }

    public async Task<Result<CreateUserRequest>> AddAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        
    }
}
