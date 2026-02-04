namespace ApplicationCore.UseCases.Carts.CheckoutPreview;

public class CheckoutPreviewHandler(IRepository<Cart> cartRepository)
    : IRequestHandler<CheckoutPreviewQuery, CheckoutPreviewResponse>
{
    public async Task<CheckoutPreviewResponse> Handle(CheckoutPreviewQuery query, CancellationToken cancellationToken = default)
    {
        var result = await cartRepository.QueryAsync(q => q
            .Where(c => c.UserId == query.UserId)
            .Select(c => new
            {
                Items = c.Items.Select(i => new CheckoutPreviewItemDto(
                    i.ProductId,
                    i.Product!.Name,
                    i.Product.Price,
                    i.Quantity,
                    i.Product.Price * i.Quantity,
                    i.Product.IsActive && i.Product.StockQuantity >= i.Quantity,
                    i.Product.StockQuantity
                )).ToList()
            }), cancellationToken);

        var cart = result.FirstOrDefault();

        if (cart is null || cart.Items.Count == 0)
        {
            return new CheckoutPreviewResponse([], 0, false);
        }

        var totalAmount = cart.Items
            .Where(i => i.IsAvailable)
            .Sum(i => i.Subtotal);

        var hasUnavailableItems = cart.Items.Any(i => !i.IsAvailable);

        return new CheckoutPreviewResponse(cart.Items, totalAmount, hasUnavailableItems);
    }
}
