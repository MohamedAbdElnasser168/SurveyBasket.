namespace SurveyBasket.Api.Contracts.Authentication;

public record REsetPasswordRequest(
    string Email,
    string Code,
    string NewPassword

);
