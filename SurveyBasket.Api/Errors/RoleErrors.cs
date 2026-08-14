namespace SurveyBasket.Api.Errors;

public static class RoleErrors
{
    public static readonly Error RoleNotFound = new("Role.NotFound", "The role was not found.",StatusCodes.Status404NotFound);
    
    public static readonly Error DublicatedRole = new("Role.AlreadyExists", "The role already exists.", StatusCodes.Status409Conflict);

    public static readonly Error InvalidPermissions = new("Role.InvalidPermissions", "The provided permissions are invalid.", StatusCodes.Status400BadRequest); 
    
    public static readonly Error FailedToUpdateRole = new("Role.FailedToUpdate", "Failed to update the role.", StatusCodes.Status400BadRequest);
}
