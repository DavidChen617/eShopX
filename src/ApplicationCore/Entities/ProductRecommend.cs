namespace ApplicationCore.Entities;

public class ProductRecommend : BaseEntity
{
    public Guid ProductId { get; set; }
    public string RecommendType { get; set; } = "homepage";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Product Product { get; set; } = null!;
}
