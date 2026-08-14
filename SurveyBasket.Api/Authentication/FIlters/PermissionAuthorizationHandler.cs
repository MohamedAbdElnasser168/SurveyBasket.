namespace SurveyBasket.Api.Authentication.FIlters;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var user = context.User.Identity;
        if (user is null || !user.IsAuthenticated)
            return;

        var hasPermission = context.User.Claims.Any(c => c.Value == requirement.Permission && c.Type == Permissions.Type);

        // same as above, but with a different approach

        //if (context.User.Identity is not { IsAuthenticated:true} || 
        //!context.User.Claims.Any(c => c.Value == requirement.Permission && c.Type == Permissions.Type))
        //return;


        // Add thr reequuirement to the Succeeded requirements if the user has the required permissionhkh 
        if (hasPermission )
        context.Succeed(requirement);
        
        return;

    }
}
