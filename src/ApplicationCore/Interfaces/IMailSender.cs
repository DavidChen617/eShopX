namespace ApplicationCore.Interfaces;

public interface IMailSender
{
    Task SendAsync(MailSendRequest request, CancellationToken cancellationToken = default);
}

public record MailSendRequest(
    string ToEmail,
    string Subject,
    string? TextBody = null,
    string? HtmlBody = null,
    string? ToName = null
);