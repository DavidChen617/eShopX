using eShopX.Domain.Aggregates.Payments.Events;
using eShopX.Domain.Exceptions;

namespace eShopX.Domain.Aggregates.Payments;

public sealed class Payment : AggregateRoot
{
    public Guid OrderId { get; private set; }
    public PaymentMethod Method { get; private set; }
    public PaymentStatus Status { get; private set; }
    public decimal Amount { get; private set; }
    public string? TransactionId { get; private set; }
    public string? PaymentUrl { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Payment() { }

    public static Payment Create(Guid orderId, PaymentMethod method, decimal amount, string? paymentUrl)
    {
        return new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Method = method,
            Status = PaymentStatus.Pending,
            Amount = amount,
            PaymentUrl = paymentUrl,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsPaid(string transactionId)
    {
        if (Status != PaymentStatus.Pending)
            throw new ArgumentInvalidException("Only pending payments can be marked as paid.");
        Status = PaymentStatus.Paid;
        TransactionId = transactionId;
        PaidAt = DateTime.UtcNow;
        RaiseDomainEvent(new PaymentPaidEvent(Id, OrderId, transactionId));
    }

    public void MarkAsFailed()
    {
        if (Status != PaymentStatus.Pending)
            throw new ArgumentInvalidException("Only pending payments can be marked as failed.");
        Status = PaymentStatus.Failed;
        RaiseDomainEvent(new PaymentFailedEvent(Id, OrderId));
    }
}
