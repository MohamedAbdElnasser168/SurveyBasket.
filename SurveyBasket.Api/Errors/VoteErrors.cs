namespace SurveyBasket.Api.Errors
{
    public class VoteErrors
    {

        public static readonly Error InvalidQuestions =
           new("Vote.InvalidQuestions", "Questions in request not the same in Database",StatusCodes.Status400BadRequest);

        public static readonly Error DuplicatedVote =
          new("Vote.DuplicatedVote", "This user already voted for this poll before",StatusCodes.Status409Conflict);
    }
}
