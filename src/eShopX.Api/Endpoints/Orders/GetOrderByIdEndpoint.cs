using System.Security.Claims;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using CoreMesh.Result.Http;
using eShopX.Application.UseCases.Orders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace eShopX.Api.Endpoints.Orders;

public sealed class GetOrderByIdEndpoint : IGroupedEndpoint<OrdersGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/{orderId:guid}", Handle);
    }

    private static async Task<IResult> Handle(
        Guid orderId,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetOrderByIdQuery(orderId), ct);
        return result.ToHttpResult();
    }
}
