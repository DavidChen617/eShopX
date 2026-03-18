using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Infrastructure.Payments.Line;

public class LinePayClient(HttpClient client, IOptions<LinePayOptions> options)
{
    internal async Task<string> SendAsync(string apiPath, string jsonBody, CancellationToken cancellationToken)
    {
        var request = BuildRequest(apiPath, jsonBody);
        var response = await client.SendAsync(request, cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(responseText))
            throw new ExternalServiceException("LinePay", "API returned empty response.");

        return responseText;
    }

    private HttpRequestMessage BuildRequest(string apiPath, string jsonBody)
    {
        var nonce = Guid.NewGuid().ToString();
        var fullPath = new Uri(client.BaseAddress!, apiPath).AbsolutePath;
        var signature = BuildSignature(fullPath, jsonBody, nonce);

        var request = new HttpRequestMessage(HttpMethod.Post, apiPath);
        request.Headers.Add("X-LINE-ChannelId", options.Value.ChannelId);
        request.Headers.Add("X-LINE-Authorization", signature);
        request.Headers.Add("X-LINE-Authorization-Nonce", nonce);
        request.Content = new StringContent(jsonBody, Encoding.UTF8, MediaTypeNames.Application.Json);
        return request;
    }

    private string BuildSignature(string apiPath, string body, string nonce)
    {
        var message = options.Value.ChannelSecret + apiPath + body + nonce;
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(options.Value.ChannelSecret));
        return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(message)));
    }
}
