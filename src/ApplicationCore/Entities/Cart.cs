namespace ApplicationCore.Entities;

public class Cart : BaseEntity
{
    public Guid UserId { get; set; }
    public List<CartItem> Items { get; set; } = new();
}
