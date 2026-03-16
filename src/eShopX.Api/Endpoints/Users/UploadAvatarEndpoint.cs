using eShopX.Application.UseCases.Users;

namespace eShopX.Endpoints.Users;

public sealed class UploadAvatarEndpoint : IGroupedEndpoint<UsersGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/me/avatar", Handle)
            .Produces<ApiResponse<UpdateUserAvatarResponse>>(200)
            .Produces<ApiResponse>(404)
            .Produces<ApiResponse>(400)
            .DisableAntiforgery()
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        IFormFile file,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        if (file.Length == 0)
            return Results.BadRequest(new { code = "empty_file", message = "File is empty." });

        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await using var stream = file.OpenReadStream();

        var result = await dispatcher.Send(
            new UpdateUserAvatarCommand(userId, file.FileName, stream), ct);

        return result.ToHttpResult();
    }
}
