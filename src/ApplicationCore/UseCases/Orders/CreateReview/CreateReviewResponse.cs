namespace ApplicationCore.UseCases.Reviews.CreateReview;

public record  CreateReviewResponse( Guid ReviewId,                                                                                                                             
    Guid ProductId,                                                                                                                            
    int Rating,                                                                                                                                
    DateTime CreatedAt);
