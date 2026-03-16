using eShopX.Application.UseCases.Auth;

namespace eShopX.Endpoints.Auth;

public sealed class SendOtpEndpoint : IGroupedEndpoint<AuthGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/send-otp", Handle)
            .Produces<ApiResponse>(200)
            .Produces<ApiResponse>(400)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        SendOtpCommand command,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(command, ct);
        return result.ToHttpResult();
    }
}
