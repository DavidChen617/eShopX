using eShopX.Domain.Aggregates.Categories;
using eShopX.Domain.Aggregates.Products;
using eShopX.Domain.Aggregates.Sizes;
using eShopX.Domain.Aggregates.Tags;
using eShopX.Domain.Aggregates.Users;
using eShopX.Domain.ValueObjects;

namespace Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedDataAsync(IServiceProvider serviceProvider)
    {
        var db = serviceProvider.GetRequiredService<EShopContext>();
        var cloudinaryOptions = serviceProvider.GetRequiredService<IOptions<CloudinaryOptions>>().Value;

        await SeedAdminAsync(db, serviceProvider);
        await SeedCategoriesAsync(db);
        await SeedSizesAsync(db);
        await SeedTagsAsync(db);
        await SeedProductsAsync(db, cloudinaryOptions);
    }

    private static async Task SeedAdminAsync(EShopContext db, IServiceProvider serviceProvider)
    {
        var exists = await db.Users.AnyAsync(x => x.Email == "admin@example.com");
        if (exists)
            return;

        var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher>();
        var passwordHash = passwordHasher.HashPassword("1qaz!QAZ");
        var admin = User.Create("SuperAdmin", "admin@example.com");
        var provider = UserAuthProvider.Create(admin.Id, Provider.Local, null, passwordHash: passwordHash);
        db.Users.Add(admin);
        db.UserAuthProviders.Add(provider);
        await db.SaveChangesAsync();
    }

    private static async Task SeedCategoriesAsync(EShopContext db)
    {
        if (await db.Categories.AnyAsync())
            return;

        var categories = new[] { "衣服", "褲子", "鞋子", "帽子" }
            .Select(Category.Create);

        db.Categories.AddRange(categories);
        await db.SaveChangesAsync();
    }

    private static async Task SeedSizesAsync(EShopContext db)
    {
        if (await db.Sizes.AnyAsync())
            return;

        var standard = SizeType.Clothing | SizeType.Hat;
        var sizes = new List<Size>();

        foreach (var name in new[] { "XS", "S", "M", "L", "XL", "XXL", "3XL" })
            sizes.Add(Size.Create(name, standard));

        foreach (var name in new[] { "26", "27", "28", "29", "30", "31", "32", "33", "34", "36", "38" })
            sizes.Add(Size.Create(name, SizeType.Pants));

        foreach (var name in new[] { "US 5", "US 5.5", "US 6", "US 6.5", "US 7", "US 7.5", "US 8",
                                     "US 8.5", "US 9", "US 9.5", "US 10", "US 10.5", "US 11", "US 12" })
            sizes.Add(Size.Create(name, SizeType.Shoes));

        db.Sizes.AddRange(sizes);
        await db.SaveChangesAsync();
    }

    private static async Task SeedTagsAsync(EShopContext db)
    {
        if (await db.Tags.AnyAsync()) return;

        var tags = new List<Tag>();

        foreach (var name in new[] { "春", "夏", "秋", "冬" })
            tags.Add(Tag.Create(name, TagType.Season));

        foreach (var name in new[] { "休閒", "正式", "運動", "街頭", "復古", "簡約" })
            tags.Add(Tag.Create(name, TagType.Style));

        foreach (var name in new[] { "防水", "透氣", "彈性", "顯瘦", "加大尺碼", "限量" })
            tags.Add(Tag.Create(name, TagType.Feature));

        db.Tags.AddRange(tags);
        await db.SaveChangesAsync();
    }

    private static async Task SeedProductsAsync(EShopContext db, CloudinaryOptions cloudinary)
    {
        if (await db.Products.AnyAsync()) return;

        var rng = new Random(42);
        int Stock() => rng.Next(30, 101);

        string Url(string publicId) =>
            $"https://res.cloudinary.com/{cloudinary.CloudName}/image/upload/{publicId}";

        void AddImages(ProductVariant variant, string baseId, bool hasTwoImages)
        {
            variant.AddImage(Url($"{baseId}/1"), $"{baseId}/1", isPrimary: true, sortOrder: 1);
            if (hasTwoImages)
                variant.AddImage(Url($"{baseId}/2"), $"{baseId}/2", isPrimary: false, sortOrder: 2);
        }

        // 查詢分類
        var cats = await db.Categories.ToDictionaryAsync(c => c.Name, c => c.Id);

        // 查詢尺寸
        var clothingSizes = await db.Sizes
            .Where(s => (s.Type & SizeType.Clothing) == SizeType.Clothing)
            .ToListAsync();
        var pantsSizes = await db.Sizes
            .Where(s => s.Type == SizeType.Pants)
            .ToListAsync();
        var shoesSizes = await db.Sizes
            .Where(s => s.Type == SizeType.Shoes)
            .ToListAsync();
        var hatSizes = await db.Sizes
            .Where(s => (s.Type & SizeType.Hat) == SizeType.Hat)
            .ToListAsync();

        var products = new List<Product>();

        Product Make(string name, string desc, string categoryName, Audience? audience,
                     string color, string publicIdBase, bool hasTwoImages,
                     List<Size> sizes, decimal price)
        {
            var product = Product.Create(name, desc, audience, cats[categoryName]);
            var variant = product.AddVariant(color);
            foreach (var size in sizes)
                variant.AddSku(size.Id, Money.Of(price), Stock());
            AddImages(variant, publicIdBase, hasTwoImages);
            product.Publish();
            return product;
        }

        products.Add(Make("男款簡約白色圓領T恤",
            "100% 純棉材質，柔軟透氣，版型寬鬆不緊繃。純白色百搭設計，是衣櫃裡不可缺少的基本款。",
            "衣服", Audience.Men, "白色",
            "eshopx/上衣/男/男款簡約白色圓領T恤_純色_基本款", true, clothingSizes, 890));

        products.Add(Make("男款深藍色輕量防風外套",
            "採用輕量防風面料，立領設計有效擋風。收納方便，適合通勤與戶外活動，春秋兩季皆宜。",
            "衣服", Audience.Men, "深藍色",
            "eshopx/上衣/男/男款深藍色輕量防風外套_立領", true, clothingSizes, 890));

        products.Add(Make("男款淺灰色連帽衛衣",
            "厚實棉質布料，保暖舒適。寬鬆版型提供充裕活動空間，連帽設計兼具休閒與實用。",
            "衣服", Audience.Men, "淺灰色",
            "eshopx/上衣/男/男款淺灰色連帽衛衣（hoodie），寬鬆版型", true, clothingSizes, 890));

        products.Add(Make("男款黑色修身長袖上衣",
            "極簡無印設計，修身剪裁展現俐落線條。彈性布料貼合身形，適合單穿或疊穿。",
            "衣服", Audience.Men, "黑色",
            "eshopx/上衣/男/男款黑色修身長袖上衣，極簡設計", true, clothingSizes, 890));

        products.Add(Make("男款米白色亞麻襯衫",
            "天然亞麻混紡，透氣吸濕。休閒版型微寬鬆，可紮入也可外穿，自然垂墜感盡顯質感。",
            "衣服", Audience.Men, "米白色",
            "eshopx/上衣/男/男款米白色亞麻襯衫，休閒版型", true, clothingSizes, 890));

        products.Add(Make("女款白色寬鬆棉質T恤",
            "柔軟純棉，親膚透氣。寬鬆落肩版型自然隨性，搭配牛仔褲或短裙皆適合。",
            "衣服", Audience.Women, "白色",
            "eshopx/上衣/女/女款白色寬鬆棉質T恤，基本款", true, clothingSizes, 890));

        products.Add(Make("女款淡藍色牛仔外套",
            "經典牛仔布料，水洗後呈現自然做舊感。版型不過長，俐落有型，四季皆可輕鬆搭配。",
            "衣服", Audience.Women, "淡藍色",
            "eshopx/上衣/女/女款淡藍色牛仔外套", true, clothingSizes, 890));

        products.Add(Make("女款粉藕色輕薄風衣",
            "輕薄防風面料，腰帶設計修飾身形比例。淡雅粉藕色優雅知性，春秋外出首選。",
            "衣服", Audience.Women, "粉藕色",
            "eshopx/上衣/女/女款粉藕色輕薄風衣，腰帶設計", true, clothingSizes, 890));

        products.Add(Make("女款黑色針織毛衣",
            "細膩針織工藝，柔軟不起球。圓領修身剪裁，勾勒優美輪廓，秋冬穿搭的百搭單品。",
            "衣服", Audience.Women, "黑色",
            "eshopx/上衣/女/女款黑色針織毛衣，圓領修身", false, clothingSizes, 890));

        products.Add(Make("女款米白色寬鬆亞麻上衣",
            "天然亞麻材質，輕盈透氣。寬鬆版型隨性舒適，自然的米白色調讓整體造型清爽乾淨。",
            "衣服", Audience.Women, "米白色",
            "eshopx/上衣/女/女款米白色寬鬆亞麻上衣", true, clothingSizes, 890));

        products.Add(Make("男款黑色修身直筒休閒褲",
            "彈性舒適面料，修身直筒版型展現俐落感。黑色百搭，上班休閒皆適合。",
            "褲子", Audience.Men, "黑色",
            "eshopx/褲子/男/男款黑色修身直筒休閒褲", true, pantsSizes, 990));

        products.Add(Make("男款深藍色牛仔褲",
            "經典深藍洗色，標準直筒版型不挑身形。耐穿耐洗，日常穿搭的必備基本款。",
            "褲子", Audience.Men, "深藍色",
            "eshopx/褲子/男/男款深藍色牛仔褲，標準版型", true, pantsSizes, 990));

        products.Add(Make("男款卡其色工裝褲",
            "多口袋機能設計，實用耐用。寬鬆版型活動自如，卡其色系耐髒百搭。",
            "褲子", Audience.Men, "卡其色",
            "eshopx/褲子/男/男款卡其色工裝褲", true, pantsSizes, 990));

        products.Add(Make("男款灰色運動縮口褲",
            "吸濕排汗彈性布料，縮口設計俐落有型。運動訓練或日常休閒都能輕鬆駕馭。",
            "褲子", Audience.Men, "灰色",
            "eshopx/褲子/男/男款灰色運動縮口褲", true, pantsSizes, 990));

        products.Add(Make("男款米白色亞麻寬褲",
            "天然亞麻材質透氣舒爽，寬鬆版型輕盈自在。米白色系清爽自然，夏日穿搭首選。",
            "褲子", Audience.Men, "米白色",
            "eshopx/褲子/男/男款米白色亞麻寬褲", true, pantsSizes, 990));

        products.Add(Make("女款黑色高腰闊腿褲",
            "高腰設計拉長腿部比例，闊腿剪裁修飾臀腿線條。垂墜感面料優雅飄逸，上班約會都合適。",
            "褲子", Audience.Women, "黑色",
            "eshopx/褲子/女/女款黑色高腰闊腿褲", true, pantsSizes, 990));

        products.Add(Make("女款淺藍色直筒牛仔褲",
            "淺藍洗色直筒版型，清爽不失俐落。彈性牛仔布料穿著舒適，四季皆可輕鬆穿搭。",
            "褲子", Audience.Women, "淺藍色",
            "eshopx/褲子/女/女款淺藍色直筒牛仔褲", true, pantsSizes, 990));

        products.Add(Make("女款米白色寬版亞麻褲",
            "天然亞麻透氣親膚，寬版剪裁舒適自在。米白色調清雅百搭，春夏穿搭質感首選。",
            "褲子", Audience.Women, "米白色",
            "eshopx/褲子/女/女款米白色寬版亞麻褲", true, pantsSizes, 990));

        products.Add(Make("女款深灰色修身九分褲",
            "修身版型展現俐落線條，九分長度露出腳踝更顯比例。深灰色沉穩百搭，通勤首選。",
            "褲子", Audience.Women, "深灰色",
            "eshopx/褲子/女/女款深灰色修身九分褲", true, pantsSizes, 990));

        products.Add(Make("女款卡其色休閒直筒褲",
            "舒適棉質面料，直筒版型不挑身形。卡其色系低調百搭，休閒通勤輕鬆切換。",
            "褲子", Audience.Women, "卡其色",
            "eshopx/褲子/女/女款卡其色休閒直筒褲", true, pantsSizes, 990));

        products.Add(Make("男款白色簡約皮革小白鞋",
            "真皮鞋面細膩光滑，簡約設計百搭各種穿搭風格。輕量鞋底舒適耐穿，日常出行首選。",
            "鞋子", Audience.Men, "白色",
            "eshopx/鞋子/男/男款白色簡約皮革小白鞋", true, shoesSizes, 1490));

        products.Add(Make("男款黑色帆布休閒鞋",
            "經典帆布材質輕盈透氣，橡膠底防滑耐磨。黑色經典百搭，校園街頭都能完美融入。",
            "鞋子", Audience.Men, "黑色",
            "eshopx/鞋子/男/男款黑色帆布休閒鞋", true, shoesSizes, 1490));

        products.Add(Make("男款米白色厚底老爹鞋",
            "復古老爹鞋廓型，厚底設計增高顯腿長。混搭材質拼接鞋面獨具個性，街頭感十足。",
            "鞋子", Audience.Men, "米白色",
            "eshopx/鞋子/男/男款米白色厚底老爹鞋", true, shoesSizes, 1490));

        products.Add(Make("男款深藍色輕量運動鞋",
            "輕量緩震鞋底，運動表現出色。透氣網布鞋面保持足部乾爽，跑步訓練日常皆宜。",
            "鞋子", Audience.Men, "深藍色",
            "eshopx/鞋子/男/男款深藍色輕量運動鞋", true, shoesSizes, 1490));

        products.Add(Make("男款棕色麂皮樂福鞋",
            "柔軟麂皮材質質感細膩，無繫帶樂福設計穿脫方便。棕色調沉穩雅緻，正式休閒皆宜。",
            "鞋子", Audience.Men, "棕色",
            "eshopx/鞋子/男/男款棕色麂皮樂福鞋", true, shoesSizes, 1490));

        products.Add(Make("女款白色皮革厚底球鞋",
            "皮革鞋面耐用好清潔，厚底設計增加身高比例。時尚造型兼具舒適，全天穿著不疲累。",
            "鞋子", Audience.Women, "白色",
            "eshopx/鞋子/女/女款白色皮革厚底球鞋", true, shoesSizes, 1490));

        products.Add(Make("女款黑色方頭低跟穆勒鞋",
            "方頭設計現代感十足，低跟不疲腳。穆勒造型優雅隨性，辦公室與約會場合皆適宜。",
            "鞋子", Audience.Women, "黑色",
            "eshopx/鞋子/女/女款黑色方頭低跟穆勒鞋", true, shoesSizes, 1490));

        products.Add(Make("女款米白色帆布懶人鞋",
            "輕薄帆布鞋面透氣舒適，無繫帶設計方便穿脫。米白色清爽耐看，春夏日常的輕便首選。",
            "鞋子", Audience.Women, "米白色",
            "eshopx/鞋子/女/女款米白色帆布懶人鞋", true, shoesSizes, 1490));

        products.Add(Make("女款裸粉色芭蕾平底鞋",
            "柔軟皮革鞋面貼合足型，芭蕾造型優雅甜美。裸粉色百搭顯白，長時間行走也舒適。",
            "鞋子", Audience.Women, "裸粉色",
            "eshopx/鞋子/女/女款裸粉色芭蕾平底鞋", true, shoesSizes, 1490));

        products.Add(Make("女款深棕色切爾西靴",
            "彈性靴筒穿脫便利，皮革鞋面質感耐用。切爾西經典設計歷久不衰，秋冬穿搭百搭款。",
            "鞋子", Audience.Women, "深棕色",
            "eshopx/鞋子/女/女款深棕色切尔西靴", true, shoesSizes, 1490));

        products.Add(Make("極簡白色棒球帽",
            "簡約無印設計，小logo點綴低調有型。棉質透氣帽身舒適耐戴，遮陽造型兩不誤。",
            "帽子", null, "白色",
            "eshopx/帽子/極簡白色棒球帽，小logo", true, hatSizes, 490));

        products.Add(Make("黑色漁夫帽",
            "寬帽沿設計全面遮陽，柔軟帽身可折疊收納。黑色百搭，街頭休閒風格的加分配件。",
            "帽子", null, "黑色",
            "eshopx/帽子/黑色漁夫帽，寬帽沿", true, hatSizes, 490));

        products.Add(Make("米白色針織毛帽",
            "柔軟針織材質保暖舒適，寬版反折設計可依需求調整高度。米白色百搭，秋冬必備單品。",
            "帽子", null, "米白色",
            "eshopx/帽子/米白色針織毛帽，寬版反折 小logo", false, hatSizes, 490));

        products.Add(Make("深藍色鴨舌帽",
            "結構帽型挺立有型，金屬調節扣可調整頭圍。深藍色沉穩百搭，日常休閒的時尚配件。",
            "帽子", null, "深藍色",
            "eshopx/帽子/深藍色鴨舌帽，金屬扣", true, hatSizes, 490));

        products.Add(Make("卡其色軍裝帽",
            "軍裝風格廓型硬挺有型，卡其色系低調耐看。帆布材質耐用透氣，戶外活動的實用配件。",
            "帽子", null, "卡其色",
            "eshopx/帽子/卡其色軍裝帽", false, hatSizes, 490));

        db.Products.AddRange(products);
        await db.SaveChangesAsync();
    }
}
