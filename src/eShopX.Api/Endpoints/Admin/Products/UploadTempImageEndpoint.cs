using eShopX.Application.Interfaces;

namespace eShopX.Endpoints.Admin.Products;

public sealed class UploadTempImageEndpoint : IGroupedEndpoint<AdminProductsGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/images", Handle)
            .Produces(200)
            .Produces<ApiResponse>(400)
            .DisableAntiforgery()
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        IFormFileCollection files,
        IImageStorage imageStorage,
        CancellationToken ct)
    {
        if (files.Count == 0)
            return Results.BadRequest(new { code = "empty_files", message = "No files provided." });

        var uploads = await Task.WhenAll(files.Select(async file =>
        {
            await using var stream = file.OpenReadStream();
            return await imageStorage.UploadAsync(new ImageUploadRequest(file.FileName, stream), ct);
        }));

        return Results.Ok(uploads.Select(r => new { r.FileName, r.Url, r.PublicId }));
    }
}
