namespace ApplicationCore.Entities;

public class FlashSaleItem : BaseEntity
{
    public Guid FlashSaleId { get; set; }
    public Guid? SlotId { get; set; }
    public Guid ProductId { get; set; }
    public decimal FlashPrice { get; set; }
    public int StockTotal { get; set; }
    public int StockRemaining { get; set; }
    public string? Badge { get; set; }
    public int SortOrder { get; set; }
    public int PurchaseLimit { get; set; } = 1;

    public FlashSale FlashSale { get; set; } = null!;
    public FlashSaleSlot? Slot { get; set; }
    public Product Product { get; set; } = null!;
}
