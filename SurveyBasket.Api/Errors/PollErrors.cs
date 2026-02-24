namespace SurveyBasket.Api.Errors
{
    public class PollErrors
    {
        public static readonly Error PollNotFound =
           new("Poll.NotFound", "No poll was found with the given id");

        public static readonly Error DuplicatedPollTitle =
          new("Poll.DuplicatedTitle", "Another poll  with the same title is already exists");

    }
}
