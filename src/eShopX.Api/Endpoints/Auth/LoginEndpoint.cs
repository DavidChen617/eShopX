using ApplicationCore.UseCases.Auth.Login;

namespace eShopX.Endpoints.Auth;

public class LoginEndpoint : IGroupedEndpoint<AuthGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/login", Handle)
            .Accepts<LoginCommand>(MediaTypeNames.Application.Json)
            .Produces<LoginResponse>()
            .WithDescription("使用 Email 和密碼登入，取得 JWT Token");
    }

    private static async Task<IResult> Handle([FromBody] LoginCommand command, ISender sender)
    {
        var result = await sender.Send(command);
        return Results.Ok(ApiResponse<LoginResponse>.Success(result, "登入成功"));
    }
}
