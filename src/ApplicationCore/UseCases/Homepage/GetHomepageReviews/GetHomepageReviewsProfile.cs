namespace ApplicationCore.UseCases.Homepage.GetHomepageReviews;

public class GetHomepageReviewsProfile : Profile
{
    public GetHomepageReviewsProfile()
    {
        CreateMap<Review, HomepageReviewDto>()
            .ConstructUsing(src => new HomepageReviewDto
            {
                ReviewId = src.Id,
                UserName = src.IsAnonymous ? null : src.User.Name,
                UserAvatar = src.IsAnonymous ? null : src.User.AvatarUrl,
                ProductId = src.ProductId,
                ProductName = src.Product.Name,
                Rating = src.Rating,
                Content = src.Content,
                ImageUrls = src.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).ToList(),
                CreatedAt = src.CreatedAt
            });
    }
}

public class HomepageReviewDto
{
    public Guid ReviewId { get; set; }
    public string? UserName { get; set; }
    public string? UserAvatar { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Content { get; set; }
    public List<string> ImageUrls { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}
