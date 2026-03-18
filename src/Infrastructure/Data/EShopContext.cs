using CoreMesh.Dispatching.Abstractions;
using Domain.Aggregates;
using Domain.Aggregates.Carts;
using Domain.Aggregates.Categories;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Payments;
using Domain.Aggregates.Products;
using Domain.Aggregates.Payments.Events;
using Domain.Aggregates.Products.Events;
using Domain.Aggregates.Shipments.Events;
using Domain.Aggregates.Shipments;
using Domain.Aggregates.Sizes;
using Domain.Aggregates.Tags;
using Domain.Aggregates.Users;
using Domain.Outbox;

namespace Infrastructure.Data;

public class EShopContext(DbContextOptions<EShopContext> options) : DbContext(options)
{
    // Users
    public DbSet<User> Users { get; set; }
    public DbSet<UserAuthProvider> UserAuthProviders { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    // Products
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<ProductSku> ProductSkus { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<ProductTag> ProductTags { get; set; }

    // Carts
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }

    // Orders
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    // Payments
    public DbSet<Payment> Payments { get; set; }

    // Shipments
    public DbSet<Shipment> Shipments { get; set; }

    // Lookup
    public DbSet<Category> Categories { get; set; }
    public DbSet<Size> Sizes { get; set; }
    public DbSet<Tag> Tags { get; set; }

    // Outbox
    public DbSet<OutboxEvent> OutboxEvents { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ConvertDomainEventsToOutboxEvents();

        foreach (var entry in ChangeTracker.Entries<UserAuthProvider>().Where(e => e.State == EntityState.Modified))
            entry.State = EntityState.Unchanged;

        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ConvertDomainEventsToOutboxEvents()
    {
        var aggregates = ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var outboxEvents = aggregates
            .SelectMany(a => a.DomainEvents)
            .Select(ToOutboxEvent)
            .OfType<OutboxEvent>()
            .ToList();

        OutboxEvents.AddRange(outboxEvents);

        foreach (var aggregate in aggregates)
            aggregate.ClearDomainEvents();
    }

    private static OutboxEvent? ToOutboxEvent(INotification domainEvent) => domainEvent switch
    {
        ProductCreatedEvent e  => OutboxEventFactory.CreateProductUpsert(e.ProductId),
        ProductUpdatedEvent e  => OutboxEventFactory.CreateProductUpsert(e.ProductId),
        ProductDeletedEvent e  => OutboxEventFactory.CreateProductDelete(e.ProductId),
        PaymentPaidEvent e     => OutboxEventFactory.CreatePaymentPaid(e.OrderId),
        PaymentFailedEvent e   => OutboxEventFactory.CreatePaymentFailed(e.OrderId),
        ShipmentCompletedEvent e => OutboxEventFactory.CreateShipmentCompleted(e.OrderId),
        _ => null
    };

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EShopContext).Assembly);
    }
}
