namespace eShopX.Domain.Outbox;

public enum OutboxStatus
{
    Pending,
    Processed,
    Failed
}
