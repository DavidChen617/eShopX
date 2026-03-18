using CoreMesh.Result;
using CoreMesh.Result.Extensions;

namespace eShopX.Endpoints.Admin.Products;

public sealed class UploadTempImageEndpoint : IGroupedEndpoint<AdminProductsGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/images", Handle)
            .Produces<ApiResponse<IReadOnlyList<TempImageUploadResult>>>(200)
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
            return Result<IReadOnlyList<TempImageUploadResult>>.BadRequest(
                new Error("empty_files", "No files provided.")).ToHttpResult();

        var uploads = await Task.WhenAll(files.Select(async file =>
        {
            await using var stream = file.OpenReadStream();
            var r = await imageStorage.UploadAsync(new ImageUploadRequest(file.FileName, stream), ct);
            return new TempImageUploadResult(r.FileName, r.Url, r.PublicId);
        }));

        return Result<IReadOnlyList<TempImageUploadResult>>.Ok(uploads).ToHttpResult();
    }
}

public record TempImageUploadResult(string FileName, string Url, string PublicId);
