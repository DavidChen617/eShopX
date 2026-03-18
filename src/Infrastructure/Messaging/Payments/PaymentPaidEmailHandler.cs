using System.Text.Json;

namespace Infrastructure.Messaging.Payments;

public class PaymentPaidEmailHandler(
    IOrderRepository orderRepository,
    IUserRepository userRepository,
    IMailSender mailSender) : IOutboxEventHandler
{
    public bool CanHandle(string eventType) => eventType == OutboxEventFactory.PaymentPaidEventType;

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
            Subject: "付款成功確認",
            ToName: user.Name,
            HtmlBody: $"""
                <h2>您好，{user.Name}！</h2>
                <p>您的訂單（<code>{order.Id}</code>）付款已成功。</p>
                <p>訂單金額：<strong>NT$ {order.TotalAmount.Amount:N0}</strong></p>
                <p>感謝您的購買，我們將盡快為您處理出貨！</p>
                """), ct);
    }
}
