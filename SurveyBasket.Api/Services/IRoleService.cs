using SurveyBasket.Api.Contracts.Roles;

namespace SurveyBasket.Api.Services;

public interface IRoleService
{
    Task<IEnumerable<RoleResponse>> GetAllAsync(bool? includeDisabled = false, CancellationToken cancellationToken = default);
    Task<Result<RoleDetailResponse>> GetAsync(string id);
    Task<Result<RoleDetailResponse>> AddAsync(RoleRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(string id, RoleRequest request, CancellationToken cancellationToken = default);
    Task<Result> ToggleIsDeletedAsync(string id, CancellationToken cancellationToken = default);
}
