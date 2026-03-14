CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260127112007_InitialCreate') THEN
    CREATE TABLE "Orders" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "Status" integer NOT NULL,
        "TotalAmount" numeric(18,2) NOT NULL,
        "ShippingName" character varying(50) NOT NULL,
        "ShippingAddress" character varying(300) NOT NULL,
        "ShippingPhone" character varying(20) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Orders" PRIMARY KEY ("Id")
    );
    COMMENT ON COLUMN "Orders"."UserId" IS '下單使用者 ID';
    COMMENT ON COLUMN "Orders"."Status" IS '訂單狀態';
    COMMENT ON COLUMN "Orders"."TotalAmount" IS '訂單總金額';
    COMMENT ON COLUMN "Orders"."ShippingName" IS '收件人姓名';
    COMMENT ON COLUMN "Orders"."ShippingAddress" IS '收件地址';
    COMMENT ON COLUMN "Orders"."ShippingPhone" IS '收件人電話';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260127112007_InitialCreate') THEN
    CREATE TABLE "Products" (
        "Id" uuid NOT NULL,
        "Name" character varying(100) NOT NULL,
        "Description" character varying(500),
        "Price" numeric(18,2) NOT NULL,
        "StockQuantity" integer NOT NULL,
        "IsActive" boolean NOT NULL DEFAULT TRUE,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Products" PRIMARY KEY ("Id")
    );
    COMMENT ON COLUMN "Products"."Name" IS '商品名稱';
    COMMENT ON COLUMN "Products"."Description" IS '商品描述';
    COMMENT ON COLUMN "Products"."Price" IS '商品單價';
    COMMENT ON COLUMN "Products"."StockQuantity" IS '庫存數量';
    COMMENT ON COLUMN "Products"."IsActive" IS '是否上架';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260127112007_InitialCreate') THEN
    CREATE TABLE "Users" (
        "Id" uuid NOT NULL,
        "Name" character varying(50) NOT NULL,
        "Email" character varying(200) NOT NULL,
        "Phone" character varying(20) NOT NULL,
        "Address" character varying(300),
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
    );
    COMMENT ON COLUMN "Users"."Name" IS '使用者名稱';
    COMMENT ON COLUMN "Users"."Email" IS '電子信箱';
    COMMENT ON COLUMN "Users"."Phone" IS '聯絡電話';
    COMMENT ON COLUMN "Users"."Address" IS '聯絡地址';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260127112007_InitialCreate') THEN
    CREATE TABLE "OrderItems" (
        "Id" uuid NOT NULL,
        "OrderId" uuid NOT NULL,
        "ProductId" uuid NOT NULL,
        "ProductName" character varying(100) NOT NULL,
        "UnitPrice" numeric(18,2) NOT NULL,
        "Quantity" integer NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_OrderItems" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OrderItems_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("Id") ON DELETE CASCADE
    );
    COMMENT ON COLUMN "OrderItems"."OrderId" IS '所屬訂單 ID';
    COMMENT ON COLUMN "OrderItems"."ProductId" IS '商品 ID（記錄用）';
    COMMENT ON COLUMN "OrderItems"."ProductName" IS '商品名稱（快照）';
    COMMENT ON COLUMN "OrderItems"."UnitPrice" IS '商品單價（快照）';
    COMMENT ON COLUMN "OrderItems"."Quantity" IS '購買數量';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260127112007_InitialCreate') THEN
    CREATE INDEX "IX_OrderItems_OrderId" ON "OrderItems" ("OrderId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260127112007_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260127112007_InitialCreate', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260130033716_jwtTokenEntity') THEN
    ALTER TABLE "Users" ADD "PasswordHash" character varying(500) NOT NULL DEFAULT '';
    COMMENT ON COLUMN "Users"."PasswordHash" IS '密碼雜湊值';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260130033716_jwtTokenEntity') THEN
    CREATE TABLE "RefreshTokens" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "Token" character varying(200) NOT NULL,
        "ExpireAt" timestamp with time zone NOT NULL,
        "IsRevoked" boolean NOT NULL DEFAULT FALSE,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_RefreshTokens" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RefreshTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    COMMENT ON COLUMN "RefreshTokens"."Token" IS 'Refresh Token 值';
    COMMENT ON COLUMN "RefreshTokens"."ExpireAt" IS '過期時間';
    COMMENT ON COLUMN "RefreshTokens"."IsRevoked" IS '是否已撤銷';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260130033716_jwtTokenEntity') THEN
    CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260130033716_jwtTokenEntity') THEN
    CREATE UNIQUE INDEX "IX_RefreshTokens_Token" ON "RefreshTokens" ("Token");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260130033716_jwtTokenEntity') THEN
    CREATE INDEX "IX_RefreshTokens_UserId" ON "RefreshTokens" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260130033716_jwtTokenEntity') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260130033716_jwtTokenEntity', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260130173839_addCart') THEN
    CREATE TABLE "Carts" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Carts" PRIMARY KEY ("Id")
    );
    COMMENT ON COLUMN "Carts"."UserId" IS '使用者 ID';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260130173839_addCart') THEN
    CREATE TABLE "CartItems" (
        "Id" uuid NOT NULL,
        "CartId" uuid NOT NULL,
        "ProductId" uuid NOT NULL,
        "Quantity" integer NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_CartItems" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_CartItems_Carts_CartId" FOREIGN KEY ("CartId") REFERENCES "Carts" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_CartItems_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT
    );
    COMMENT ON COLUMN "CartItems"."CartId" IS '購物車 ID';
    COMMENT ON COLUMN "CartItems"."ProductId" IS '商品 ID';
    COMMENT ON COLUMN "CartItems"."Quantity" IS '數量';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260130173839_addCart') THEN
    CREATE UNIQUE INDEX "IX_CartItem_CartId_ProductId" ON "CartItems" ("CartId", "ProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260130173839_addCart') THEN
    CREATE INDEX "IX_CartItems_ProductId" ON "CartItems" ("ProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260130173839_addCart') THEN
    CREATE UNIQUE INDEX "IX_Cart_UserId" ON "Carts" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260130173839_addCart') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260130173839_addCart', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260131060632_addProductImageAndUserImage') THEN
    ALTER TABLE "Users" ADD "AvatarBytes" bigint;
    COMMENT ON COLUMN "Users"."AvatarBytes" IS '頭像大小(Bytes)';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260131060632_addProductImageAndUserImage') THEN
    ALTER TABLE "Users" ADD "AvatarFormat" character varying(20);
    COMMENT ON COLUMN "Users"."AvatarFormat" IS '頭像格式';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260131060632_addProductImageAndUserImage') THEN
    ALTER TABLE "Users" ADD "AvatarHeight" integer;
    COMMENT ON COLUMN "Users"."AvatarHeight" IS '頭像高度';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260131060632_addProductImageAndUserImage') THEN
    ALTER TABLE "Users" ADD "AvatarPublicId" character varying(255);
    COMMENT ON COLUMN "Users"."AvatarPublicId" IS '頭像 PublicId';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260131060632_addProductImageAndUserImage') THEN
    ALTER TABLE "Users" ADD "AvatarUrl" character varying(2048);
    COMMENT ON COLUMN "Users"."AvatarUrl" IS '頭像 URL';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260131060632_addProductImageAndUserImage') THEN
    ALTER TABLE "Users" ADD "AvatarWidth" integer;
    COMMENT ON COLUMN "Users"."AvatarWidth" IS '頭像寬度';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260131060632_addProductImageAndUserImage') THEN
    CREATE TABLE "ProductImages" (
        "Id" uuid NOT NULL,
        "ProductId" uuid NOT NULL,
        "Url" character varying(2048) NOT NULL,
        "PublicId" character varying(255) NOT NULL,
        "Format" character varying(20) NOT NULL,
        "Width" integer NOT NULL,
        "Height" integer NOT NULL,
        "Bytes" bigint NOT NULL,
        "IsPrimary" boolean NOT NULL DEFAULT FALSE,
        "SortOrder" integer NOT NULL DEFAULT 0,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ProductImages" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ProductImages_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE CASCADE
    );
    COMMENT ON COLUMN "ProductImages"."ProductId" IS '商品 ID';
    COMMENT ON COLUMN "ProductImages"."Url" IS '圖片 URL';
    COMMENT ON COLUMN "ProductImages"."PublicId" IS '圖片 PublicId';
    COMMENT ON COLUMN "ProductImages"."Format" IS '圖片格式';
    COMMENT ON COLUMN "ProductImages"."Width" IS '圖片寬度';
    COMMENT ON COLUMN "ProductImages"."Height" IS '圖片高度';
    COMMENT ON COLUMN "ProductImages"."Bytes" IS '圖片大小(Bytes)';
    COMMENT ON COLUMN "ProductImages"."IsPrimary" IS '是否為封面圖';
    COMMENT ON COLUMN "ProductImages"."SortOrder" IS '排序';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260131060632_addProductImageAndUserImage') THEN
    CREATE INDEX "IX_ProductImages_ProductId" ON "ProductImages" ("ProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260131060632_addProductImageAndUserImage') THEN
    CREATE INDEX "IX_ProductImages_ProductId_IsPrimary" ON "ProductImages" ("ProductId", "IsPrimary");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260131060632_addProductImageAndUserImage') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260131060632_addProductImageAndUserImage', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260202091051_addExternalLogin') THEN
    CREATE TABLE "ExternalLogins" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "LoginProvider" character varying(50) NOT NULL,
        "ProviderUserId" character varying(200) NOT NULL,
        "EmailAtLinkTime" character varying(200) NOT NULL,
        "LastLoginAt" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ExternalLogins" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ExternalLogins_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    COMMENT ON COLUMN "ExternalLogins"."UserId" IS '使用者 Id';
    COMMENT ON COLUMN "ExternalLogins"."LoginProvider" IS '外部登入提供者';
    COMMENT ON COLUMN "ExternalLogins"."ProviderUserId" IS '外部使用者唯一識別(sub)';
    COMMENT ON COLUMN "ExternalLogins"."EmailAtLinkTime" IS '綁定時的 Email';
    COMMENT ON COLUMN "ExternalLogins"."LastLoginAt" IS '最後登入時間';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260202091051_addExternalLogin') THEN
    CREATE UNIQUE INDEX "IX_ExternalLogins_LoginProvider_ProviderUserId" ON "ExternalLogins" ("LoginProvider", "ProviderUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260202091051_addExternalLogin') THEN
    CREATE INDEX "IX_ExternalLogins_UserId" ON "ExternalLogins" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260202091051_addExternalLogin') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260202091051_addExternalLogin', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260202153802_addHomepageEntities') THEN
    CREATE TABLE "Banners" (
        "Id" uuid NOT NULL,
        "Title" character varying(100) NOT NULL,
        "ImageUrl" character varying(500) NOT NULL,
        "Link" character varying(500) NOT NULL,
        "SortOrder" integer NOT NULL DEFAULT 0,
        "IsActive" boolean NOT NULL DEFAULT TRUE,
        "StartsAt" timestamp with time zone,
        "EndsAt" timestamp with time zone,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Banners" PRIMARY KEY ("Id")
    );
    COMMENT ON COLUMN "Banners"."Title" IS 'Banner 標題';
    COMMENT ON COLUMN "Banners"."ImageUrl" IS '圖片網址';
    COMMENT ON COLUMN "Banners"."Link" IS '點擊連結';
    COMMENT ON COLUMN "Banners"."SortOrder" IS '排序順序';
    COMMENT ON COLUMN "Banners"."IsActive" IS '是否啟用';
    COMMENT ON COLUMN "Banners"."StartsAt" IS '生效開始時間';
    COMMENT ON COLUMN "Banners"."EndsAt" IS '生效結束時間';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260202153802_addHomepageEntities') THEN
    CREATE TABLE "Categories" (
        "Id" uuid NOT NULL,
        "Name" character varying(50) NOT NULL,
        "Icon" character varying(50) NOT NULL,
        "Link" character varying(200) NOT NULL,
        "SortOrder" integer NOT NULL DEFAULT 0,
        "IsActive" boolean NOT NULL DEFAULT TRUE,
        "ParentId" uuid,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Categories" PRIMARY KEY ("Id")
    );
    COMMENT ON COLUMN "Categories"."Name" IS '分類名稱';
    COMMENT ON COLUMN "Categories"."Icon" IS '圖標（emoji 或 icon name）';
    COMMENT ON COLUMN "Categories"."Link" IS '點擊連結';
    COMMENT ON COLUMN "Categories"."SortOrder" IS '排序順序';
    COMMENT ON COLUMN "Categories"."IsActive" IS '是否啟用';
    COMMENT ON COLUMN "Categories"."ParentId" IS '父分類 ID（支援子分類）';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260202153802_addHomepageEntities') THEN
    CREATE TABLE "FlashSales" (
        "Id" uuid NOT NULL,
        "Title" character varying(100) NOT NULL,
        "Subtitle" character varying(200),
        "StartsAt" timestamp with time zone NOT NULL,
        "EndsAt" timestamp with time zone NOT NULL,
        "IsActive" boolean NOT NULL DEFAULT TRUE,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_FlashSales" PRIMARY KEY ("Id")
    );
    COMMENT ON COLUMN "FlashSales"."Title" IS '秒殺活動標題';
    COMMENT ON COLUMN "FlashSales"."Subtitle" IS '副標題說明';
    COMMENT ON COLUMN "FlashSales"."StartsAt" IS '活動開始時間';
    COMMENT ON COLUMN "FlashSales"."EndsAt" IS '活動結束時間';
    COMMENT ON COLUMN "FlashSales"."IsActive" IS '是否啟用';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260202153802_addHomepageEntities') THEN
    CREATE TABLE "ProductRecommends" (
        "Id" uuid NOT NULL,
        "ProductId" uuid NOT NULL,
        "RecommendType" character varying(50) NOT NULL DEFAULT 'homepage',
        "SortOrder" integer NOT NULL DEFAULT 0,
        "IsActive" boolean NOT NULL DEFAULT TRUE,
        "StartsAt" timestamp with time zone,
        "EndsAt" timestamp with time zone,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ProductRecommends" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ProductRecommends_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE CASCADE
    );
    COMMENT ON COLUMN "ProductRecommends"."ProductId" IS '商品 ID';
    COMMENT ON COLUMN "ProductRecommends"."RecommendType" IS '推薦類型（homepage、category、similar 等）';
    COMMENT ON COLUMN "ProductRecommends"."SortOrder" IS '排序順序';
    COMMENT ON COLUMN "ProductRecommends"."IsActive" IS '是否啟用';
    COMMENT ON COLUMN "ProductRecommends"."StartsAt" IS '生效開始時間';
    COMMENT ON COLUMN "ProductRecommends"."EndsAt" IS '生效結束時間';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260202153802_addHomepageEntities') THEN
    CREATE TABLE "FlashSaleSlots" (
        "Id" uuid NOT NULL,
        "FlashSaleId" uuid NOT NULL,
        "Label" character varying(20) NOT NULL,
        "StartsAt" timestamp with time zone NOT NULL,
        "EndsAt" timestamp with time zone NOT NULL,
        "SortOrder" integer NOT NULL DEFAULT 0,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_FlashSaleSlots" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_FlashSaleSlots_FlashSales_FlashSaleId" FOREIGN KEY ("FlashSaleId") REFERENCES "FlashSales" ("Id") ON DELETE CASCADE
    );
    COMMENT ON COLUMN "FlashSaleSlots"."FlashSaleId" IS '所屬秒殺活動';
    COMMENT ON COLUMN "FlashSaleSlots"."Label" IS '場次標籤（如 10:00）';
    COMMENT ON COLUMN "FlashSaleSlots"."StartsAt" IS '場次開始時間';
    COMMENT ON COLUMN "FlashSaleSlots"."EndsAt" IS '場次結束時間';
    COMMENT ON COLUMN "FlashSaleSlots"."SortOrder" IS '排序順序';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260202153802_addHomepageEntities') THEN
    CREATE TABLE "FlashSaleItems" (
        "Id" uuid NOT NULL,
        "FlashSaleId" uuid NOT NULL,
        "SlotId" uuid,
        "ProductId" uuid NOT NULL,
        "FlashPrice" numeric(18,2) NOT NULL,
        "StockTotal" integer NOT NULL,
        "StockRemaining" integer NOT NULL,
        "Badge" character varying(20),
        "SortOrder" integer NOT NULL DEFAULT 0,
        "PurchaseLimit" integer NOT NULL DEFAULT 1,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_FlashSaleItems" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_FlashSaleItems_FlashSaleSlots_SlotId" FOREIGN KEY ("SlotId") REFERENCES "FlashSaleSlots" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_FlashSaleItems_FlashSales_FlashSaleId" FOREIGN KEY ("FlashSaleId") REFERENCES "FlashSales" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_FlashSaleItems_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT
    );
    COMMENT ON COLUMN "FlashSaleItems"."FlashSaleId" IS '所屬秒殺活動';
    COMMENT ON COLUMN "FlashSaleItems"."SlotId" IS '所屬場次（可選）';
    COMMENT ON COLUMN "FlashSaleItems"."ProductId" IS '商品 ID';
    COMMENT ON COLUMN "FlashSaleItems"."FlashPrice" IS '秒殺價';
    COMMENT ON COLUMN "FlashSaleItems"."StockTotal" IS '秒殺總庫存';
    COMMENT ON COLUMN "FlashSaleItems"."StockRemaining" IS '剩餘庫存';
    COMMENT ON COLUMN "FlashSaleItems"."Badge" IS '標籤（Hot、限量、新品等）';
    COMMENT ON COLUMN "FlashSaleItems"."SortOrder" IS '排序順序';
    COMMENT ON COLUMN "FlashSaleItems"."PurchaseLimit" IS '每人限購數量';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260202153802_addHomepageEntities') THEN
    CREATE INDEX "IX_Banners_IsActive_SortOrder" ON "Banners" ("IsActive", "SortOrder");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260202153802_addHomepageEntities') THEN
    CREATE INDEX "IX_Categories_IsActive_SortOrder" ON "Categories" ("IsActive", "SortOrder");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260202153802_addHomepageEntities') THEN
    CREATE INDEX "IX_Categories_ParentId" ON "Categories" ("ParentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260202153802_addHomepageEntities') THEN
    CREATE INDEX "IX_FlashSaleItems_FlashSaleId_SlotId_SortOrder" ON "FlashSaleItems" ("FlashSaleId", "SlotId", "SortOrder");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260202153802_addHomepageEntities') THEN
    CREATE INDEX "IX_FlashSaleItems_ProductId" ON "FlashSaleItems" ("ProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260202153802_addHomepageEntities') THEN
    CREATE INDEX "IX_FlashSaleItems_SlotId" ON "FlashSaleItems" ("SlotId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260202153802_addHomepageEntities') THEN
    CREATE INDEX "IX_FlashSales_IsActive_StartsAt_EndsAt" ON "FlashSales" ("IsActive", "StartsAt", "EndsAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260202153802_addHomepageEntities') THEN
    CREATE INDEX "IX_FlashSaleSlots_FlashSaleId_SortOrder" ON "FlashSaleSlots" ("FlashSaleId", "SortOrder");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260202153802_addHomepageEntities') THEN
    CREATE INDEX "IX_ProductRecommends_ProductId" ON "ProductRecommends" ("ProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260202153802_addHomepageEntities') THEN
    CREATE INDEX "IX_ProductRecommends_RecommendType_IsActive_SortOrder" ON "ProductRecommends" ("RecommendType", "IsActive", "SortOrder");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260202153802_addHomepageEntities') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260202153802_addHomepageEntities', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260203073616_AddUserRole') THEN
    ALTER TABLE "Users" ADD "IsAdmin" boolean NOT NULL DEFAULT FALSE;
    COMMENT ON COLUMN "Users"."IsAdmin" IS '是否為管理員';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260203073616_AddUserRole') THEN
    ALTER TABLE "Users" ADD "IsSeller" boolean NOT NULL DEFAULT FALSE;
    COMMENT ON COLUMN "Users"."IsSeller" IS '是否為賣家';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260203073616_AddUserRole') THEN
    ALTER TABLE "Users" ADD "SellerAppliedAt" timestamp with time zone;
    COMMENT ON COLUMN "Users"."SellerAppliedAt" IS '賣家申請時間';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260203073616_AddUserRole') THEN
    ALTER TABLE "Users" ADD "SellerApprovedAt" timestamp with time zone;
    COMMENT ON COLUMN "Users"."SellerApprovedAt" IS '賣家審核通過時間';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260203073616_AddUserRole') THEN
    ALTER TABLE "Users" ADD "SellerApprovedBy" uuid;
    COMMENT ON COLUMN "Users"."SellerApprovedBy" IS '審核的管理員 ID';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260203073616_AddUserRole') THEN
    ALTER TABLE "Users" ADD "SellerRejectionReason" character varying(500);
    COMMENT ON COLUMN "Users"."SellerRejectionReason" IS '拒絕原因';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260203073616_AddUserRole') THEN
    ALTER TABLE "Users" ADD "SellerStatus" integer;
    COMMENT ON COLUMN "Users"."SellerStatus" IS '賣家申請狀態：0=申請中, 1=已通過, 2=已拒絕';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260203073616_AddUserRole') THEN
    ALTER TABLE "Products" ADD "SellerId" uuid;
    COMMENT ON COLUMN "Products"."SellerId" IS '賣家 ID';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260203073616_AddUserRole') THEN
    CREATE INDEX "IX_Products_SellerId" ON "Products" ("SellerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260203073616_AddUserRole') THEN
    ALTER TABLE "Products" ADD CONSTRAINT "FK_Products_Users_SellerId" FOREIGN KEY ("SellerId") REFERENCES "Users" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260203073616_AddUserRole') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260203073616_AddUserRole', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260203103926_AddProductCategoryId') THEN
    ALTER TABLE "Products" ADD "CategoryId" uuid;
    COMMENT ON COLUMN "Products"."CategoryId" IS '分類 ID';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260203103926_AddProductCategoryId') THEN
    CREATE INDEX "IX_Products_CategoryId" ON "Products" ("CategoryId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260203103926_AddProductCategoryId') THEN
    ALTER TABLE "Products" ADD CONSTRAINT "FK_Products_Categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "Categories" ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260203103926_AddProductCategoryId') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260203103926_AddProductCategoryId', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260203151311_updateOrder') THEN
    ALTER TABLE "Orders" ADD "PaidAt" timestamp with time zone;
    COMMENT ON COLUMN "Orders"."PaidAt" IS '付款時間';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260203151311_updateOrder') THEN
    ALTER TABLE "Orders" ADD "PaymentMethod" character varying(30) NOT NULL DEFAULT '';
    COMMENT ON COLUMN "Orders"."PaymentMethod" IS '付款方式';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260203151311_updateOrder') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260203151311_updateOrder', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260208060539_AddFlashSaleTables') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260208060539_AddFlashSaleTables', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260208082532_AddReviews') THEN
    CREATE TABLE "Reviews" (
        "Id" uuid NOT NULL,
        "OrderId" uuid NOT NULL,
        "OrderItemId" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "ProductId" uuid NOT NULL,
        "Rating" integer NOT NULL,
        "Content" character varying(1000),
        "IsAnonymous" boolean NOT NULL DEFAULT FALSE,
        "UpdatedAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Reviews" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Reviews_OrderItems_OrderItemId" FOREIGN KEY ("OrderItemId") REFERENCES "OrderItems" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_Reviews_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_Reviews_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_Reviews_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    COMMENT ON COLUMN "Reviews"."Rating" IS '評分 1~5';
    COMMENT ON COLUMN "Reviews"."Content" IS '評價內容';
    COMMENT ON COLUMN "Reviews"."IsAnonymous" IS '是否匿名';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260208082532_AddReviews') THEN
    CREATE TABLE "ReviewImages" (
        "Id" uuid NOT NULL,
        "ReviewId" uuid NOT NULL,
        "Url" character varying(500) NOT NULL,
        "SortOrder" integer NOT NULL DEFAULT 0,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ReviewImages" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ReviewImages_Reviews_ReviewId" FOREIGN KEY ("ReviewId") REFERENCES "Reviews" ("Id") ON DELETE CASCADE
    );
    COMMENT ON COLUMN "ReviewImages"."Url" IS '圖片 URL';
    COMMENT ON COLUMN "ReviewImages"."SortOrder" IS '排序';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260208082532_AddReviews') THEN
    CREATE INDEX "IX_ReviewImages_ReviewId" ON "ReviewImages" ("ReviewId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260208082532_AddReviews') THEN
    CREATE INDEX "IX_Reviews_OrderId" ON "Reviews" ("OrderId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260208082532_AddReviews') THEN
    CREATE UNIQUE INDEX "IX_Reviews_OrderItemId" ON "Reviews" ("OrderItemId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260208082532_AddReviews') THEN
    CREATE INDEX "IX_Reviews_ProductId" ON "Reviews" ("ProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260208082532_AddReviews') THEN
    CREATE INDEX "IX_Reviews_UserId" ON "Reviews" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260208082532_AddReviews') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260208082532_AddReviews', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260212065313_ApplicationLog') THEN
    CREATE TABLE "ApplicationLogs" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "Message" text NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ApplicationLogs" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260212065313_ApplicationLog') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260212065313_ApplicationLog', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260212073302_AddScopeIdToApplicationLog') THEN
    ALTER TABLE "ApplicationLogs" ADD "ScopeId" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260212073302_AddScopeIdToApplicationLog') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260212073302_AddScopeIdToApplicationLog', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260212095416_AddBannerImageMetadata') THEN
    ALTER TABLE "Banners" ADD "ImageBytes" bigint;
    COMMENT ON COLUMN "Banners"."ImageBytes" IS '圖片大小(Byte)';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260212095416_AddBannerImageMetadata') THEN
    ALTER TABLE "Banners" ADD "ImageFormat" character varying(20);
    COMMENT ON COLUMN "Banners"."ImageFormat" IS '圖片格式';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260212095416_AddBannerImageMetadata') THEN
    ALTER TABLE "Banners" ADD "ImageHeight" integer;
    COMMENT ON COLUMN "Banners"."ImageHeight" IS '圖片高度';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260212095416_AddBannerImageMetadata') THEN
    ALTER TABLE "Banners" ADD "ImagePublicId" character varying(200);
    COMMENT ON COLUMN "Banners"."ImagePublicId" IS '圖片 PublicId';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260212095416_AddBannerImageMetadata') THEN
    ALTER TABLE "Banners" ADD "ImageWidth" integer;
    COMMENT ON COLUMN "Banners"."ImageWidth" IS '圖片寬度';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260212095416_AddBannerImageMetadata') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260212095416_AddBannerImageMetadata', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260213152712_AddOutboxEvents') THEN
    CREATE TABLE "OutboxEvents" (
        "Id" uuid NOT NULL,
        "EventType" character varying(100) NOT NULL,
        "PayloadJson" text NOT NULL,
        "Status" character varying(20) NOT NULL,
        "RetryCount" integer NOT NULL,
        "NextRetryAt" timestamp with time zone,
        "ProcessedAt" timestamp with time zone,
        "LastError" character varying(2000),
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_OutboxEvents" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260213152712_AddOutboxEvents') THEN
    CREATE INDEX "IX_OutboxEvents_Status_NextRetryAt" ON "OutboxEvents" ("Status", "NextRetryAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260213152712_AddOutboxEvents') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260213152712_AddOutboxEvents', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260214054821_AddProcessedEvents') THEN
    CREATE TABLE "ProcessedEvents" (
        "Id" uuid NOT NULL,
        "Source" character varying(100) NOT NULL,
        "EventId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ProcessedEvents" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260214054821_AddProcessedEvents') THEN
    CREATE UNIQUE INDEX "IX_ProcessedEvents_Source_EventId" ON "ProcessedEvents" ("Source", "EventId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260214054821_AddProcessedEvents') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260214054821_AddProcessedEvents', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260214145224_RemoveFlashSaleTables') THEN
    DROP TABLE "FlashSaleItems";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260214145224_RemoveFlashSaleTables') THEN
    DROP TABLE "FlashSaleSlots";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260214145224_RemoveFlashSaleTables') THEN
    DROP TABLE "FlashSales";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260214145224_RemoveFlashSaleTables') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260214145224_RemoveFlashSaleTables', '10.0.2');
    END IF;
END $EF$;
COMMIT;

