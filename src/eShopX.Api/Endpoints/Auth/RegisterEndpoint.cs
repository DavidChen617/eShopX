using ApplicationCore.UseCases.Auth.Register;

namespace eShopX.Endpoints.Auth;

public class RegisterEndpoint : IGroupedEndpoint<AuthGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/register", Handle)
            .Accepts<RegisterUserCommand>(MediaTypeNames.Application.Json)
            .Produces<RegisterUserResponse>(StatusCodes.Status201Created)
            .WithDescription("註冊新使用者帳號");
    }

    private static async Task<IResult> Handle([FromBody] RegisterUserCommand command, ISender sender)
    {
        var result = await sender.Send(command);
        return Results.Created(
            $"/api/users/{result.UserId}",
            ApiResponse<RegisterUserResponse>.Success(result, "註冊成功", StatusCodes.Status201Created));
    }
}
