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
        // 1. 取得物流暫存資料（使用者在 ECPay 選完門市後寫入 Redis）
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
        //    skuQuantityMap 用於樂觀鎖衝突時重新套用扣減
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

        // 4. 建立訂單
        var order = Order.Create(command.UserId, orderItems);
        await orderRepository.AddAsync(order, cancellationToken);

        // 5. 向支付 gateway 發起付款請求，取得付款 URL
        //    此時尚未寫入 DB，若閘道失敗直接 return，庫存不受影響
        var paymentResult = await paymentGateway.RequestAsync(
            new PaymentGatewayRequest(command.PaymentMethod, order),
            cancellationToken);

        if (!paymentResult.IsSuccess)
            return Result<CreateOrderResponse>.BadRequest(
                new Error(paymentResult.ErrorCode!, paymentResult.ErrorMessage!));

        // 6. 建立付款記錄
        var payment = Payment.Create(order.Id, command.PaymentMethod, order.TotalAmount, paymentResult.PaymentUrl);
        await paymentRepository.AddAsync(payment, cancellationToken);

        // 7. 建立物流記錄（CVS 門市 or 宅配）
        var receiver = new ReceiverInfo(command.ReceiverName, command.ReceiverPhone);
        var isCvs = CvsTypes.Contains(logisticsSubType);

        Shipment shipment = isCvs
            ? CVSShipment.Create(order.Id, logistics.TempLogisticsID, logisticsSubType,
                receiver, logistics.ReceiverStoreID!, logistics.ReceiverStoreName!)
            : HomeShipment.Create(order.Id, logistics.TempLogisticsID, logisticsSubType,
                receiver, logistics.ReceiverZipCode!, logistics.ReceiverAddress!, null);

        await shipmentRepository.AddAsync(shipment, cancellationToken);

        // 8. 清空購物車
        cart.Clear();
        cartRepository.Update(cart);

        // 9. 一次寫入 DB：庫存扣減、訂單、付款、物流、購物車清空
        //    若 SKU 發生樂觀鎖衝突（xmin），EfUnitOfWork 會 reload 最新庫存並拋 ConcurrencyException
        //    catch 後重新套用 DeductStock，最多重試 3 次
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
        }

        // 10. 清除 Redis 快取（購物車、物流選擇）
        await cacher.RemoveAsync(CartCacheKeys.Cart(command.UserId), cancellationToken);
        await cacher.RemoveAsync(LogisticsCacheKeys.UserLogistics(command.UserId), cancellationToken);

        return Result<CreateOrderResponse>.Ok(
            new CreateOrderResponse(order.Id, order.TotalAmount.Amount, payment.PaymentUrl, order.CreatedAt));
    }
}
