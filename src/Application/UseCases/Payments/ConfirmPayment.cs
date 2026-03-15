using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using eShopX.Application.Interfaces;
using eShopX.Application.Interfaces.Repositories;

namespace eShopX.Application.UseCases.Payments;

public record ConfirmPaymentCommand(Guid OrderId, string TransactionId) : IRequest<Result>;

public class ConfirmPaymentHandler(
    IPaymentRepository paymentRepository,
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ConfirmPaymentCommand, Result>
{
    public async Task<Result> Handle(
        ConfirmPaymentCommand command,
        CancellationToken cancellationToken = default)
    {
        var payment = await paymentRepository.GetByOrderIdAsync(command.OrderId, cancellationToken);
        if (payment is null)
            return Result.NotFound(new Error("payment_not_found", "Payment not found."));

        var order = await orderRepository.GetByIdAsync(command.OrderId, cancellationToken);
        if (order is null)
            return Result.NotFound(new Error("order_not_found", "Order not found."));

        payment.MarkAsPaid(command.TransactionId);
        order.MarkAsPaid();

        paymentRepository.Update(payment);
        orderRepository.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.NoContent();
    }
}
