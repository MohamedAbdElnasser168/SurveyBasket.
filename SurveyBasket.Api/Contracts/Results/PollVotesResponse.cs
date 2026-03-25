namespace SurveyBasket.Api.Contracts.Results
{
    public record PollVotesResponse
    (
        string Title,
        IEnumerable<VoteResponse> Votes
        );
}
/*
 data will be like this 
 {
    "title": "Poll Title",
    "votes": [
        {
            "voterName": "John Doe",
            "voteDate": "2024-06-01T12:00:00Z",
            "selectedAnswers": [
                {
                    "question": "Question 1",
                    "answer": "Answer A"
                },
                {
                    "question": "Question 2",
                    "answer": "Answer B"
                }
            ]
        },
        {
            "voterName": "Jane Smith",
            "voteDate": "2024-06-02T15:30:00Z",
            "selectedAnswers": [
                {
                    "question": "Question 1",
                    "answer": "Answer C"
                },
                {
                    "question": "Question 2",
                    "answer": "Answer D"
                }
            ]
        }
    ]
 
 
 */