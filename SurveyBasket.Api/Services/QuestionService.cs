using Microsoft.Extensions.Caching.Hybrid;
using SurveyBasket.Api.Contracts.Answers;
using SurveyBasket.Api.Contracts.Questions;

namespace SurveyBasket.Api.Services
{
    public class QuestionService(
        ApplicationDbContext context,
        HybridCache hybridCache,
        ILogger<QuestionService> logger) : IQuestionService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly HybridCache _hybridCache = hybridCache;
        private readonly ILogger<QuestionService> _logger = logger;

        public async Task<Result<IEnumerable<QuestionResponse>>> GetAllAsync(int pollId, CancellationToken cancellationToken = default)
        {
            // no need to check with findAsync because we only need to check if the poll exists or not, and we don't need to retrieve the poll entity from the database,
            // so we can use AnyAsync which is more efficient than findAsync

            var pollIsExists = await _context.Polls.AnyAsync(p => p.Id == pollId, cancellationToken: cancellationToken);
            if (!pollIsExists)
                return Result.Failure<IEnumerable<QuestionResponse>>(QuestionErrors.QuestionNotFound);

            // Retrieve all questions for the specified poll and map them to QuestionResponse

            //var questions = await _context.Questions
            //    .Where(q => q.PollId == pollId)
            //    .Include(q => q.Answers)
            //    .AsNoTracking()
            //    .ToListAsync(cancellationToken);


            var questions = await _context.Questions
                .Where(q => q.PollId == pollId)
                .Include(q => q.Answers)
                //.Select(q=> new QuestionResponse(
                //    q.Id,
                //    q.Content,
                //    q.Answers.Select(a=> new AnswerResponse(a.Id,a.Content))
                //    ))
                .ProjectToType<QuestionResponse>()
                .AsNoTracking()
                .ToListAsync(cancellationToken);


            return Result.Success<IEnumerable<QuestionResponse>>(questions);
        }

        public async Task<Result<IEnumerable<QuestionResponse>>> GetAvailableAsync(int pollId, string userId, CancellationToken cancellationToken = default)
        {
            // first we need to check if the user has already voted in the poll or not

            var hasVoted = await _context.Votes.AnyAsync(v => v.PollId == pollId && v.UserId == userId, cancellationToken: cancellationToken);
            if (hasVoted)
                return Result.Failure<IEnumerable<QuestionResponse>>(VoteErrors.DuplicatedVote);

            // second we need to check if the poll exists or not
            
            var pollIsExists = await _context.Polls.AnyAsync(p => p.Id == pollId 
            && p.IsPublished 
            && p.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow) 
            && p.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow)
            , cancellationToken: cancellationToken);
            
            if (!pollIsExists)
                return Result.Failure<IEnumerable<QuestionResponse>>(PollErrors.PollNotFound);

            var cacheKey = $"available_questions_{pollId}";


            //  
            var questions = await _hybridCache.GetOrCreateAsync<IEnumerable<QuestionResponse>>(
                cacheKey,
                async cacheEntry => await _context.Questions
                      .Where(q => q.PollId == pollId && q.IsActive)
                      .Include(q => q.Answers)
                      .Select(q => new QuestionResponse(
                       q.Id,
                       q.Content,
                       q.Answers.Where(a => a.IsActive).Select(a => new AnswerResponse(a.Id, a.Content))
                       ))
                       .AsNoTracking()
                       .ToListAsync(cancellationToken)
                 //,
                //new HybridCacheEntryOptions
                //{
                //   Expiration = TimeSpan.FromMinutes(30)
                //}

                );



            return Result.Success(questions!);

        }


        public async Task<Result<QuestionResponse>> GetByIdAsync(int pollId, int questionId, CancellationToken cancellationToken = default)
        {

            var question = await _context.Questions
                .Where(q => q.PollId == pollId && q.Id == questionId)
                .Include(q => q.Answers)
                // mapping using mapster
                .ProjectToType<QuestionResponse>()
                .AsNoTracking()
                .SingleOrDefaultAsync(cancellationToken);


            if (question is null)
                return Result.Failure<QuestionResponse>(QuestionErrors.QuestionNotFound);

            return Result.Success(question);

        }


        public async Task<Result<QuestionResponse>> AddAsync(int pollId, QuestionRequest request, CancellationToken cancellationToken = default)
        {
            // no need to check with findAsync because we only need to check if the poll exists or not, and we don't need to retrieve the poll entity from the database,
            // so we can use AnyAsync which is more efficient than findAsync
            var pollIsExists = await _context.Polls.AnyAsync(p => p.Id == pollId, cancellationToken: cancellationToken);
            if (!pollIsExists)
                return Result.Failure<QuestionResponse>(QuestionErrors.QuestionNotFound);

            // Check if the question already exists in the poll with the same content
            var questionIsExists = await _context.Questions.AnyAsync(q => q.Content == request.Content && q.PollId == pollId, cancellationToken: cancellationToken);
            if (questionIsExists)
                return Result.Failure<QuestionResponse>(QuestionErrors.DuplicatedQuestionContent);


            // Create a new question entity and add it to the database 

            var question = request.Adapt<Question>();

            // Set the pollId for the question from the route parameter
            question.PollId = pollId;

            // Add answers to the question  , request.Answers is a list of strings that represent the content of the answers,
            // so we need to create a new answer entity for each answer and add it to the question's answers collection
            //request.Answers.ForEach(answer =>
            //{
            //    question.Answers.Add(new Answer
            //    {
            //        Content = answer
            //    });
            //});


            await _context.AddAsync(question, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await _hybridCache.RemoveAsync($"available_questions_{pollId}", cancellationToken);

            return Result.Success(question.Adapt<QuestionResponse>());
        }

        public async Task<Result> UpdateAsync(int pollId, int questionId, QuestionRequest request, CancellationToken cancellationToken = default)
        {
            // first we need to check if the question exists with the same content in the poll or not.

            // هل في سؤال تاني بنفس المحتوى في نفس الاستبيان غير السؤال اللي انا عايز احدثه ؟

            var contentIsExists = await _context.Questions.
                AnyAsync(q => q.Content == request.Content &&
                q.PollId == pollId &&
                q.Id != questionId,
                cancellationToken: cancellationToken);

            if (contentIsExists)
                return Result.Failure(QuestionErrors.DuplicatedQuestionContent);



            // second we will find the question
            var question = await _context.Questions
                .Include(q => q.Answers)
                .SingleOrDefaultAsync(q => q.Id == questionId && q.PollId == pollId, cancellationToken);

            if (question is null)
                return Result.Failure(QuestionErrors.QuestionNotFound);

            question.Content = request.Content;

            // 4 steps to update the answers of the question :


            // first step
            // get current answers in the question
            var currentAnswers = question.Answers.Select(x => x.Content).ToList();

            // second step
            // get current answers in the request by using except to get the new answers that are not exist in the database
            var newAnswers = request.Answers.Except(currentAnswers).ToList();


            // third step
            newAnswers.ForEach(answer =>
            {
                // add the new answer to the question's answers collection
                question.Answers.Add(new Answer
                {
                    Content = answer
                });
            });

            // currentAnswers = [a1,a2,a3] we will decide what to do with those answers that are exist in the database
            // but not exist in the request ,
            // we have 2 options either we will keep them or
            // we will keep them in the database and just set IsActive to false


            // fourth step
            question.Answers.ToList().ForEach(answer =>
            {
                if (!request.Answers.Contains(answer.Content))
                {
                    // if the answer is not exist in the request we will set IsActive to false
                    answer.IsActive = false;

                }
            });


            await _context.SaveChangesAsync(cancellationToken);

            await _hybridCache.RemoveAsync($"available_questions_{pollId}", cancellationToken);

            return Result.Success();

        }

        public async Task<Result> ToggeleIsActive(int pollId, int questionId, CancellationToken cancellationToken = default)
        {
            // first we need to check if the question exists or not, if it doesn't exist we will return a failure result with the appropriate error message
            //var question = await _context.Questions
            //    .Where(q => q.PollId == pollId && q.Id == questionId)
            //    .SingleOrDefaultAsync(cancellationToken);

            var question = await _context.Questions
              .SingleOrDefaultAsync(q => q.PollId == pollId && q.Id == questionId, cancellationToken);


            if (question is null)
                return Result.Failure(QuestionErrors.QuestionNotFound);

            // if the question exists we will toggle the IsActive property and save the changes to the database

            question.IsActive = !question.IsActive;
            await _context.SaveChangesAsync(cancellationToken);

            await _hybridCache.RemoveAsync($"available_questions_{pollId}", cancellationToken);

            return Result.Success();
        }


    }
}
