using eShopX.Application.UseCases.Auth;

namespace eShopX.Endpoints.Auth;

public sealed class RegisterEndpoint : IGroupedEndpoint<AuthGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/register", Handle)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        RegisterUserCommand command,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(command, ct);
        return result.ToHttpResult();
    }
}
