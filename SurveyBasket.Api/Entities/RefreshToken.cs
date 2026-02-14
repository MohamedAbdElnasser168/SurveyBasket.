namespace SurveyBasket.Api.Entities
{
    [Owned] // This attribute indicates that the RefreshToken entity is
            // owned by another entity (e.g., User) and does not have its own identity.
    public class RefreshToken
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresOn { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        
        // revoke means that the token is no longer valid and cannot be used for authentication or authorization purposes.
        public DateTime? RevokedOn { get; set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresOn;
        public bool IsActive => RevokedOn is null && !IsExpired;

    }
}
