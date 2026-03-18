using System.Text.Json;
using Application.UseCases.Outbox;

namespace Infrastructure.Messaging.Orders;

public class OrderShippedEmailHandler(IMailSender mailSender) : IOutboxEventHandler
{
    public bool CanHandle(string eventType) => eventType == OutboxEventFactory.OrderShippedEventType;

    public async Task HandleAsync(OutboxEventEnvelope evt, CancellationToken ct)
    {
        var payload = JsonSerializer.Deserialize<OrderShippedOutboxPayload>(evt.PayloadJson)
                      ?? throw new InvalidOperationException($"Invalid payload for event {evt.EventId}");

        var (subject, html) = BuildEmail(payload);

        await mailSender.SendAsync(new MailSendRequest(
            ToEmail: payload.UserEmail,
            Subject: subject,
            ToName: payload.UserName,
            HtmlBody: html), ct);
    }

    private static (string Subject, string Html) BuildEmail(OrderShippedOutboxPayload p)
    {
        var isCvs = p.StoreName is not null;

        var subject = "您的訂單已出貨！";

        var detail = isCvs
            ? $"<p>取件門市：<strong>{p.StoreName}</strong>（門市代號：{p.StoreId}）</p>"
            : $"<p>配送地址：<strong>{p.ZipCode} {p.Address}</strong></p>";

        var html = $"""
            <h2>您好，{p.UserName}！</h2>
            <p>您的訂單（<code>{p.OrderId}</code>）已出貨。</p>
            {detail}
            <p>物流編號：<strong>{p.LogisticsId}</strong></p>
            <p>感謝您的購買，祝您購物愉快！</p>
            """;

        return (subject, html);
    }
}
