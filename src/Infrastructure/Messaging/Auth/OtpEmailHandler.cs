using System.Text.Json;
using eShopX.Application.Interfaces;
using eShopX.Application.UseCases.Outbox;

namespace Infrastructure.Messaging.Auth;

public class OtpEmailHandler(IMailSender mailSender) : IOutboxEventHandler
{
    public bool CanHandle(string eventType) => eventType == OutboxEventFactory.OtpEmailEventType;

    public async Task HandleAsync(OutboxEventEnvelope evt, CancellationToken ct)
    {
        var payload = JsonSerializer.Deserialize<OtpEmailOutboxPayload>(evt.PayloadJson)
                      ?? throw new InvalidOperationException($"Invalid payload for event {evt.EventId}");

        await mailSender.SendAsync(new MailSendRequest(
            ToEmail: payload.Email,
            Subject: "您的 eShopX 驗證碼",
            ToName: payload.Email,
            HtmlBody: $"""
                <h2>驗證您的 Email</h2>
                <p>您的驗證碼為：</p>
                <h1 style="letter-spacing: 8px;">{payload.Otp}</h1>
                <p>此驗證碼將於 <strong>10 分鐘</strong>後失效，請勿將驗證碼提供給他人。</p>
                """), ct);
    }
}
