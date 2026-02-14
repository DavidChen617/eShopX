using Infrastructure.Payments;
using Infrastructure.Payments.Line.Models;

namespace eShopX.Endpoints.Payments;

public class LinePayRequestEndpoint : IGroupedEndpoint<PaymentsGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/line/request", Handle)
            .Accepts<LinePayRequest>(MediaTypeNames.Application.Json)
            .Produces<ApiResponse<LinePayRequestResponse>>();
    }

    private static async Task<IResult> Handle(
        [FromBody] LinePayRequest request,
        [FromServices] ICreatePaymentService<LinePayRequest, LinePayRequestResponse> linePayService)
    {
        var result = await linePayService.CreateAsync(request);
        return Results.Ok(ApiResponse<LinePayRequestResponse>.Success(result));
    }
}
