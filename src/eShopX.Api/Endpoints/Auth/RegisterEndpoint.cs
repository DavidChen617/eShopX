using eShopX.Application.UseCases.Auth;

namespace eShopX.Endpoints.Auth;

public sealed class RegisterEndpoint : IGroupedEndpoint<AuthGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/register", Handle)
            .Produces<ApiResponse<RegisterUserResponse>>(201)
            .Produces<ApiResponse>(400)
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
