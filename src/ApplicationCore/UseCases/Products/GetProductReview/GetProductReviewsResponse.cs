namespace ApplicationCore.UseCases.Products.GetProductReview;

public record GetProductReviewsResponse(List<ReviewItem> Items,                                                                                                                    
    int TotalCount,                                                                                                                            
    decimal AverageRating);

public record ReviewItem(                                                                                                                      
    Guid ReviewId,                                                                                                                             
    string? UserName,                                                                                                                          
    int Rating,                                                                                                                                
    string? Content,                                                                                                                           
    List<string> ImageUrls,                                                                                                                    
    DateTime CreatedAt);
