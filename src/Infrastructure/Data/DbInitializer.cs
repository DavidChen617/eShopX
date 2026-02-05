using Infrastructure.Auth;

namespace Infrastructure.Data;

public static class DbInitializer
{
    private static readonly Guid[] FixedCategoryIds =
    [
        Guid.Parse("11111111-1111-1111-1111-111111111111"),
        Guid.Parse("22222222-2222-2222-2222-222222222222"),
        Guid.Parse("33333333-3333-3333-3333-333333333333"),
        Guid.Parse("44444444-4444-4444-4444-444444444444"),
        Guid.Parse("55555555-5555-5555-5555-555555555555"),
        Guid.Parse("66666666-6666-6666-6666-666666666666"),
        Guid.Parse("77777777-7777-7777-7777-777777777777"),
        Guid.Parse("88888888-8888-8888-8888-888888888888"),
        Guid.Parse("99999999-9999-9999-9999-999999999999"),
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")
    ];

    public static async Task SeedAsync(EShopContext context)
    {
        await ResetSeedDataAsync(context);
        await SeedAdminUserAsync(context);
        await SeedSellerUserAsync(context);
        await SeedBannersAsync(context);
        await SeedCategoriesAsync(context);
        await SeedProductsAsync(context);
        await SeedFlashSaleAsync(context);
        await SeedProductRecommendsAsync(context);
    }

    private static async Task SeedAdminUserAsync(EShopContext context)
    {
        const string adminEmail = "admin@example.com";
        if (await context.Users.AnyAsync(x => x.Email == adminEmail))
            return;

        var hasher = new PasswordHasher();
        var admin = new User
        {
            Name = "David Chen",
            Email = "admin@example.com",
            Phone = string.Empty,
            PasswordHash = hasher.HashPassword("1qaz!QAZ"),
            IsAdmin = true,
            IsSeller = false
        };

        context.Users.Add(admin);
        await context.SaveChangesAsync();
    }

    private static async Task SeedSellerUserAsync(EShopContext context)
    {
        var sellerId = Guid.Parse("019c232c-8dbc-7c2b-a02f-af8dfc856490");
        if (await context.Users.AnyAsync(x => x.Id == sellerId))
            return;

        var hasher = new PasswordHasher();
        var seller = new User
        {
            Id = sellerId,
            Name = "Andy",
            Email = "andy@example.com",
            Phone = "0912312312",
            PasswordHash = hasher.HashPassword("1qaz!QAZ"),
            IsAdmin = false,
            IsSeller = true
        };

        context.Users.Add(seller);
        await context.SaveChangesAsync();
    }

    private static async Task SeedBannersAsync(EShopContext context)
    {
        if (await context.Banners.AnyAsync())
            return;

        var banners = new List<Banner>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "雙11狂歡節 全場5折起",
                ImageUrl = "https://placehold.co/750x300/FF5000/FFFFFF?text=雙11狂歡節",
                Link = "/products?promo=double11",
                SortOrder = 1,
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "限時秒殺 每日10點開搶",
                ImageUrl = "https://placehold.co/750x300/FF2D54/FFFFFF?text=限時秒殺",
                Link = "/products?flash=true",
                SortOrder = 2,
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "春季新品 搶先預購",
                ImageUrl = "https://placehold.co/750x300/FFD700/333333?text=新品上市",
                Link = "/products?q=new",
                SortOrder = 3,
                IsActive = true
            }
        };

        context.Banners.AddRange(banners);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCategoriesAsync(EShopContext context)
    {
        if (await context.Categories.AnyAsync())
            return;

        var categories = new List<Category>
        {
            new() { Id = FixedCategoryIds[0], Name = "服飾", Icon = "👕", Link = "/products?cat=clothing", SortOrder = 1 },
            new() { Id = FixedCategoryIds[1], Name = "美妝", Icon = "💄", Link = "/products?cat=beauty", SortOrder = 2 },
            new() { Id = FixedCategoryIds[2], Name = "生鮮", Icon = "🍎", Link = "/products?cat=fresh", SortOrder = 3 },
            new() { Id = FixedCategoryIds[3], Name = "數碼", Icon = "📱", Link = "/products?cat=digital", SortOrder = 4 },
            new() { Id = FixedCategoryIds[4], Name = "家居", Icon = "🏠", Link = "/products?cat=home", SortOrder = 5 },
            new() { Id = FixedCategoryIds[5], Name = "遊戲", Icon = "🎮", Link = "/products?cat=gaming", SortOrder = 6 },
            new() { Id = FixedCategoryIds[6], Name = "書籍", Icon = "📚", Link = "/products?cat=books", SortOrder = 7 },
            new() { Id = FixedCategoryIds[7], Name = "箱包", Icon = "🎒", Link = "/products?cat=bags", SortOrder = 8 },
            new() { Id = FixedCategoryIds[8], Name = "運動", Icon = "⚽", Link = "/products?cat=sports", SortOrder = 9 },
            new() { Id = FixedCategoryIds[9], Name = "更多", Icon = "⋯", Link = "/products", SortOrder = 10 }
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();
    }

    private static async Task SeedProductsAsync(EShopContext context)
    {
        if (await context.Products.AnyAsync())
            return;

        var sellerId = Guid.Parse("019c232c-8dbc-7c2b-a02f-af8dfc856490");
        var sellerExists = await context.Users.AnyAsync(x => x.Id == sellerId);
        if (!sellerExists)
            return;

        var categories = await context.Categories
            .OrderBy(x => x.SortOrder)
            .ToListAsync();
        if (categories.Count == 0)
            return;

        var rng = new Random(20260203);
        var products = new List<Product>();
        var suffixes = new[] { "精選", "熱銷", "人氣", "新品", "限定" };

        foreach (var category in categories)
        {
            var count = rng.Next(5, 11);
            for (var i = 1; i <= count; i++)
            {
                var createdAt = DateTime.UtcNow.AddDays(-rng.Next(1, 120));
                var name = $"{category.Name} {suffixes[rng.Next(suffixes.Length)]} {i}";
                var price = Math.Round((decimal)(rng.Next(50, 5000) + rng.NextDouble()), 2);
                var stock = rng.Next(5, 200);
                var isActive = rng.Next(0, 10) > 1;

                products.Add(new Product
                {
                    Id = Guid.NewGuid(),
                    SellerId = sellerId,
                    CategoryId = category.Id,
                    Name = name,
                    Description = $"精選 {category.Name} 商品，品質保證，適合日常使用。",
                    Price = price,
                    StockQuantity = stock,
                    IsActive = isActive,
                    CreatedAt = createdAt,
                    UpdatedAt = createdAt
                });
            }
        }

        context.Products.AddRange(products);
        await context.SaveChangesAsync();
    }

    private static async Task ResetSeedDataAsync(EShopContext context)
    {
        if (!await context.Categories.AnyAsync())
            return;

        var existingCategoryIds = await context.Categories
            .Select(x => x.Id)
            .ToListAsync();

        var fixedSet = FixedCategoryIds.ToHashSet();
        var hasLegacyCategories = existingCategoryIds.Any(id => !fixedSet.Contains(id));

        if (!hasLegacyCategories)
            return;

        context.CartItems.RemoveRange(await context.CartItems.ToListAsync());
        context.Carts.RemoveRange(await context.Carts.ToListAsync());
        context.OrderItems.RemoveRange(await context.OrderItems.ToListAsync());
        context.Orders.RemoveRange(await context.Orders.ToListAsync());
        context.FlashSaleItems.RemoveRange(await context.FlashSaleItems.ToListAsync());
        context.FlashSaleSlots.RemoveRange(await context.FlashSaleSlots.ToListAsync());
        context.FlashSales.RemoveRange(await context.FlashSales.ToListAsync());
        context.ProductRecommends.RemoveRange(await context.ProductRecommends.ToListAsync());
        context.ProductImages.RemoveRange(await context.ProductImages.ToListAsync());
        context.Products.RemoveRange(await context.Products.ToListAsync());
        context.Categories.RemoveRange(await context.Categories.ToListAsync());

        await context.SaveChangesAsync();
    }

    private static async Task SeedFlashSaleAsync(EShopContext context)
    {
        if (await context.FlashSales.AnyAsync())
            return;

        var products = await context.Products.Take(4).ToListAsync();
        if (products.Count < 4)
            return;

        var now = DateTime.UtcNow;
        var flashSale = new FlashSale
        {
            Id = Guid.NewGuid(),
            Title = "限時秒殺",
            Subtitle = "高併發搶購體驗，需登入才能參與。",
            StartsAt = now.Date,
            EndsAt = now.Date.AddDays(1),
            IsActive = true
        };

        context.FlashSales.Add(flashSale);

        var slots = new List<FlashSaleSlot>
        {
            new()
            {
                Id = Guid.NewGuid(),
                FlashSaleId = flashSale.Id,
                Label = "10:00",
                StartsAt = now.Date.AddHours(2),
                EndsAt = now.Date.AddHours(4),
                SortOrder = 1
            },
            new()
            {
                Id = Guid.NewGuid(),
                FlashSaleId = flashSale.Id,
                Label = "12:00",
                StartsAt = now.Date.AddHours(4),
                EndsAt = now.Date.AddHours(6),
                SortOrder = 2
            },
            new()
            {
                Id = Guid.NewGuid(),
                FlashSaleId = flashSale.Id,
                Label = "14:00",
                StartsAt = now.Date.AddHours(6),
                EndsAt = now.Date.AddHours(8),
                SortOrder = 3
            },
            new()
            {
                Id = Guid.NewGuid(),
                FlashSaleId = flashSale.Id,
                Label = "16:00",
                StartsAt = now.Date.AddHours(8),
                EndsAt = now.Date.AddHours(10),
                SortOrder = 4
            }
        };

        context.FlashSaleSlots.AddRange(slots);

        var badges = new[] { "Hot", "限量", "售罄", "新品" };
        var items = products.Select((p, i) => new FlashSaleItem
        {
            Id = Guid.NewGuid(),
            FlashSaleId = flashSale.Id,
            SlotId = slots[i % slots.Count].Id,
            ProductId = p.Id,
            FlashPrice = Math.Round(p.Price * 0.6m, 2),
            StockTotal = 500 - i * 100,
            StockRemaining = i == 2 ? 0 : 500 - i * 100 - i * 50,
            Badge = badges[i],
            SortOrder = i + 1,
            PurchaseLimit = 1
        }).ToList();

        context.FlashSaleItems.AddRange(items);
        await context.SaveChangesAsync();
    }

    private static async Task SeedProductRecommendsAsync(EShopContext context)
    {
        if (await context.ProductRecommends.AnyAsync())
            return;

        var products = await context.Products.Where(p => p.IsActive).Take(6).ToListAsync();
        if (products.Count == 0)
            return;

        var recommends = products.Select((p, i) => new ProductRecommend
        {
            Id = Guid.NewGuid(),
            ProductId = p.Id,
            RecommendType = "homepage",
            SortOrder = i + 1,
            IsActive = true
        }).ToList();

        context.ProductRecommends.AddRange(recommends);
        await context.SaveChangesAsync();
    }
}