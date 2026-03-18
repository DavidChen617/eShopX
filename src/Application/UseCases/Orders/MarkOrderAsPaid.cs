using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;

namespace Application.UseCases.Orders;

public record MarkOrderAsPaidCommand(Guid OrderId) : IRequest<Result>;

public class MarkOrderAsPaidHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<MarkOrderAsPaidCommand, Result>
{
    public async Task<Result> Handle(
        MarkOrderAsPaidCommand command,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(command.OrderId, cancellationToken);
        if (order is null)
            return Result.NotFound(new Error("order_not_found", "Order not found."));

        order.MarkAsPaid();
        orderRepository.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.NoContent();
    }
}
