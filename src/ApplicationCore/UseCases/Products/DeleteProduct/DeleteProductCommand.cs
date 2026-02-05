namespace ApplicationCore.UseCases.Products.DeleteProduct;

public record DeleteProductCommand(Guid SellerId, Guid ProductId) : IRequest;