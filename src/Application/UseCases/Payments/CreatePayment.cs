using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using Domain.Aggregates.Payments;

namespace Application.UseCases.Payments;

public record CreatePaymentCommand(
    Guid OrderId,
    PaymentMethod Method,
    string? PaymentUrl) : IRequest<Result<CreatePaymentResponse>>;

public record CreatePaymentResponse(Guid PaymentId, Guid OrderId, string? PaymentUrl);

public class CreatePaymentHandler(
    IOrderRepository orderRepository,
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreatePaymentCommand, Result<CreatePaymentResponse>>
{
    public async Task<Result<CreatePaymentResponse>> Handle(
        CreatePaymentCommand command,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(command.OrderId, cancellationToken);
        if (order is null)
            return Result<CreatePaymentResponse>.NotFound(new Error("order_not_found", "Order not found."));

        var existing = await paymentRepository.GetByOrderIdAsync(command.OrderId, cancellationToken);
        if (existing is not null)
            return Result<CreatePaymentResponse>.BadRequest(
                new Error("payment_already_exists", "A payment already exists for this order."));

        var payment = Payment.Create(
            command.OrderId,
            command.Method,
            order.TotalAmount,
            command.PaymentUrl);

        await paymentRepository.AddAsync(payment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CreatePaymentResponse>.Ok(
            new CreatePaymentResponse(payment.Id, payment.OrderId, payment.PaymentUrl));
    }
}
