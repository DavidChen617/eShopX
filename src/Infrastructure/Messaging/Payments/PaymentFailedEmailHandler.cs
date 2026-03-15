using System.Text.Json;
using eShopX.Application.Interfaces;
using eShopX.Application.Interfaces.Repositories;
using eShopX.Application.UseCases.Outbox;

namespace Infrastructure.Messaging.Payments;

public class PaymentFailedEmailHandler(
    IOrderRepository orderRepository,
    IUserRepository userRepository,
    IMailSender mailSender) : IOutboxEventHandler
{
    public bool CanHandle(string eventType) => eventType == OutboxEventFactory.PaymentFailedEventType;

    public async Task HandleAsync(OutboxEventEnvelope evt, CancellationToken ct)
    {
        var payload = JsonSerializer.Deserialize<OrderIdOutboxPayload>(evt.PayloadJson)
                      ?? throw new InvalidOperationException($"Invalid payload for event {evt.EventId}");

        var order = await orderRepository.GetByIdAsync(payload.OrderId, ct);
        if (order is null) return;

        var user = await userRepository.GetByIdAsync(order.UserId, ct);
        if (user is null) return;

        await mailSender.SendAsync(new MailSendRequest(
            ToEmail: user.Email,
            Subject: "付款失敗通知",
            ToName: user.Name,
            HtmlBody: $"""
                <h2>您好，{user.Name}！</h2>
                <p>很遺憾，您的訂單（<code>{order.Id}</code>）付款未成功。</p>
                <p>請重新嘗試付款，或聯絡客服人員協助處理。</p>
                """), ct);
    }
}
