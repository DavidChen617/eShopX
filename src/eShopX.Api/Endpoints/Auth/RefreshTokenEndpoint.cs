
namespace eShopX.Endpoints.Auth;

public sealed class RefreshTokenEndpoint : IGroupedEndpoint<AuthGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/refresh", Handle)
            .Produces<ApiResponse<RefreshTokenResponse>>(200)
            .Produces<ApiResponse>(400)
            .Produces<ApiResponse>(404)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        RefreshTokenCommand command,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(command, ct);
        return result.ToHttpResult();
    }
}
