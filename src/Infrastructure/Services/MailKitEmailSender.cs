using Infrastructure.Options;

using MailKit.Net.Smtp;
using MailKit.Security;

using Microsoft.Extensions.Options;

using MimeKit;

namespace Infrastructure.Services;

public class MailKitEmailSender(IOptions<MailOptions> options) : IMailSender
{
    private readonly MailOptions _options = options.Value;

    public async Task SendAsync(MailSendRequest request, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        var fromName = string.IsNullOrWhiteSpace(_options.FromName) ? _options.FromEmail : _options.FromName;
        message.From.Add(new MailboxAddress(fromName, _options.FromEmail));
        message.To.Add(new MailboxAddress(request.ToName ?? request.ToEmail, request.ToEmail));
        message.Subject = request.Subject;

        var builder = new BodyBuilder
        {
            TextBody = request.TextBody
        };
        if (!string.IsNullOrWhiteSpace(request.HtmlBody))
        {
            builder.HtmlBody = request.HtmlBody;
        }
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        var socket = ResolveSocketOptions();
        await client.ConnectAsync(_options.SmtpHost, _options.SmtpPort, socket, cancellationToken);

        if (!string.IsNullOrWhiteSpace(_options.UserName))
        {
            await client.AuthenticateAsync(_options.UserName, _options.Password, cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }

    private SecureSocketOptions ResolveSocketOptions()
    {
        if (_options.UseSsl)
        {
            return SecureSocketOptions.SslOnConnect;
        }

        if (_options.UseStartTls)
        {
            return SecureSocketOptions.StartTls;
        }

        return SecureSocketOptions.None;
    }
}