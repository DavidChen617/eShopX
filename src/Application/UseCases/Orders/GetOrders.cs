using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Mapper;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using Domain.Aggregates.Orders;

namespace Application.UseCases.Orders;

public record GetOrdersQuery(
    Guid? UserId = null,
    OrderStatus? Status = null,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<GetOrdersResponse>>;

public record OrderSummaryResponse(
    Guid OrderId,
    Guid UserId,
    string Status,
    decimal TotalAmount,
    DateTime CreatedAt) : IMapFrom<Order, OrderSummaryResponse>
{
    public OrderSummaryResponse() : this(default, default, null!, default, default) { }

    public OrderSummaryResponse MapFrom(Order source) =>
        new(source.Id, source.UserId, source.Status.ToString(), source.TotalAmount.Amount, source.CreatedAt);
}

public record GetOrdersResponse(
    IReadOnlyList<OrderSummaryResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);

public class GetOrdersHandler(
    IOrderRepository orderRepository,
    IMapper mapper) : IRequestHandler<GetOrdersQuery, Result<GetOrdersResponse>>
{
    public async Task<Result<GetOrdersResponse>> Handle(
        GetOrdersQuery query,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await orderRepository.GetPagedAsync(
            query.UserId,
            query.Status,
            query.Page,
            query.PageSize,
            cancellationToken);

        var responses = mapper.Map<Order, OrderSummaryResponse>(items).ToList();

        return Result<GetOrdersResponse>.Ok(
            new GetOrdersResponse(responses, totalCount, query.Page, query.PageSize));
    }
}
