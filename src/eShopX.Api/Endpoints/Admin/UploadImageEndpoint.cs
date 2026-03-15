using eShopX.Application.Interfaces;

namespace eShopX.Endpoints.Admin;

public sealed class UploadImageEndpoint : IGroupedEndpoint<AdminProductsGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/images/upload", Handle)
            .DisableAntiforgery()
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        IFormFile file,
        IImageStorage imageStorage,
        CancellationToken ct)
    {
        if (file.Length == 0)
            return Results.BadRequest(new { code = "empty_file", message = "File is empty." });

        await using var stream = file.OpenReadStream();
        var result = await imageStorage.UploadAsync(
            new ImageUploadRequest(file.FileName, stream),
            ct);

        return Results.Ok(result);
    }
}
