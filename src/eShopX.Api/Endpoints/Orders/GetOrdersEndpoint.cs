using System.Security.Claims;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using CoreMesh.Result.Http;
using eShopX.Application.UseCases.Orders;
using eShopX.Domain.Aggregates.Orders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace eShopX.Api.Endpoints.Orders;

public sealed class GetOrdersEndpoint : IGroupedEndpoint<OrdersGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/", Handle);
    }

    private static async Task<IResult> Handle(
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        int page = 1,
        int pageSize = 20,
        OrderStatus? status = null,
        CancellationToken ct = default)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await dispatcher.Send(new GetOrdersQuery(userId, status, page, pageSize), ct);
        return result.ToHttpResult();
    }
}
