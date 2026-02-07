using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using ApplicationCore.UseCases.Orders.CreatePaidOrderFromCart;

using Infrastructure.Options;
using Infrastructure.Payments;
using Infrastructure.Payments.Line.Models;

using Microsoft.Extensions.Options;

namespace eShopX.Endpoints.Payments;

public class LinePayConfirmCallbackEndpoint : IGroupedEndpoint<LinePayCallbackGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/confirm", HandleAsync)
            .AllowAnonymous();
    }

    public async Task<IResult> HandleAsync(
        [FromQuery] long? transactionId,
        [FromQuery] decimal? amount,
        [FromQuery] string? currency,
        [FromQuery] string? orderId,
        [FromQuery] Guid? userId,
        [FromServices]
        IPaymentService<LinePayRequest, LinePayRequestResponse?, LinePayConfirmInput, LinePayConfirmResponse?> linePayService,
        [FromServices] IOptions<LinePayOptions> options,
        [FromServices] IMailSender mailSender,
        [FromServices] ISender sender,
        [FromServices] IRepository<User> userRepository)
    {
        var frontBase = string.IsNullOrWhiteSpace(options.Value.FrontendBaseUrl)
            ? "http://localhost:4200"
            : options.Value.FrontendBaseUrl.TrimEnd('/');

        if (transactionId is null || amount is null || string.IsNullOrWhiteSpace(currency))
        {
            var failUrl = $"{frontBase}/pay/line/fail?reason=missing_params";
            return Results.Redirect(failUrl);
        }

        var confirmRequest = new LinePayConfirmRequest(amount.Value, currency);
        var result = await linePayService.ConfirmAsync(new LinePayConfirmInput(transactionId.Value, confirmRequest));

        var status = result?.ReturnCode == "0000" ? "success" : "fail";
        if (status == "success")
        {
            if (userId.HasValue)
            {
                var user = await userRepository.GetByIdAsync(userId.Value, default);
                if (user is not null)
                {
                    await mailSender.SendAsync(new MailSendRequest(
                        ToEmail: user.Email,
                        Subject: "Line Pay 付款成功",
                        TextBody: $"交易成功。orderId={orderId ?? ""}, transactionId={transactionId.Value}"
                    ));
                }
            }

            if (userId.HasValue)
            {
                await sender.Send(new CreatePaidOrderFromCartCommand(userId.Value, "LinePay"));
            }
        }
        var redirectUrl =
            $"{frontBase}/pay/line/{status}?transactionId={transactionId.Value}&orderId={Uri.EscapeDataString(orderId ?? string.Empty)}";

        return Results.Redirect(redirectUrl);
    }
}
