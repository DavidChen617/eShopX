namespace ApplicationCore.UseCases.Reviews.CreateReview;

public class CreateReviewHandler(
    IRepository<Review> reviewRepository, 
    IReadRepository<OrderItem> orderItemRepository,
    IMapper mapper): IRequestHandler<CreateReviewCommand, CreateReviewResponse>
{
    public async Task<CreateReviewResponse> Handle(CreateReviewCommand command, CancellationToken cancellationToken = default)
    {
        var orderItem = await orderItemRepository.GetByIdAsync(command.OrderItemId, cancellationToken)
                        ?? throw new NotFoundException("OrderItem", command.OrderItemId);

        if (orderItem.OrderId != command.OrderId)
            throw new BadRequestException($"OrderItem does not belong to this order");

        var review = new Review
        {
            OrderId =  command.OrderId,
            OrderItemId = command.OrderItemId,
            UserId = command.UserId,
            ProductId = orderItem.ProductId,
            Rating =  command.Rating,
            Content =  command.Content,
            IsAnonymous = command.IsAnonymous
        };

        if (command.ImageUrls?.Count > 0)
            review.Images = command.ImageUrls.Select((url, index) =>
                new ReviewImage{ ReviewId = review.Id, Url = url, SortOrder = index }).ToList();

        await reviewRepository.AddAsync(review, cancellationToken);
    
        return mapper.Map<Review, CreateReviewResponse>(review);
    }
}
