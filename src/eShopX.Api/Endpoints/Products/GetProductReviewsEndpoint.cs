using ApplicationCore.UseCases.Products.GetProductReview;

namespace eShopX.Endpoints.Products;

public class GetProductReviewsEndpoint: IGroupedEndpoint<ProductsGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/{productId:guid}/reviews", Handle)                                                                                      
            .Produces<ApiResponse<GetProductReviewsResponse>>()
            .WithDescription("取得商品評價列表"); 
    }
    
    private static async Task<IResult> Handle(                                                                                                 
        Guid productId,                                                                                                 
        [FromQuery] int page,                                                                                                                  
        [FromQuery] int pageSize,                                                                                                              
        ISender sender)                                                                                                                        
    {                                                                                                                                          
        var query = new GetProductReviewsQuery(productId, page < 1 ? 1 : page, pageSize < 1 ? 10 : pageSize);                                  
        var result = await sender.Send(query);                                                                                                 
        return Results.Ok(ApiResponse<GetProductReviewsResponse>.Success(result));                                                             
    }
}
