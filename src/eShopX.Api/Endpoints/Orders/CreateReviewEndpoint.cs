using System.Security.Claims;
using ApplicationCore.UseCases.Reviews.CreateReview;

namespace eShopX.Endpoints.Orders;

public class CreateReviewEndpoint : IGroupedEndpoint<OrdersGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/{orderId:guid}/review", Handle)
            .Accepts<CreateReviewRequest>(MediaTypeNames.Application.Json)
            .Produces<ApiResponse<CreateReviewResponse>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("提交訂單評價");
    }

    private static async Task<IResult> Handle(Guid orderId,
        [FromBody] CreateReviewRequest request,
        ISender sender,
        HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        var command = new CreateReviewCommand(
            orderId,
            request.OrderItemId,
            userId,
            request.Rating,
            request.Content,
            request.IsAnonymous,
            request.ImageUrls);

        var result = await sender.Send(command);

        return Results.Created(
            $"/api/reviews/{result.ReviewId}",
            ApiResponse<CreateReviewResponse>.Success(result, "評價成功", StatusCodes.Status201Created));
    }
}

public record CreateReviewRequest(
    Guid OrderItemId,
    int Rating,
    string? Content,
    bool IsAnonymous = false,
    List<string>? ImageUrls = null);
