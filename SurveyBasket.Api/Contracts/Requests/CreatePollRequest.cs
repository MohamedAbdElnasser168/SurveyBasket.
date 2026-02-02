
namespace SurveyBasket.Api.Contracts.Requests
{
    public record CreatePollRequest
    (
        // data annotations can be used for validation

        //[Required(ErrorMessage ="Required field!")]
        string? Title ,
        string? Description 
    );
    
}
