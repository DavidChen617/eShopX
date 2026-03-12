using Infrastructure.Options;
using Infrastructure.Payments;
using Infrastructure.Payments.PayPal;

using Microsoft.Extensions.Options;

namespace eShopX.Endpoints.Payments;

public class PayPalGroupEndpoint : IGroupEndpoint
{
    public string GroupPrefix { get; } = "/api/payments";
    public void Configure(RouteGroupBuilder group) => group.WithTags("Payments");
}

public class PayPalCreateOrderEndpoint : IGroupedEndpoint<PayPalGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/paypal/create-order", Handle)
            .Accepts<PayPalCreateOrderCommand>(MediaTypeNames.Application.Json)
            .Produces<ApiResponse<PayPalCreateOrderResult>>();
    }

    private static async Task<IResult> Handle(
        [FromBody] PayPalCreateOrderCommand cmd,
        [FromServices] ICreatePaymentService<PayPalCreateOrderRequest, PayPalCreateOrderResponse> payPalService,
        [FromServices] IOptions<PayPalOptions> options)
    {
        var publicBase = string.IsNullOrWhiteSpace(options.Value.PublicBaseUrl)
            ? options.Value.FrontendBaseUrl
            : options.Value.PublicBaseUrl;

        var amountValue = cmd.Currency.Equals("TWD", StringComparison.OrdinalIgnoreCase)
            ? Math.Round(cmd.Amount, 0, MidpointRounding.AwayFromZero)
            : cmd.Amount;
        var amountText = cmd.Currency.Equals("TWD", StringComparison.OrdinalIgnoreCase)
            ? amountValue.ToString("0")
            : amountValue.ToString("0.##");

        var returnUrl = $"{publicBase.TrimEnd('/')}/api/payments/paypal/callback/return";
        var cancelUrl = $"{publicBase.TrimEnd('/')}/api/payments/paypal/callback/cancel";
        if (cmd.UserId.HasValue)
        {
            var userIdValue = Uri.EscapeDataString(cmd.UserId.Value.ToString());
            returnUrl = $"{returnUrl}?userId={userIdValue}";
            cancelUrl = $"{cancelUrl}?userId={userIdValue}";
        }

        var req = new PayPalCreateOrderRequest(
            Intent: "CAPTURE",
            PurchaseUnits: new()
            {
                new PayPalPurchaseUnit(
                    ReferenceId: cmd.OrderId,
                    Amount: new PayPalAmount(cmd.Currency, amountText)
                )
            },
            ApplicationContext: new PayPalApplicationContext(
                ReturnUrl: returnUrl,
                CancelUrl: cancelUrl
            )
        );

        var resp = await payPalService.CreateAsync(req);
        var approveUrl = resp.Links?.FirstOrDefault(x => x.Rel is "approve" or "payer-action")?.Href ?? "";

        return Results.Ok(ApiResponse<PayPalCreateOrderResult>.Success(
            new PayPalCreateOrderResult(resp.Id, approveUrl)
        ));
    }
}
public record PayPalCreateOrderCommand(decimal Amount, string Currency, string OrderId, Guid? UserId);
public record PayPalCreateOrderResult(string OrderId, string ApproveUrl);
