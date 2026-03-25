

using SurveyBasket.Api.Contracts.Polls;
using System.Threading.Tasks;

namespace SurveyBasket.Api.Services
{
    public class PollService(ApplicationDbContext context ) : IPollService
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<IEnumerable<PollResponse>> GetAllAsync(CancellationToken cancellationToken = default)=>
              await _context.Polls
                .AsNoTracking()
                .ProjectToType<PollResponse>()
                .ToListAsync(cancellationToken);




        public async Task<IEnumerable<PollResponse>> GetCurrentAsync(CancellationToken cancellationToken = default) =>
            await _context.Polls
                .Where(p => p.IsPublished && p.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow) && p.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow))
                .AsNoTracking()
                .ProjectToType<PollResponse>()
                .ToListAsync(cancellationToken);
        




        public async Task<Result<PollResponse>> GetPollByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var poll = await _context.Polls.FindAsync(id, cancellationToken);
            if (poll is null)
            {
                return Result.Failure<PollResponse>(PollErrors.PollNotFound);
            }
            return Result.Success(poll.Adapt<PollResponse>());
        }




       // function to add a poll and return the added poll added poll should be returned as PollResponse
        public async Task<Result<PollResponse>> AddAsync(PollRequest pollRequest, CancellationToken cancellationToken = default)
        {
            var isExistingTitle = await _context.Polls.AnyAsync(p => p.Title == pollRequest.Title, cancellationToken);
            if (isExistingTitle)
                return Result.Failure<PollResponse>(PollErrors.DuplicatedPollTitle);

            var poll = pollRequest.Adapt<Poll>();
            await _context.Polls.AddAsync(poll, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(poll.Adapt<PollResponse>());


        }

        public async Task<Result> UpdateAsync(int id, PollRequest pollRequest, CancellationToken cancellationToken = default)
        {

            var isExistingTitle = await _context.Polls.AnyAsync(p => p.Title == pollRequest.Title && p.Id != id, cancellationToken);
            if (isExistingTitle)
                return Result.Failure<PollResponse>(PollErrors.DuplicatedPollTitle);


            // poll 
            var currentPoll = await _context.Polls.FindAsync(id, cancellationToken);
            if (currentPoll is null)
            {
                return Result.Failure(PollErrors.PollNotFound);
            }

            // update the poll by takeing the values from the poll request and modify poll then save the changes to the database
            currentPoll.Title = pollRequest.Title!;
            currentPoll.Summary = pollRequest.Summary;
            currentPoll.StartsAt = pollRequest.StartsAt;
            currentPoll.EndsAt = pollRequest.EndsAt;
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }


        public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var poll = await _context.Polls.FindAsync(id, cancellationToken);
            if (poll is null)
            {
                return Result.Failure(PollErrors.PollNotFound);

            }
            _context.Polls.Remove(poll);
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();

        }

        public async Task<Result> TogglePublishStatusAsync(int id, CancellationToken cancellationToken = default)
        {
            var poll = await _context.Polls.FindAsync(id, cancellationToken);
            if (poll is null)
            {
                return Result.Failure(PollErrors.PollNotFound);
            }

            poll.IsPublished = !poll.IsPublished;
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
