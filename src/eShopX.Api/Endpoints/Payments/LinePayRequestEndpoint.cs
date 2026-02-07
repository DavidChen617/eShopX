using Infrastructure.Payments;
using Infrastructure.Payments.Line.Models;

namespace eShopX.Endpoints.Payments;

public class LinePayRequestEndpoint : IGroupedEndpoint<PaymentsGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/line/request", HandleAsync)
            .Accepts<LinePayRequest>(MediaTypeNames.Application.Json)
            .Produces<ApiResponse<LinePayRequestResponse>>();
    }

    public async Task<IResult> HandleAsync(
        [FromBody] LinePayRequest request,
        [FromServices]
        IPaymentService<LinePayRequest, LinePayRequestResponse?, LinePayConfirmInput, LinePayConfirmResponse?> linePayService)
    {
        try
        {
            var result = await linePayService.CreateAsync(request);
            return Results.Ok(ApiResponse<LinePayRequestResponse?>.Success(result));
        }
        catch (Exception e)
        {
            var message = e.InnerException is null ? e.Message : e.InnerException.Message;
            return Results.BadRequest(ApiResponse.Error(StatusCodes.Status400BadRequest, message));
        }
    }
}
