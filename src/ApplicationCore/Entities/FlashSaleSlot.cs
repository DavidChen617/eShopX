namespace ApplicationCore.Entities;

public class FlashSaleSlot : BaseEntity
{
    public Guid FlashSaleId { get; set; }
    public string Label { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public int SortOrder { get; set; }

    public FlashSale FlashSale { get; set; } = null!;
    public ICollection<FlashSaleItem> Items { get; set; } = [];
}