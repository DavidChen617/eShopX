using Application.UseCases.Users;

namespace eShopX.Endpoints.Users;

public sealed class GetMeEndpoint : IGroupedEndpoint<UsersGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/me", Handle)
            .Produces<ApiResponse<GetMeResponse>>(200)
            .Produces<ApiResponse>(404)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await dispatcher.Send(new GetMeQuery(userId), ct);
        return result.ToHttpResult();
    }
}
