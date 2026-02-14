namespace SurveyBasket.Api.Contracts.Authentication
{
    public record RefreshTokenRequest
        (
        string RefreshToken,
        string Token
        );
    
}
