namespace Infrastructure.Data;

public class EShopContext(DbContextOptions<EShopContext> options) : DbContext(options)
{
    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<ProductImage> ProductImages { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<OrderItem> OrderItems { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<ExternalLogin> ExternalLogins { get; set; }
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    public virtual DbSet<Cart> Carts { get; set; }
    public virtual DbSet<CartItem> CartItems { get; set; }
    public virtual DbSet<Banner> Banners { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<FlashSale> FlashSales { get; set; }
    public virtual DbSet<FlashSaleSlot> FlashSaleSlots { get; set; }
    public virtual DbSet<FlashSaleItem> FlashSaleItems { get; set; }
    public virtual DbSet<ProductRecommend> ProductRecommends { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EShopContext).Assembly);
    }
}