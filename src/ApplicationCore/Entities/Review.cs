namespace ApplicationCore.Entities;

public class Review : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid OrderItemId { get; set; }
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    /// <summary>
    /// 評分 1~5 星
    /// </summary>
    public int Rating { get; set; }
    /// <summary>
    /// 評價內容（可選）
    /// </summary>
    public string? Content { get; set; }
    /// <summary>
    /// 是否匿名評價
    /// </summary>
    public bool IsAnonymous { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Order Order { get; set; } = null!;
    public OrderItem OrderItem { get; set; } = null!;
    public User User { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public List<ReviewImage> Images { get; set; } = [];
}
