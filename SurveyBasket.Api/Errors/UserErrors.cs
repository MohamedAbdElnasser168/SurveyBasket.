namespace SurveyBasket.Api.Errors
{
    public static class UserErrors
    {
        public static readonly Error InvalidCredentials = 
            new ("User.InvalidCredentials", "Invalid Email Or Password");
        public static readonly Error InvalidTokens =
            new("User.InvalidTokens", "Invalid Tokens");
    }
}
