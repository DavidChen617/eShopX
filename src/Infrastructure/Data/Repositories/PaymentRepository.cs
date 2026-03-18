using Domain.Aggregates.Payments;

namespace Infrastructure.Data.Repositories;

public class PaymentRepository(EShopContext db) : IPaymentRepository
{
    public Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => db.Payments.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        => db.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId, cancellationToken);

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
        => await db.Payments.AddAsync(payment, cancellationToken);

    public void Update(Payment payment)
        => db.Payments.Update(payment);

    public void Delete(Payment payment)
        => db.Payments.Remove(payment);
}
