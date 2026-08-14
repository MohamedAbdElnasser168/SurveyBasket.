namespace SurveyBasket.Api.Contracts.Roles;

public class RoleRequestValidator:AbstractValidator<RoleRequest>
{
    public RoleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .Length(3,100).WithMessage("Name cannot exceed 100 characters.");
    

        RuleFor(x => x.Permissions)
            .NotNull().WithMessage("Permission is required.")
            .NotEmpty().WithMessage("Permission cannot be empty.")
            .Must(x=>x.Distinct().Count() == x.Count()) 
            .WithMessage("Permission must be unique.")
            .When(x => x.Permissions != null);
    }
}
