using System.Text.Json;
using eShopX.Application.Interfaces;
using eShopX.Application.Interfaces.Repositories;
using eShopX.Application.UseCases.Outbox;
using eShopX.Domain.Aggregates.Shipments;

namespace Infrastructure.Messaging.Shipments;

public class ShipmentCompletedEmailHandler(
    IOrderRepository orderRepository,
    IUserRepository userRepository,
    IShipmentRepository shipmentRepository,
    IMailSender mailSender) : IOutboxEventHandler
{
    public bool CanHandle(string eventType) => eventType == OutboxEventFactory.ShipmentCompletedEventType;

    public async Task HandleAsync(OutboxEventEnvelope evt, CancellationToken ct)
    {
        var payload = JsonSerializer.Deserialize<OrderIdOutboxPayload>(evt.PayloadJson)
                      ?? throw new InvalidOperationException($"Invalid payload for event {evt.EventId}");

        var order = await orderRepository.GetByIdAsync(payload.OrderId, ct);
        if (order is null) return;

        var user = await userRepository.GetByIdAsync(order.UserId, ct);
        if (user is null) return;

        var shipment = await shipmentRepository.GetByOrderIdAsync(payload.OrderId, ct);
        if (shipment is null) return;

        var detail = shipment switch
        {
            CVSShipment cvs => $"<p>取件門市：<strong>{cvs.StoreName}</strong>（門市代號：{cvs.StoreId}）</p>",
            HomeShipment home => $"<p>配送地址：<strong>{home.ZipCode} {home.Address}</strong></p>",
            _ => string.Empty
        };

        await mailSender.SendAsync(new MailSendRequest(
            ToEmail: user.Email,
            Subject: "您的包裹已到達！",
            ToName: user.Name,
            HtmlBody: $"""
                <h2>您好，{user.Name}！</h2>
                <p>您的訂單（<code>{order.Id}</code>）包裹已到達。</p>
                {detail}
                <p>請盡快前往取件，感謝您的購買！</p>
                """), ct);
    }
}
