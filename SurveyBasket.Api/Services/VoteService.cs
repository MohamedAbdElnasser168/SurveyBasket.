using Microsoft.EntityFrameworkCore;
using SurveyBasket.Api.Contracts.Questions;
using SurveyBasket.Api.Contracts.Votes;

namespace SurveyBasket.Api.Services
{
    public class VoteService(ApplicationDbContext context) : IVoteService
    {
        private readonly ApplicationDbContext _context = context;
        public async Task<Result> AddAsync(int pollId, string userId, VoteRequest request, CancellationToken cancellationToken = default)
        {
            // first we need to check if the user has already voted in the poll or not

            var hasVoted = await _context.Votes.AnyAsync(v => v.PollId == pollId && v.UserId == userId, cancellationToken: cancellationToken);
            if (hasVoted)
                return Result.Failure(VoteErrors.DuplicatedVote);

            // second we need to check if the poll exists or not
            var pollIsExists = await _context.Polls.AnyAsync(p => p.Id == pollId && p.IsPublished && p.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow) && p.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken: cancellationToken);
            if (!pollIsExists)
                return Result.Failure(PollErrors.PollNotFound);

            // get available questions from selected Poll
            var availableQuestions = await _context.Questions
                .Where(q=>q.PollId==pollId&&q.IsActive)
                .Select(q=>q.Id)
                .ToListAsync(cancellationToken);

            // compare questions with questions in request
            if (!request.Answers.Select(x => x.QuestionId).SequenceEqual(availableQuestions))
            {
                return Result.Failure(VoteErrors.InvalidQuestions);
            }

            // save in db

            var vote = new Vote
            {
                PollId = pollId,
                UserId = userId,
                Answers=request.Answers.Adapt<IEnumerable<VoteAnswer>>().ToList()
            };

            await _context.AddAsync(vote, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();


        }
    }
}
