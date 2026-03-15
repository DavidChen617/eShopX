using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using eShopX.Application.Interfaces;
using eShopX.Application.Interfaces.Repositories;
using eShopX.Application.UseCases.Carts;
using eShopX.Application.UseCases.Logistics;
using eShopX.Domain.Aggregates.Orders;
using eShopX.Domain.Aggregates.Payments;
using eShopX.Domain.Aggregates.Shipments;
using eShopX.Domain.ValueObjects;

namespace eShopX.Application.UseCases.Orders;

public record CreateOrderCommand(
    Guid UserId,
    PaymentMethod PaymentMethod,
    string? PaymentUrl,
    string LogisticsId,
    LogisticsSubType LogisticsSubType,
    string ReceiverName,
    string ReceiverPhone,
    // CVS
    string? StoreId,
    string? StoreName,
    // Home
    string? ZipCode,
    string? Address) : IRequest<Result<CreateOrderResponse>>;

public record CreateOrderResponse(Guid OrderId, decimal TotalAmount, string? PaymentUrl, DateTime CreatedAt);

public class CreateOrderHandler(
    ICartRepository cartRepository,
    IProductRepository productRepository,
    ISizeRepository sizeRepository,
    IOrderRepository orderRepository,
    IPaymentRepository paymentRepository,
    IShipmentRepository shipmentRepository,
    ICacher cacher,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateOrderCommand, Result<CreateOrderResponse>>
{
    private static readonly HashSet<LogisticsSubType> CvsTypes =
        [LogisticsSubType.UNIMART, LogisticsSubType.FAMI, LogisticsSubType.HILIFE];

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

            if (!skuDetails.Product.IsActive)
                return Result<CreateOrderResponse>.BadRequest(
                    new Error("product_inactive", $"Product '{skuDetails.Product.Name}' is no longer available."));

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

        var payment = Payment.Create(order.Id, command.PaymentMethod, order.TotalAmount, command.PaymentUrl);
        await paymentRepository.AddAsync(payment, cancellationToken);

        var receiver = new ReceiverInfo(command.ReceiverName, command.ReceiverPhone);
        Shipment shipment = CvsTypes.Contains(command.LogisticsSubType)
            ? CVSShipment.Create(order.Id, command.LogisticsId, command.LogisticsSubType,
                receiver, command.StoreId!, command.StoreName!)
            : HomeShipment.Create(order.Id, command.LogisticsId, command.LogisticsSubType,
                receiver, command.ZipCode!, command.Address!, null);

        await shipmentRepository.AddAsync(shipment, cancellationToken);

        cart.Clear();
        cartRepository.Update(cart);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cacher.RemoveAsync(CartCacheKeys.Cart(command.UserId), cancellationToken);
        await cacher.RemoveAsync(LogisticsCacheKeys.UserLogistics(command.UserId), cancellationToken);

        return Result<CreateOrderResponse>.Ok(
            new CreateOrderResponse(order.Id, order.TotalAmount.Amount, payment.PaymentUrl, order.CreatedAt));
    }
}
