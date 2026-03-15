using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using eShopX.Application.Exceptions;
using eShopX.Application.Interfaces;
using eShopX.Application.Interfaces.Repositories;
using eShopX.Application.UseCases.Carts;
using eShopX.Application.UseCases.Logistics;
using eShopX.Domain.Aggregates.Orders;
using eShopX.Domain.Aggregates.Payments;
using eShopX.Domain.Aggregates.Products;
using eShopX.Domain.Aggregates.Shipments;
using eShopX.Domain.ValueObjects;

namespace eShopX.Application.UseCases.Orders;

public record CreateOrderCommand(
    Guid UserId,
    PaymentMethod PaymentMethod,
    string ReceiverName,
    string ReceiverPhone) : IRequest<Result<CreateOrderResponse>>;

public record CreateOrderResponse(Guid OrderId, decimal TotalAmount, string? PaymentUrl, DateTime CreatedAt);

public class CreateOrderHandler(
    ICartRepository cartRepository,
    IProductRepository productRepository,
    ISizeRepository sizeRepository,
    IOrderRepository orderRepository,
    IPaymentRepository paymentRepository,
    IShipmentRepository shipmentRepository,
    IPaymentGateway paymentGateway,
    ICacher cacher,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateOrderCommand, Result<CreateOrderResponse>>
{
    private static readonly HashSet<LogisticsSubType> CvsTypes =
        [LogisticsSubType.UNIMART, LogisticsSubType.FAMI, LogisticsSubType.HILIFE];

    public async Task<Result<CreateOrderResponse>> Handle(
        CreateOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var logistics = await cacher.GetAsync<LogisticsCacheData>(
            LogisticsCacheKeys.UserLogistics(command.UserId), cancellationToken);

        if (logistics is null)
            return Result<CreateOrderResponse>.BadRequest(
                new Error("logistics_not_found", "請先完成物流選擇。"));

        if (!Enum.TryParse<LogisticsSubType>(logistics.LogisticsSubType, true, out var logisticsSubType))
            return Result<CreateOrderResponse>.BadRequest(
                new Error("invalid_logistics_subtype", "無效的物流類型。"));

        var cart = await cartRepository.GetByUserIdAsync(command.UserId, cancellationToken);
        if (cart is null || cart.Items.Count == 0)
            return Result<CreateOrderResponse>.BadRequest(new Error("cart_empty", "Cart is empty."));

        var orderItems = new List<(Guid SkuId, ProductSnapshot Snapshot, Money UnitPrice, int Quantity)>();
        var skuQuantityMap = new Dictionary<Guid, int>(); // skuId → quantity for retry

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
            skuQuantityMap[cartItem.SkuId] = cartItem.Quantity;

            var snapshot = new ProductSnapshot(skuDetails.Product.Name, skuDetails.Variant.Color, sizeName);
            orderItems.Add((cartItem.SkuId, snapshot, skuDetails.Sku.Price, cartItem.Quantity));
            productRepository.Update(skuDetails.Product);
        }

        var order = Order.Create(command.UserId, orderItems);
        await orderRepository.AddAsync(order, cancellationToken);

        var paymentResult = await paymentGateway.RequestAsync(
            new PaymentGatewayRequest(command.PaymentMethod, order),
            cancellationToken);

        if (!paymentResult.IsSuccess)
            return Result<CreateOrderResponse>.BadRequest(
                new Error(paymentResult.ErrorCode!, paymentResult.ErrorMessage!));

        var payment = Payment.Create(order.Id, command.PaymentMethod, order.TotalAmount, paymentResult.PaymentUrl);
        await paymentRepository.AddAsync(payment, cancellationToken);

        var receiver = new ReceiverInfo(command.ReceiverName, command.ReceiverPhone);
        var isCvs = CvsTypes.Contains(logisticsSubType);

        Shipment shipment = isCvs
            ? CVSShipment.Create(order.Id, logistics.TempLogisticsID, logisticsSubType,
                receiver, logistics.ReceiverStoreID!, logistics.ReceiverStoreName!)
            : HomeShipment.Create(order.Id, logistics.TempLogisticsID, logisticsSubType,
                receiver, logistics.ReceiverZipCode!, logistics.ReceiverAddress!, null);

        await shipmentRepository.AddAsync(shipment, cancellationToken);

        cart.Clear();
        cartRepository.Update(cart);

        const int maxRetries = 3;
        for (var attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
                break;
            }
            catch (ConcurrencyException ex) when (attempt < maxRetries - 1)
            {
                // EfUnitOfWork already reloaded the stale SKU entries; re-apply stock deduction
                foreach (var staleSku in ex.StaleEntities.OfType<ProductSku>())
                {
                    if (skuQuantityMap.TryGetValue(staleSku.Id, out var qty))
                        staleSku.DeductStock(qty);
                }
            }
        }

        await cacher.RemoveAsync(CartCacheKeys.Cart(command.UserId), cancellationToken);
        await cacher.RemoveAsync(LogisticsCacheKeys.UserLogistics(command.UserId), cancellationToken);

        return Result<CreateOrderResponse>.Ok(
            new CreateOrderResponse(order.Id, order.TotalAmount.Amount, payment.PaymentUrl, order.CreatedAt));
    }
}
