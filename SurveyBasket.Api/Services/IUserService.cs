using SurveyBasket.Api.Contracts.User;

namespace SurveyBasket.Api.Services;

public interface IUserService
{
    Task<Result<UserProfileResponse>> GetProfileAsync(string userId , CancellationToken cancellationToken = default);
    Task<Result> UpdateProfileAsync(string userId, UpdateProfileRequest request);
    Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request);

    Task<IEnumerable<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<UserResponse>> GetAsync(string id, CancellationToken cancellationToken = default);

}
