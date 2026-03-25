using SurveyBasket.Api.Contracts.Results;

namespace SurveyBasket.Api.Services
{
    public class ResultService(ApplicationDbContext context) : IResultService
    {
        private readonly ApplicationDbContext _contex = context;
        public async Task<Result<PollVotesResponse>> GetPollVotesAsync(int pollId, CancellationToken cancellationToken = default)
        {
            // Check if the poll exists
            var pollExists = await _contex.Polls.AnyAsync(p => p.Id == pollId, cancellationToken);
            if (!pollExists)
                return Result.Failure<PollVotesResponse>(PollErrors.PollNotFound);

            var pollVotes = await _contex.Polls
                .Where(p => p.Id == pollId && p.IsPublished)
                .Select(p => new PollVotesResponse
                    (
                        p.Title,
                        p.Votes.Select(v => new VoteResponse(
                            $"{v.User.FirstName} {v.User.LastName}",
                            v.SubmittedOn,
                            v.Answers.Select(a => new QuestionAnswerResponse(
                                a.Question.Content,
                                a.Answer.Content
                                ))

                        ))
                    ))
                .SingleOrDefaultAsync(cancellationToken);



            return pollVotes is null
                ? Result.Failure<PollVotesResponse>(PollErrors.PollNotFound)
                : Result.Success(pollVotes);

        }

        public async Task<Result<IEnumerable<VotesPerDayResponse>>> GetVotesPerDayAsync(int pollId, CancellationToken cancellationToken = default)
        {
            // Check if the poll exists
            var pollExists = await _contex.Polls.AnyAsync(p => p.Id == pollId, cancellationToken);
            if (!pollExists)
                return Result.Failure<IEnumerable<VotesPerDayResponse>>(PollErrors.PollNotFound);


            var votesPerDay = await _contex.Votes
                .Where(v => v.PollId == pollId)
                .GroupBy(v => new { Date = DateOnly.FromDateTime(v.SubmittedOn) })
                .Select(g => new VotesPerDayResponse(

                    g.Key.Date,
                    g.Count()

                ))
                .ToListAsync(cancellationToken);

            return votesPerDay is null
                ? Result.Failure<IEnumerable<VotesPerDayResponse>>(PollErrors.PollNotFound)
                : Result.Success<IEnumerable<VotesPerDayResponse>>(votesPerDay);

        }

        public async Task<Result<IEnumerable<VotesPerQuestionResponse>>> GetVotesPerQuestionAsync(int pollId, CancellationToken cancellationToken = default)
        {
            // Check if the poll exists
            var pollExists = await _contex.Polls.AnyAsync(p => p.Id == pollId, cancellationToken);
            if (!pollExists)
                return Result.Failure<IEnumerable<VotesPerQuestionResponse>>(PollErrors.PollNotFound);



            var votesPerQuestion = await _contex.VoteAnswers
                .Where(v => v.Vote.PollId == pollId)
                .Select(v => new VotesPerQuestionResponse(
                    v.Question.Content,

                    v.Question.Votes
                    .GroupBy(v => new { Answers = v.Answer.Id, AnswerContent = v.Answer.Content })
                    .Select(g => new VotesPerAnswersResponse(
                        g.Key.AnswerContent,
                        g.Count()
                        ))

                    ))

                .ToListAsync(cancellationToken);


            return votesPerQuestion is null
               ? Result.Failure<IEnumerable<VotesPerQuestionResponse>>(PollErrors.PollNotFound)
               : Result.Success<IEnumerable<VotesPerQuestionResponse>>(votesPerQuestion);
        }
    }
}
