using eShopX.Application.Interfaces;
using eShopX.Domain.Aggregates.Payments;
using Infrastructure.Options;
using Infrastructure.Payments.Line;
using Infrastructure.Payments.Line.Models;
using Infrastructure.Payments.PayPal;
using Infrastructure.Payments.PayPal.Models;
using Microsoft.Extensions.Options;

namespace Infrastructure.Payments;

public class PaymentGateway(
    LinePayService linePay,
    PayPalService payPal,
    IOptions<SiteOptions> siteOptions) : IPaymentGateway
{
    public Task<PaymentGatewayResult> RequestAsync(PaymentGatewayRequest request, CancellationToken ct = default)
        => request.Method switch
        {
            PaymentMethod.LinePay => RequestLinePayAsync(request, ct),
            PaymentMethod.PayPal => RequestPayPalAsync(request, ct),
            _ => Task.FromResult(new PaymentGatewayResult(false, null, "unsupported_method",
                $"Payment method {request.Method} is not supported."))
        };

    private async Task<PaymentGatewayResult> RequestLinePayAsync(PaymentGatewayRequest request, CancellationToken ct)
    {
        var site = siteOptions.Value;
        var order = request.Order;
        var req = new LinePayRequest(
            Amount: order.TotalAmount.Amount,
            Currency: "TWD",
            OrderId: order.Id.ToString(),
            Packages:
            [
                new LinePayPackage(
                    Id: order.Id.ToString(),
                    Amount: order.TotalAmount.Amount,
                    Products: order.Items
                        .Select((item, i) => new LinePayProduct(
                            Id: (i + 1).ToString(),
                            Name: item.Snapshot.ProductName,
                            Quantity: item.Quantity,
                            Price: item.UnitPrice.Amount))
                        .ToList()
                )
            ],
            RedirectUrls: new LinePayRedirectUrls(
                ConfirmUrl: GetUrl("confirm"),
                CancelUrl: GetUrl("cancel")
            )
        );

        var response = await linePay.RequestPaymentAsync(req, ct);
        if (response.ReturnCode != "0000")
            return new PaymentGatewayResult(false, null, "linepay_error", response.ReturnMessage);

        return new PaymentGatewayResult(true, response.Info?.PaymentUrl?.Web);

        string GetUrl(string type)
        {
            return site.FrontendDomain + "/payments/linepay/" + type + "?orderId=" + order.Id;
        }
    }

    private async Task<PaymentGatewayResult> RequestPayPalAsync(PaymentGatewayRequest request, CancellationToken ct)
    {
        var site = siteOptions.Value;
        var order = request.Order;
        var req = new PayPalCreateOrderRequest(
            Intent: "CAPTURE",
            PurchaseUnits:
            [
                new PayPalPurchaseUnit(
                    ReferenceId: order.Id.ToString(),
                    Amount: new PayPalAmount("USD", order.TotalAmount.Amount.ToString("F2"))
                )
            ],
            ApplicationContext: new PayPalApplicationContext(
                ReturnUrl: GetUrl("return"),
                CancelUrl: GetUrl("cancel")
            )
        );

        var response = await payPal.CreateAsync(req, ct);
        var paymentUrl = response.Links?.FirstOrDefault(l => l.Rel == "approve")?.Href;
        return new PaymentGatewayResult(true, paymentUrl);

        string GetUrl(string type)
        {
            return site.DomainUrl + "/api/v1/payments/paypal/" + type + "?orderId=" + order.Id;
        }
    }
}
