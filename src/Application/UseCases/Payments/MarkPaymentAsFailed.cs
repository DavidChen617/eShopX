using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using eShopX.Application.Interfaces;
using eShopX.Application.Interfaces.Repositories;

namespace eShopX.Application.UseCases.Payments;

public record MarkPaymentAsFailedCommand(Guid PaymentId) : IRequest<Result>;

public class MarkPaymentAsFailedHandler(
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<MarkPaymentAsFailedCommand, Result>
{
    public async Task<Result> Handle(
        MarkPaymentAsFailedCommand command,
        CancellationToken cancellationToken = default)
    {
        var payment = await paymentRepository.GetByIdAsync(command.PaymentId, cancellationToken);
        if (payment is null)
            return Result.NotFound(new Error("payment_not_found", "Payment not found."));

        payment.MarkAsFailed();

        paymentRepository.Update(payment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.NoContent();
    }
}
