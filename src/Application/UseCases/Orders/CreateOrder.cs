using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using eShopX.Application.Interfaces;
using eShopX.Application.Interfaces.Repositories;
using eShopX.Application.UseCases.Carts;
using eShopX.Domain.Aggregates.Orders;
using eShopX.Domain.ValueObjects;

namespace eShopX.Application.UseCases.Orders;

public record CreateOrderCommand(Guid UserId) : IRequest<Result<CreateOrderResponse>>;

public record CreateOrderResponse(Guid OrderId, decimal TotalAmount, DateTime CreatedAt);

public class CreateOrderHandler(
    ICartRepository cartRepository,
    IProductRepository productRepository,
    ISizeRepository sizeRepository,
    IOrderRepository orderRepository,
    ICacher cacher,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateOrderCommand, Result<CreateOrderResponse>>
{
    public async Task<Result<CreateOrderResponse>> Handle(
        CreateOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var cart = await cartRepository.GetByUserIdAsync(command.UserId, cancellationToken);
        if (cart is null || cart.Items.Count == 0)
            return Result<CreateOrderResponse>.BadRequest(new Error("cart_empty", "Cart is empty."));

        var orderItems = new List<(Guid SkuId, ProductSnapshot Snapshot, Money UnitPrice, int Quantity)>();

        foreach (var cartItem in cart.Items)
        {
            var skuDetails = await productRepository.GetSkuDetailsAsync(cartItem.SkuId, cancellationToken);
            if (skuDetails is null)
                return Result<CreateOrderResponse>.BadRequest(
                    new Error("sku_not_found", $"SKU {cartItem.SkuId} not found."));

            string? sizeName = null;
            if (skuDetails.Sku.SizeId.HasValue)
            {
                var size = await sizeRepository.GetByIdAsync(skuDetails.Sku.SizeId.Value, cancellationToken);
                sizeName = size?.Name;
            }

            skuDetails.Sku.DeductStock(cartItem.Quantity);

            var snapshot = new ProductSnapshot(skuDetails.Product.Name, skuDetails.Variant.Color, sizeName);
            orderItems.Add((cartItem.SkuId, snapshot, skuDetails.Sku.Price, cartItem.Quantity));

            productRepository.Update(skuDetails.Product);
        }

        var order = Order.Create(command.UserId, orderItems);
        await orderRepository.AddAsync(order, cancellationToken);

        cart.Clear();
        cartRepository.Update(cart);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cacher.RemoveAsync(CartCacheKeys.Cart(command.UserId), cancellationToken);

        return Result<CreateOrderResponse>.Ok(
            new CreateOrderResponse(order.Id, order.TotalAmount.Amount, order.CreatedAt));
    }
}
