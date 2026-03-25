namespace SurveyBasket.Api.Abstractions
{
    public record Error(string Code,string Description,int? StatusCode)
    {
        // This is a static property that represents an error with no code and no description. It can be used as a default value when there is no specific error to report.
        public static readonly Error None= new(string.Empty,string.Empty,null);
    }
}
