
namespace SurveyBasket.Api.Authentication.FIlters;

public class PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) 
    : DefaultAuthorizationPolicyProvider(options)
{
    private readonly AuthorizationOptions _authorizationOptions = options.Value;
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    { 
        var policy = await base.GetPolicyAsync(policyName);
        if (policy is not null)
            return policy;
        // If the policy does not exist, create a new one with the permission requirement
        var permissionPolicy = new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();

        _authorizationOptions.AddPolicy(policyName, permissionPolicy);


        return permissionPolicy;
    }
}
