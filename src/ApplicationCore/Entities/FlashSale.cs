namespace ApplicationCore.Entities;

public class FlashSale : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime UpdatedAt { get; set; }

    public ICollection<FlashSaleSlot> Slots { get; set; } = [];
    public ICollection<FlashSaleItem> Items { get; set; } = [];
}
