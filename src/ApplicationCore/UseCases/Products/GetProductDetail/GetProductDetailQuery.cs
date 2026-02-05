namespace ApplicationCore.UseCases.Products.GetProductDetail;

public record GetProductDetailQuery(Guid ProductId) : IRequest<GetProductDetailResponse>;