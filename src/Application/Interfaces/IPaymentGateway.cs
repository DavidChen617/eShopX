using Domain.Aggregates.Orders;
using Domain.Aggregates.Payments;

namespace Application.Interfaces;

public interface IPaymentGateway
{
    Task<PaymentGatewayResult> RequestAsync(PaymentGatewayRequest request, CancellationToken ct = default);
}

public record PaymentGatewayRequest(PaymentMethod Method, Order Order);

public record PaymentGatewayResult(
    bool IsSuccess,
    string? PaymentUrl,
    string? ErrorCode = null,
    string? ErrorMessage = null);
