namespace SurveyBasket.Api.Authentication.FIlters;

public class HasPermissionAttribute (string permission) : AuthorizeAttribute(permission)
{

}
