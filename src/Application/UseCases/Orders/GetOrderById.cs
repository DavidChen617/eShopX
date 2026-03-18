using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Mapper;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using Domain.Aggregates.Orders;

namespace Application.UseCases.Orders;

public record GetOrderByIdQuery(Guid OrderId) : IRequest<Result<OrderResponse>>;

public record OrderItemResponse(
    Guid ItemId,
    Guid SkuId,
    string ProductName,
    string Color,
    string? Size,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice);

public record OrderResponse(
    Guid OrderId,
    Guid UserId,
    string Status,
    decimal TotalAmount,
    DateTime CreatedAt,
    IReadOnlyList<OrderItemResponse> Items) : IMapFrom<Order, OrderResponse>
{
    public OrderResponse() : this(default, default, null!, default, default, null!) { }

    public OrderResponse MapFrom(Order source) => new(
        source.Id,
        source.UserId,
        source.Status.ToString(),
        source.TotalAmount.Amount,
        source.CreatedAt,
        source.Items.Select(i => new OrderItemResponse(
            i.Id,
            i.SkuId,
            i.Snapshot.ProductName,
            i.Snapshot.Color,
            i.Snapshot.Size,
            i.UnitPrice.Amount,
            i.Quantity,
            i.TotalPrice.Amount
        )).ToList()
    );
}

public class GetOrderByIdHandler(
    IOrderRepository orderRepository,
    IMapper mapper) : IRequestHandler<GetOrderByIdQuery, Result<OrderResponse>>
{
    public async Task<Result<OrderResponse>> Handle(
        GetOrderByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(query.OrderId, cancellationToken);
        if (order is null)
            return Result<OrderResponse>.NotFound(new Error("order_not_found", "Order not found."));

        return Result<OrderResponse>.Ok(mapper.Map<Order, OrderResponse>(order));
    }
}
