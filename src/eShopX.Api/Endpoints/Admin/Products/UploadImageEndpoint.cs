using eShopX.Application.UseCases.Products;

namespace eShopX.Endpoints.Admin.Products;

public sealed class UploadImageEndpoint : IGroupedEndpoint<AdminProductsGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/{productId:guid}/variants/{variantId:guid}/images", Handle)
            .Produces<ApiResponse<UploadProductImageResponse>>(200)
            .Produces<ApiResponse>(404)
            .Produces<ApiResponse>(400)
            .DisableAntiforgery()
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        Guid productId,
        Guid variantId,
        IFormFile file,
        [AsParameters] UploadImageParams p,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        if (file.Length == 0)
            return Results.BadRequest(new { code = "empty_file", message = "File is empty." });

        await using var stream = file.OpenReadStream();

        var result = await dispatcher.Send(
            new UploadProductImageCommand(productId, variantId, file.FileName, stream, p.IsPrimary, p.SortOrder),
            ct);

        return result.ToHttpResult();
    }
}

public record UploadImageParams(bool IsPrimary = false, int SortOrder = 0);
