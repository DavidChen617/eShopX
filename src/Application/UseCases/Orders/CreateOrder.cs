using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using Application.UseCases.Carts;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Payments;
using Domain.Aggregates.Products;
using Domain.Aggregates.Shipments;
using Domain.ValueObjects;

namespace Application.UseCases.Orders;

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
        // 1. 取得物流暫存資料
        var logistics = await cacher.GetAsync<LogisticsCacheData>(
            LogisticsCacheKeys.UserLogistics(command.UserId), cancellationToken);

        if (logistics is null)
            return Result<CreateOrderResponse>.BadRequest(
                new Error("logistics_not_found", "請先完成物流選擇。"));

        if (!Enum.TryParse<LogisticsSubType>(logistics.LogisticsSubType, true, out var logisticsSubType))
            return Result<CreateOrderResponse>.BadRequest(
                new Error("invalid_logistics_subtype", "無效的物流類型。"));

        // 2. 驗證購物車不為空
        var cart = await cartRepository.GetByUserIdAsync(command.UserId, cancellationToken);
        if (cart is null || cart.Items.Count == 0)
            return Result<CreateOrderResponse>.BadRequest(new Error("cart_empty", "Cart is empty."));

        // 3. 逐一驗證 SKU、建立商品快照、扣庫存（in-memory）
        var orderItems = new List<(Guid SkuId, ProductSnapshot Snapshot, Money UnitPrice, int Quantity)>();
        var skuQuantityMap = new Dictionary<Guid, int>();

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

        // 4. 建立訂單、物流、付款記錄（PaymentUrl 暫為 null，gateway 呼叫前不寫 URL）
        var order = Order.Create(command.UserId, orderItems);
        await orderRepository.AddAsync(order, cancellationToken);

        var payment = Payment.Create(order.Id, command.PaymentMethod, order.TotalAmount, null);
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

        // 5. 寫入 DB（庫存扣減、訂單、付款、物流、購物車清空）
        //    在呼叫 gateway 之前先寫，確保 DB 一致性
        //    若 SKU 發生樂觀鎖衝突，最多重試 3 次；全部失敗則回傳錯誤，此時 gateway 尚未呼叫
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
                foreach (var staleSku in ex.StaleEntities.OfType<ProductSku>())
                {
                    if (skuQuantityMap.TryGetValue(staleSku.Id, out var qty))
                        staleSku.DeductStock(qty);
                }
            }
            catch (ConcurrencyException) when (attempt == maxRetries - 1)
            {
                return Result<CreateOrderResponse>.BadRequest(
                    new Error("stock_conflict", "庫存更新衝突，請重試。"));
            }
        }

        // 6. 呼叫 payment gateway 取得付款 URL
        var paymentResult = await paymentGateway.RequestAsync(
            new PaymentGatewayRequest(command.PaymentMethod, order),
            cancellationToken);

        if (!paymentResult.IsSuccess)
        {
            await cacher.RemoveAsync(CartCacheKeys.Cart(command.UserId), cancellationToken);
            return Result<CreateOrderResponse>.BadRequest(
                new Error(paymentResult.ErrorCode!, paymentResult.ErrorMessage!));
        }

        // 7. 更新付款 URL
        payment.SetPaymentUrl(paymentResult.PaymentUrl!);
        paymentRepository.Update(payment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 8. 清除 Redis（購物車、物流選擇）
        await cacher.RemoveAsync(CartCacheKeys.Cart(command.UserId), cancellationToken);
        await cacher.RemoveAsync(LogisticsCacheKeys.UserLogistics(command.UserId), cancellationToken);

        return Result<CreateOrderResponse>.Ok(
            new CreateOrderResponse(order.Id, order.TotalAmount.Amount, payment.PaymentUrl, order.CreatedAt));
    }
}
