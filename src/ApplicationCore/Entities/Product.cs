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
    
    /// <summary>                                                                                                                                  
    /// 樂觀鎖版本號 - 用於併發控制，防止超賣                                                                                                      
    /// PostgreSQL 使用 xmin 系統欄位                                                                                                              
    /// </summary> 
    public uint RowVersion { get; set; }
}
