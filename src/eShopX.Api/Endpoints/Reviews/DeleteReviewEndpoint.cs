using System.Security.Claims;
using ApplicationCore.UseCases.Reviews.DeleteReview;

namespace eShopX.Endpoints.Reviews;

public class DeleteReviewEndpoint : IGroupedEndpoint<ReviewsGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapDelete("/{reviewId:guid}", Handle)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden)
            .WithDescription("刪除評價");
    }

    private static async Task<IResult> Handle(
        Guid reviewId,
        ISender sender,
        HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        await sender.Send(new DeleteReviewCommand(reviewId, userId));
        return Results.NoContent();
    }
}
