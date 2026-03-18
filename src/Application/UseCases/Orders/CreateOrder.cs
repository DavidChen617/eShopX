using Application.UseCases.Carts;
using Application.UseCases.Products;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Payments;
using Domain.Aggregates.Products;
using Domain.ValueObjects;

namespace Application.UseCases.Orders;

public record CreateOrderCommand(
    Guid UserId,
    PaymentMethod PaymentMethod,
    string ReceiverName,
    string ReceiverPhone) : IRequest<Result<CreateOrderResponse>>;

public record CreateOrderResponse(
    Guid OrderId,
    decimal TotalAmount,
    string? PaymentUrl,
    DateTime CreatedAt,
    string LogisticsSubType,
    string? StoreName,
    string? Address);

public class CreateOrderHandler(
    ICartRepository cartRepository,
    IProductRepository productRepository,
    ISizeRepository sizeRepository,
    IOrderRepository orderRepository,
    IPaymentRepository paymentRepository,
    IShipmentRepository shipmentRepository,
    IShipmentFactory shipmentFactory,
    IPaymentGateway paymentGateway,
    ICacher cacher,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateOrderCommand, Result<CreateOrderResponse>>
{

    public async Task<Result<CreateOrderResponse>> Handle(
        CreateOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        // 1. 取得物流暫存資料
        var logistics = await cacher.GetAsync<LogisticsCacheData>(
            LogisticsCacheKeys.UserLogistics(command.UserId), cancellationToken);

        if (logistics is null)
            return Result<CreateOrderResponse>.BadRequest(
                new Error("logistics_not_found", "物流選擇已逾時，請重新選擇門市。"));


        // 2. 驗證購物車不為空
        var cart = await cartRepository.GetByUserIdAsync(command.UserId, cancellationToken);
        if (cart is null || cart.Items.Count == 0)
            return Result<CreateOrderResponse>.BadRequest(new Error("cart_empty", "Cart is empty."));

        // 3. 批次查詢 SKU、Size，再 in-memory 驗證與扣庫存
        var skuIds = cart.Items.Select(i => i.SkuId).ToList();
        var skuDetailsMap = (await productRepository.GetSkuDetailsByIdsAsync(skuIds, cancellationToken))
            .ToDictionary(d => d.Sku.Id);

        var sizeIds = skuDetailsMap.Values
            .Where(d => d.Sku.SizeId.HasValue)
            .Select(d => d.Sku.SizeId!.Value)
            .Distinct()
            .ToList();
        var sizeMap = (await sizeRepository.GetByIdsAsync(sizeIds, cancellationToken))
            .ToDictionary(s => s.Id);

        var orderItems = new List<(Guid SkuId, ProductSnapshot Snapshot, Money UnitPrice, int Quantity)>();
        var skuQuantityMap = new Dictionary<Guid, int>();

        foreach (var cartItem in cart.Items)
        {
            if (!skuDetailsMap.TryGetValue(cartItem.SkuId, out var skuDetails))
                return Result<CreateOrderResponse>.BadRequest(
                    new Error("sku_not_found", $"SKU {cartItem.SkuId} not found."));

            if (!skuDetails.Product.IsActive)
                return Result<CreateOrderResponse>.BadRequest(
                    new Error("product_inactive", $"Product '{skuDetails.Product.Name}' is no longer available."));

            if (skuDetails.Sku.StockQuantity < cartItem.Quantity)
                return Result<CreateOrderResponse>.BadRequest(
                    new Error("insufficient_stock", $"商品「{skuDetails.Product.Name}」庫存不足。"));

            string? sizeName = null;
            if (skuDetails.Sku.SizeId.HasValue && sizeMap.TryGetValue(skuDetails.Sku.SizeId.Value, out var size))
                sizeName = size.Name;

            skuDetails.Sku.DeductStock(cartItem.Quantity);
            skuQuantityMap[cartItem.SkuId] = cartItem.Quantity;

            var snapshot = new ProductSnapshot(skuDetails.Product.Name, skuDetails.Variant.Color, sizeName);
            orderItems.Add((cartItem.SkuId, snapshot, skuDetails.Sku.Price, cartItem.Quantity));
        }

        // 4. 建立訂單、物流、付款記錄（PaymentUrl 暫為 null，gateway 呼叫前不寫 URL）
        var order = Order.Create(command.UserId, orderItems);
        await orderRepository.AddAsync(order, cancellationToken);

        var payment = Payment.Create(order.Id, command.PaymentMethod, order.TotalAmount, null);
        await paymentRepository.AddAsync(payment, cancellationToken);

        var receiver = new ReceiverInfo(command.ReceiverName, command.ReceiverPhone);
        var shipment = shipmentFactory.Create(order.Id, logistics, receiver);
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
                    {
                        staleSku.DeductStock(qty);
                    }
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
            // 補償回滾：還原庫存、刪除訂單/付款/物流、還原購物車
            foreach (var (skuId, qty) in skuQuantityMap)
                if (skuDetailsMap.TryGetValue(skuId, out var sd))
                {
                    sd.Sku.AddStock(qty);
                }

            orderRepository.Delete(order);
            paymentRepository.Delete(payment);
            shipmentRepository.Delete(shipment);

            foreach (var item in orderItems)
                cart.AddItem(item.SkuId, item.Quantity);
            cartRepository.Update(cart);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            var rollbackProductIds = skuDetailsMap.Values.Select(d => d.Product.Id).Distinct();
            foreach (var productId in rollbackProductIds)
                await cacher.RemoveAsync(ProductCacheKeys.Product(productId), cancellationToken);

            return Result<CreateOrderResponse>.BadRequest(
                new Error(paymentResult.ErrorCode!, paymentResult.ErrorMessage!));
        }

        // 7. 更新付款 URL
        payment.SetPaymentUrl(paymentResult.PaymentUrl!);
        paymentRepository.Update(payment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 8. 清除 Redis（購物車、物流選擇、商品快取）
        await cacher.RemoveAsync(CartCacheKeys.Cart(command.UserId), cancellationToken);
        await cacher.RemoveAsync(LogisticsCacheKeys.UserLogistics(command.UserId), cancellationToken);

        var productIds = skuDetailsMap.Values.Select(d => d.Product.Id).Distinct();
        foreach (var productId in productIds)
            await cacher.RemoveAsync(ProductCacheKeys.Product(productId), cancellationToken);

        return Result<CreateOrderResponse>.Ok(
            new CreateOrderResponse(
                order.Id, order.TotalAmount.Amount, payment.PaymentUrl, order.CreatedAt,
                logistics.LogisticsSubType,
                logistics.ReceiverStoreName,
                logistics.ReceiverAddress));
    }
}
