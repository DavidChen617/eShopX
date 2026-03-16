using eShopX.Application.UseCases.Sizes;
using eShopX.Domain.Aggregates.Sizes;

namespace eShopX.Endpoints.Admin.Sizes;

public sealed class CreateSizeEndpoint : IGroupedEndpoint<AdminSizesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/", Handle)
            .Produces<ApiResponse<SizeResponse>>(201)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        CreateSizeRequest request,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new CreateSizeCommand(request.Name, request.Type), ct);
        return result.ToHttpResult();
    }
}

public record CreateSizeRequest(string Name, SizeType Type);
