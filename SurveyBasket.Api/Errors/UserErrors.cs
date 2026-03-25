namespace SurveyBasket.Api.Errors
{
    public static class UserErrors
    {
        public static readonly Error InvalidCredentials = 
            new ("User.InvalidCredentials", "Invalid Email Or Password", StatusCodes.Status401Unauthorized);
        public static readonly Error InvalidJwtTokens =
            new("User.InvalidJwtTokens", "Invalid Tokens", StatusCodes.Status401Unauthorized);

        public static readonly Error InvalidRefreshTokens =
            new("User.InvalidRefreshTokens", "Invalid Tokens", StatusCodes.Status401Unauthorized);
    }
}
