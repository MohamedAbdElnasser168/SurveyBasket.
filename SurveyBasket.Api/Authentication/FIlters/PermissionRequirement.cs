namespace SurveyBasket.Api.Authentication.FIlters;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
