using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using eShopX.Application.Interfaces.Repositories;
using eShopX.Domain.Aggregates.Users;

namespace eShopX.Application.UseCases.Users;

public record GetMeQuery(Guid UserId) : IRequest<Result<GetMeResponse>>;

public record GetMeResponse(
    Guid Id,
    string Name,
    string Email,
    string? AvatarUrl,
    IReadOnlyList<string> Roles,
    bool IsLocalUser);

public class GetMeHandler(
    IUserRepository userRepository) : IRequestHandler<GetMeQuery, Result<GetMeResponse>>
{
    public async Task<Result<GetMeResponse>> Handle(
        GetMeQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(query.UserId, cancellationToken);
        if (user is null)
            return Result<GetMeResponse>.NotFound(new Error("user_not_found", "User not found."));

        var roles = Enum.GetValues<Role>()
            .Where(r => user.Roles.HasFlag(r))
            .Select(r => r.ToString())
            .ToList();

        var isLocalUser = user.AuthProviders.Any(p => p.Provider == Provider.Local);

        return Result<GetMeResponse>.Ok(new GetMeResponse(
            user.Id,
            user.Name,
            user.Email,
            user.Avatar?.Url,
            roles,
            isLocalUser));
    }
}
