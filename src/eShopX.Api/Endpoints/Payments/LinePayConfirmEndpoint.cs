using Infrastructure.Payments;
using Infrastructure.Payments.Line.Models;

namespace eShopX.Endpoints.Payments;

public class LinePayConfirmEndpoint : IGroupedEndpoint<PaymentsGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/line/{transactionId}/confirm", HandleAsync)
            .Accepts<LinePayConfirmRequest>(MediaTypeNames.Application.Json)
            .Produces<ApiResponse<LinePayConfirmResponse>>();
    }

    public async Task<IResult> HandleAsync([FromRoute] long transactionId, [FromBody] LinePayConfirmRequest request, [FromServices] IPaymentService<LinePayRequest, LinePayRequestResponse?, LinePayConfirmInput, LinePayConfirmResponse?> linePayService)
    {
        try
        {
            var result = await linePayService.ConfirmAsync(new LinePayConfirmInput(transactionId, request));
            return Results.Ok(ApiResponse<LinePayConfirmResponse?>.Success(result));
        }
        catch (Exception e)
        {
            var message = e.InnerException is null ? e.Message : e.InnerException.Message;
            return Results.BadRequest(ApiResponse.Error(StatusCodes.Status400BadRequest, message));
        }
    }
}