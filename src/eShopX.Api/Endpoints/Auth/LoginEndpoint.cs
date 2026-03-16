using eShopX.Application.UseCases.Auth;

namespace eShopX.Endpoints.Auth;

public sealed class LoginEndpoint : IGroupedEndpoint<AuthGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/login", Handle)
            .Produces<ApiResponse<LoginResponse>>(200)
            .Produces<ApiResponse>(400)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        LoginCommand command,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(command, ct);
        return result.ToHttpResult();
    }
}
