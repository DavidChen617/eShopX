using Infrastructure.Payments.PayPal.Models;

namespace Infrastructure.Payments.PayPal;

public class PayPalService(PayPalClient client)
{
    public async Task<PayPalCreateOrderResponse> CreateAsync(
        PayPalCreateOrderRequest request, CancellationToken ct = default)
    {
        var token = await client.GetAccessTokenAsync(ct);
        return await client.CreateOrderAsync(token.AccessToken, request, ct);
    }

    public async Task<PayPalCaptureOrderResponse> ConfirmAsync(
        PayPalCaptureRequest request, CancellationToken ct = default)
    {
        var token = await client.GetAccessTokenAsync(ct);
        return await client.CaptureOrderAsync(token.AccessToken, request.OrderId, ct);
    }
}
