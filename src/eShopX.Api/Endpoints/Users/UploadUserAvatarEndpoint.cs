using System.Security.Claims;

using ApplicationCore.Interfaces;
using ApplicationCore.UseCases.Users.UploadAvatar;

namespace eShopX.Endpoints.Users;

public class UploadUserAvatarEndpoint : IGroupedEndpoint<UsersGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/me/avatar", Handle)
            .Accepts<UploadUserAvatarRequest>(MediaTypeNames.Multipart.FormData)
            .Produces<UploadUserAvatarResponse>()
            .DisableAntiforgery()
            .WithDescription("上傳/更新使用者頭像");
    }

    private static async Task<IResult> Handle(
        ClaimsPrincipal user,
        [FromForm] UploadUserAvatarRequest request,
        ISender sender)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Json(
                ApiResponse.Error(StatusCodes.Status401Unauthorized, "Unauthorized"),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        await using var stream = request.File.OpenReadStream();
        var imageRequest = new ImageUploadRequest(
            stream,
            request.File.FileName,
            request.File.ContentType,
            request.File.Length);

        var result = await sender.Send(new UploadUserAvatarCommand(userId, imageRequest));
        return Results.Ok(ApiResponse<UploadUserAvatarResponse>.Success(result));
    }
}

public record UploadUserAvatarRequest(IFormFile File);
