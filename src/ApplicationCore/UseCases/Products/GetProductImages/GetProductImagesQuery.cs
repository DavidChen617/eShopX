namespace ApplicationCore.UseCases.Products.GetProductImages;

public record GetProductImagesQuery(Guid ProductId) : IRequest<GetProductImagesResponse>;