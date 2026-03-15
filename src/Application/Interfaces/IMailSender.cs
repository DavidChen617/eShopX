namespace eShopX.Application.Interfaces;

public interface IMailSender
{
    Task SendAsync(MailSendRequest request, CancellationToken cancellationToken = default);
}

public record MailSendRequest(
    string ToEmail,
    string Subject,
    string? ToName = null,
    string? TextBody = null,
    string? HtmlBody = null);
