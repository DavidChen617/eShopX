using System.Text.Json;
using eShopX.Domain.Outbox;

namespace eShopX.Application.UseCases.Outbox;

public static class OutboxEventFactory
{
    public const string ProductUpsertEventType      = "product.upsert";
    public const string ProductDeleteEventType      = "product.delete";
    public const string OrderShippedEventType       = "order.shipped";
    public const string PaymentPaidEventType        = "payment.paid";
    public const string PaymentFailedEventType      = "payment.failed";
    public const string ShipmentCompletedEventType  = "shipment.completed";

    public static OutboxEvent CreateProductUpsert(Guid productId)
        => OutboxEvent.Create(ProductUpsertEventType, JsonSerializer.Serialize(new ProductOutboxPayload(productId)));

    public static OutboxEvent CreateProductDelete(Guid productId)
        => OutboxEvent.Create(ProductDeleteEventType, JsonSerializer.Serialize(new ProductOutboxPayload(productId)));

    public static OutboxEvent CreateOrderShipped(OrderShippedOutboxPayload payload)
        => OutboxEvent.Create(OrderShippedEventType, JsonSerializer.Serialize(payload));

    public static OutboxEvent CreatePaymentPaid(Guid orderId)
        => OutboxEvent.Create(PaymentPaidEventType, JsonSerializer.Serialize(new OrderIdOutboxPayload(orderId)));

    public static OutboxEvent CreatePaymentFailed(Guid orderId)
        => OutboxEvent.Create(PaymentFailedEventType, JsonSerializer.Serialize(new OrderIdOutboxPayload(orderId)));

    public static OutboxEvent CreateShipmentCompleted(Guid orderId)
        => OutboxEvent.Create(ShipmentCompletedEventType, JsonSerializer.Serialize(new OrderIdOutboxPayload(orderId)));
}

public record ProductOutboxPayload(Guid ProductId);
public record OrderIdOutboxPayload(Guid OrderId);

public record OrderShippedOutboxPayload(
    Guid OrderId,
    string UserEmail,
    string UserName,
    string LogisticsId,
    string LogisticsSubType,
    string? StoreName,
    string? StoreId,
    string? Address,
    string? ZipCode);
