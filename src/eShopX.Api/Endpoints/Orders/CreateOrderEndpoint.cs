using System.Security.Claims;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using CoreMesh.Result.Http;
using eShopX.Api.Endpoints.Orders;
using eShopX.Application.Interfaces;
using eShopX.Application.UseCases.Logistics;
using eShopX.Application.UseCases.Orders;
using eShopX.Domain.Aggregates.Payments;
using eShopX.Domain.Aggregates.Shipments;
using Infrastructure.Logistics;
using Infrastructure.Options;
using Infrastructure.Payments.Line;
using Infrastructure.Payments.Line.Models;
using Infrastructure.Payments.PayPal;
using Microsoft.Extensions.Options;

namespace eShopX.Endpoints.Orders;

public sealed class CreateOrderEndpoint : IGroupedEndpoint<OrdersGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/", Handle);
    }

    private static async Task<IResult> Handle(
        CreateOrderRequest request,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        ICacher cacher,
        LinePayService linePayService,
        PayPalService payPalService,
        IOptions<LinePayOptions> linePayOptions,
        IOptions<PayPalOptions> payPalOptions,
        CancellationToken ct)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var logistics = await cacher.GetAsync<LogisticsCacheData>(
            LogisticsCacheKeys.UserLogistics(userId), ct);

        if (logistics is null)
            return Results.BadRequest(new { code = "logistics_not_found", message = "請先完成物流選擇。" });

        if (!Enum.TryParse<LogisticsSubType>(logistics.LogisticsSubType, true, out var logisticsSubType))
            return Results.BadRequest(new { code = "invalid_logistics_subtype", message = "無效的物流類型。" });

        var orderId = Guid.NewGuid();
        string? paymentUrl = null;

        if (request.PaymentMethod == PaymentMethod.LinePay)
        {
            var opt = linePayOptions.Value;
            var linePayReq = new LinePayRequest(
                Amount: request.TotalAmount,
                Currency: "TWD",
                OrderId: orderId.ToString(),
                Packages:
                [
                    new LinePayPackage(
                        Id: orderId.ToString(),
                        Amount: request.TotalAmount,
                        Products: [new LinePayProduct("1", "訂單商品", 1, request.TotalAmount)]
                    )
                ],
                RedirectUrls: new LinePayRedirectUrls(
                    ConfirmUrl: $"{opt.FrontendBaseUrl}/payments/linepay/confirm?orderId={orderId}",
                    CancelUrl: $"{opt.FrontendBaseUrl}/payments/linepay/cancel?orderId={orderId}"
                )
            );

            var linePayResponse = await linePayService.RequestPaymentAsync(linePayReq, ct);
            if (linePayResponse.ReturnCode != "0000")
                return Results.BadRequest(new { code = "linepay_error", message = linePayResponse.ReturnMessage });

            paymentUrl = linePayResponse.Info?.PaymentUrl?.Web;
        }
        else if (request.PaymentMethod == PaymentMethod.PayPal)
        {
            var opt = payPalOptions.Value;
            var payPalReq = new PayPalCreateOrderRequest(
                Intent: "CAPTURE",
                PurchaseUnits:
                [
                    new PayPalPurchaseUnit(
                        ReferenceId: orderId.ToString(),
                        Amount: new PayPalAmount("TWD", request.TotalAmount.ToString("F2"))
                    )
                ],
                ApplicationContext: new PayPalApplicationContext(
                    ReturnUrl: $"{opt.PublicBaseUrl}/api/payments/paypal/return?orderId={orderId}",
                    CancelUrl: $"{opt.PublicBaseUrl}/api/payments/paypal/cancel?orderId={orderId}"
                )
            );

            var payPalResponse = await payPalService.CreateAsync(payPalReq, ct);
            paymentUrl = payPalResponse.Links?.FirstOrDefault(l => l.Rel == "approve")?.Href;
        }

        HashSet<LogisticsSubType> cvsTypes = [LogisticsSubType.UNIMART, LogisticsSubType.FAMI, LogisticsSubType.HILIFE];
        var isCvs = cvsTypes.Contains(logisticsSubType);

        var command = new CreateOrderCommand(
            UserId: userId,
            PaymentMethod: request.PaymentMethod,
            PaymentUrl: paymentUrl,
            LogisticsId: logistics.TempLogisticsID,
            LogisticsSubType: logisticsSubType,
            ReceiverName: request.ReceiverName,
            ReceiverPhone: request.ReceiverPhone,
            StoreId: isCvs ? logistics.ReceiverStoreID : null,
            StoreName: isCvs ? logistics.ReceiverStoreName : null,
            ZipCode: isCvs ? null : logistics.ReceiverZipCode,
            Address: isCvs ? null : logistics.ReceiverAddress
        );

        var result = await dispatcher.Send(command, ct);
        return result.ToHttpResult();
    }
}

public record CreateOrderRequest(
    PaymentMethod PaymentMethod,
    string ReceiverName,
    string ReceiverPhone,
    decimal TotalAmount);
