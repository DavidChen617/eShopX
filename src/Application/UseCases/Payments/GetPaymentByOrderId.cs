using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Mapper;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using Domain.Aggregates.Payments;

namespace Application.UseCases.Payments;

public record GetPaymentByOrderIdQuery(Guid OrderId) : IRequest<Result<PaymentResponse>>;

public record PaymentResponse(
    Guid PaymentId,
    Guid OrderId,
    string Method,
    string Status,
    decimal Amount,
    string? TransactionId,
    string? PaymentUrl,
    DateTime? PaidAt,
    DateTime CreatedAt) : IMapFrom<Payment, PaymentResponse>
{
    public PaymentResponse() : this(default, default, null!, null!, default, null, null, null, default) { }

    public PaymentResponse MapFrom(Payment source) => new(
        source.Id,
        source.OrderId,
        source.Method.ToString(),
        source.Status.ToString(),
        source.Amount.Amount,
        source.TransactionId,
        source.PaymentUrl,
        source.PaidAt,
        source.CreatedAt);
}

public class GetPaymentByOrderIdHandler(
    IPaymentRepository paymentRepository,
    IMapper mapper) : IRequestHandler<GetPaymentByOrderIdQuery, Result<PaymentResponse>>
{
    public async Task<Result<PaymentResponse>> Handle(
        GetPaymentByOrderIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var payment = await paymentRepository.GetByOrderIdAsync(query.OrderId, cancellationToken);
        if (payment is null)
            return Result<PaymentResponse>.NotFound(new Error("payment_not_found", "Payment not found."));

        return Result<PaymentResponse>.Ok(mapper.Map<Payment, PaymentResponse>(payment));
    }
}
