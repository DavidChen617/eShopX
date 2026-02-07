namespace ApplicationCore.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Guid? SellerId { get; set; }
    public User? Seller { get; set; }

    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
}
