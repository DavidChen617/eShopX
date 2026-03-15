using eShopX.Application.UseCases.Auth;

namespace eShopX.Endpoints.Auth;

public sealed class LogoutEndpoint : IGroupedEndpoint<AuthGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/logout", Handle)
            .RequireAuthorization()
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        LogoutRequest request,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new LogoutCommand(request.RefreshToken), ct);
        return result.ToHttpResult();
    }
}

public record LogoutRequest(string RefreshToken);
