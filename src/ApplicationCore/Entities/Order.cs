using ApplicationCore.UseCases.Orders;

namespace ApplicationCore.Entities;

public class Order : BaseEntity
{
    public Guid UserId { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
    public string ShippingName { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string ShippingPhone { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new();
}