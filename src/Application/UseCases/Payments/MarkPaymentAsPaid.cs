using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;

namespace Application.UseCases.Payments;

public record MarkPaymentAsPaidCommand(Guid PaymentId, string TransactionId) : IRequest<Result>;

public class MarkPaymentAsPaidHandler(
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<MarkPaymentAsPaidCommand, Result>
{
    public async Task<Result> Handle(
        MarkPaymentAsPaidCommand command,
        CancellationToken cancellationToken = default)
    {
        var payment = await paymentRepository.GetByIdAsync(command.PaymentId, cancellationToken);
        if (payment is null)
            return Result.NotFound(new Error("payment_not_found", "Payment not found."));

        payment.MarkAsPaid(command.TransactionId);

        paymentRepository.Update(payment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.NoContent();
    }
}
