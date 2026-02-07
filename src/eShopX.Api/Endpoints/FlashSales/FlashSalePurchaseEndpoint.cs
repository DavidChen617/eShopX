using System.Security.Claims;
using ApplicationCore.Interfaces;
using Infrastructure.Messaging;

namespace eShopX.Endpoints.FlashSales;

public class FlashSalePurchaseEndpoint: IGroupedEndpoint<FlashSaleGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/purchase", Handle)
            .Accepts<FlashSalePurchaseRequest>(MediaTypeNames.Application.Json)
            .Produces<FlashSalePurchaseResponse>(StatusCodes.Status202Accepted)
            .Produces(StatusCodes.Status409Conflict)
            .WithDescription("閃購搶購");
    }

    private static async Task<IResult> Handle(
        [FromBody] FlashSalePurchaseRequest request,                                                                                           
        IFlashSalePurchaseService flashSaleService,
        IFlashSaleOrderPublisher publisher,
        HttpContext httpContext)
    {
        // Get current user ID
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value; 

        if (!Guid.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();
        
        var result = await flashSaleService.TryDeductStockAsync(
            request.FlashSaleItemId,
            userId,
            request.Quantity);

        return result switch
        {
            -1 => Results.Conflict(ApiResponse.Error(409, "庫存不足")),
            -2 => Results.Conflict(ApiResponse.Error(409, "超過限購數量")),
            _ => await PublishAndReturn(publisher, request, userId)
        };
    }

    private static async Task<IResult> PublishAndReturn(IFlashSaleOrderPublisher publisher, FlashSalePurchaseRequest request, Guid userId)
    {
        // Send Kafka message
        await publisher.PublishAsync(new FlashSaleOrderMessage(
            request.FlashSaleItemId,
            userId,
            request.Quantity,
            DateTime.UtcNow));
        
        return Results.Accepted(null, ApiResponse<FlashSalePurchaseResponse>.Success(                                                              
            new FlashSalePurchaseResponse(request.FlashSaleItemId, userId, request.Quantity),                                                      
            "搶購成功，訂單處理中",                                                                                                                
            StatusCodes.Status202Accepted));
    }
}

public record FlashSalePurchaseRequest(Guid FlashSaleItemId, int Quantity = 1);                                                                
public record FlashSalePurchaseResponse(Guid FlashSaleItemId, Guid UserId, int Quantity);
