namespace ApplicationCore.UseCases.Carts.GetCart;

public class GetCartHandler(IRepository<Cart> cartRepository)
    : IRequestHandler<GetCartQuery, GetCartResponse>
{
    public async Task<GetCartResponse> Handle(GetCartQuery query, CancellationToken cancellationToken = default)
    {
        var result = await cartRepository.QueryAsync(q => q
            .Where(c => c.UserId == query.UserId)
            .Select(c => new GetCartResponse(
                c.Items.Select(i => new CartItemDto(
                    i.ProductId,
                    i.Product!.Name,
                    i.Product.Price,
                    i.Quantity,
                    i.Product.Price * i.Quantity,
                    i.Product.StockQuantity,
                    i.Product.StockQuantity >= i.Quantity
                )).ToList(),
                c.Items.Sum(i => i.Product!.Price * i.Quantity),
                c.Items.Sum(i => i.Quantity)
            )), cancellationToken);

        return result.FirstOrDefault() ?? new GetCartResponse([], 0, 0);
    }
}