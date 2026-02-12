using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using ApplicationCore.UseCases.Orders.CreatePaidOrderFromCart;

using Infrastructure.Options;
using Infrastructure.Payments;
using Infrastructure.Payments.PayPal;

using Microsoft.Extensions.Options;

namespace eShopX.Endpoints.Payments;

public class PayPalReturnEndpoint : IGroupedEndpoint<PayPalCallbackGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/return", HandleAsync)
            .AllowAnonymous();
    }

    public async Task<IResult> HandleAsync(
        [FromQuery] string token,
        [FromQuery] Guid? userId,
        [FromServices] IConfirmPaymentService<PayPalCaptureRequest, PayPalCaptureOrderResponse> payPalService,
        [FromServices] IOptions<PayPalOptions> options,
        [FromServices] IMailSender mailSender,
        [FromServices] ISender sender,
        [FromServices] IRepository<User> userRepository)
    {
        var result = await payPalService.ConfirmAsync(new PayPalCaptureRequest(token));
        var front = string.IsNullOrWhiteSpace(options.Value.FrontendBaseUrl)
            ? "http://localhost:4200"
            : options.Value.FrontendBaseUrl.TrimEnd('/');

        var status = result.Status == "COMPLETED" ? "success" : "fail";
        if (status == "success")
        {
            if (userId.HasValue)
            {
                var user = await userRepository.GetByIdAsync(userId.Value, default);
                if (user is not null)
                {
                    await mailSender.SendAsync(new MailSendRequest(
                        ToEmail: user.Email,
                        Subject: "PayPal 付款成功",
                        TextBody: $"交易成功。orderId={token}"
                    ));
                }
            }

            if (userId.HasValue)
            {
                await sender.Send(new CreatePaidOrderFromCartCommand(userId.Value, "PayPal"));
            }
        }
        return Results.Redirect($"{front}/pay/paypal/{status}?orderId={token}");
    }
}
