namespace SurveyBasket.Api.Contracts.Polls
{
    public record PollRequest
    (
        // data annotations can be used for validation

        //[Required(ErrorMessage ="Required field!")]
        string? Title ,
        string Summary,
        DateOnly StartsAt,
        DateOnly EndsAt
    
    );
    
}
