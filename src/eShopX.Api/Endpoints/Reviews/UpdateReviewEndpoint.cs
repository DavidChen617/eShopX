using System.Security.Claims;
using ApplicationCore.UseCases.Reviews.UpdateReview;

namespace eShopX.Endpoints.Reviews;

public class UpdateReviewEndpoint : IGroupedEndpoint<ReviewsGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPut("/{reviewId:guid}", Handle)
            .Accepts<UpdateReviewRequest>(MediaTypeNames.Application.Json)
            .Produces<ApiResponse<UpdateReviewResponse>>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden)
            .WithDescription("修改評價");
    }

    private static async Task<IResult> Handle(
        Guid reviewId,
        [FromBody] UpdateReviewRequest request,
        ISender sender,
        HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        var command = new UpdateReviewCommand(
            reviewId,
            userId,
            request.Rating,
            request.Content,
            request.IsAnonymous,
            request.ImageUrls);

        var result = await sender.Send(command);
        return Results.Ok(ApiResponse<UpdateReviewResponse>.Success(result, "評價已更新"));
    }
}

public record UpdateReviewRequest(
    int Rating,
    string? Content,
    bool IsAnonymous = false,
    List<string>? ImageUrls = null);
