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

        public static readonly Error DublicatedEmail =
           new("User.DublicatedEmail", "Another user with the same email is already exists", StatusCodes.Status409Conflict);

        public static readonly Error EmailNotConfirmed =
            new("User.EmailNotConfirmed", "Email is not confirmed", StatusCodes.Status401Unauthorized);

        public static readonly Error InvalidCodde =
            new("User.IvalidCode", "Invalid code", StatusCodes.Status401Unauthorized);

        public static readonly Error DublicatedConfirmation =
            new("User.DublicatedConfirmation", " Email already confirmed", StatusCodes.Status400BadRequest);


    }

}
