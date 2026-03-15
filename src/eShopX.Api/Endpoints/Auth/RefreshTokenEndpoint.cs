using eShopX.Application.UseCases.Auth;

namespace eShopX.Endpoints.Auth;

public sealed class RefreshTokenEndpoint : IGroupedEndpoint<AuthGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/refresh", Handle);
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
