namespace SurveyBasket.Api.Contracts.Polls
{
    public record PollResponse
    (
         // data annotations can be used for validation

         //[Required(ErrorMessage ="Required field!")]
        int Id,
        string? Title,
        string Summary,
        bool IsPublished,
        DateOnly StartsAt,
        DateOnly EndsAt,
        string CreatedById,
        DateTime CreatedOn,
        string? UpdatedById,
        DateTime? UpdatedOn


    );
}
