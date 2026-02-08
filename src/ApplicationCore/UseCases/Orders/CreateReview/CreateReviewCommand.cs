namespace ApplicationCore.UseCases.Reviews.CreateReview;

public record CreateReviewCommand(Guid OrderId,                                                                                                                              
    Guid OrderItemId,                                                                                                                          
    Guid UserId,                                                                                                                               
    int Rating,                                                                                                                                
    string? Content,                                                                                                                           
    bool IsAnonymous,                                                                                                                          
    List<string>? ImageUrls) : IRequest<CreateReviewResponse>;
