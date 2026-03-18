using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Infrastructure.Payments.Line;

public class LinePayService(LinePayClient linePayClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<LinePayRequestResponse> RequestPaymentAsync(LinePayRequest request,
        CancellationToken ct = default)
    {
        var text = await linePayClient.SendAsync("request", Body(), ct);

        return JsonSerializer.Deserialize<LinePayRequestResponse>(text, JsonOptions)
               ?? throw new ExternalServiceException("LinePay", "Empty response from payment request.");

        string Body() => JsonSerializer.Serialize(request, JsonOptions);
    }

    public async Task<LinePayConfirmResponse> ConfirmPaymentAsync(long transactionId, LinePayConfirmRequest request,
        CancellationToken ct = default)
    {
        var text = await linePayClient.SendAsync($"{transactionId}/confirm", Body(), ct);

        return JsonSerializer.Deserialize<LinePayConfirmResponse>(text, JsonOptions)
               ?? throw new ExternalServiceException("LinePay", "Empty response from confirm.");
        
        string Body() => JsonSerializer.Serialize(request, JsonOptions);
    }
}
