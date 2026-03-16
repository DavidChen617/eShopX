using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initialize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Carts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false, comment: "所屬使用者 ID"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carts", x => x.Id);
                },
                comment: "購物車（每位使用者一個）");

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "分類名稱（唯一）"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                },
                comment: "商品分類");

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false, comment: "下單使用者 ID"),
                    Status = table.Column<int>(type: "integer", nullable: false, comment: "訂單狀態（待付款、已付款、已出貨、已完成）"),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false, comment: "訂單總金額"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                },
                comment: "訂單主表");

            migrationBuilder.CreateTable(
                name: "OutboxEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "事件類型名稱（如 PaymentPaidEvent）"),
                    Payload = table.Column<string>(type: "text", nullable: false, comment: "事件序列化內容（JSON）"),
                    Status = table.Column<int>(type: "integer", nullable: false, comment: "處理狀態（Pending、Processed、Failed）"),
                    RetryCount = table.Column<int>(type: "integer", nullable: false, comment: "已重試次數（上限 3 次後標記為 Failed）"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "事件成功處理的時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxEvents", x => x.Id);
                },
                comment: "Outbox 事件佇列（保證至少一次送出 Domain Event）");

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false, comment: "關聯訂單 ID"),
                    Method = table.Column<int>(type: "integer", nullable: false, comment: "付款方式（LinePay、PayPal、ECPay 等）"),
                    Status = table.Column<int>(type: "integer", nullable: false, comment: "付款狀態（待付款、已付款、失敗）"),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false, comment: "付款金額"),
                    TransactionId = table.Column<string>(type: "text", nullable: true, comment: "第三方金流交易序號"),
                    PaymentUrl = table.Column<string>(type: "text", nullable: true, comment: "付款頁面網址（由金流方產生）"),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "實際付款完成時間"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                },
                comment: "付款記錄");

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "商品名稱"),
                    Description = table.Column<string>(type: "text", nullable: true, comment: "商品描述"),
                    Audience = table.Column<int>(type: "integer", nullable: true, comment: "適用客群（男、女、中性）"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, comment: "是否上架販售"),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false, comment: "所屬分類 ID"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                },
                comment: "商品主表");

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false, comment: "所屬使用者 ID"),
                    Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "Token 值（唯一）"),
                    ExpireAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Token 到期時間"),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false, comment: "是否已被撤銷"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                },
                comment: "使用者 Refresh Token");

            migrationBuilder.CreateTable(
                name: "Shipments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false, comment: "關聯訂單 ID"),
                    LogisticsId = table.Column<string>(type: "text", nullable: false, comment: "綠界物流訂單編號"),
                    LogisticsSubType = table.Column<int>(type: "integer", nullable: false, comment: "物流子類型（FAMIC2C、UNIMARTC2C、TCAT 等）"),
                    LogisticsStatus = table.Column<string>(type: "text", nullable: false, comment: "物流狀態代碼（來自綠界回調）"),
                    LogisticsStatusName = table.Column<string>(type: "text", nullable: false, comment: "物流狀態說明文字"),
                    UpdateStatusDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "最後物流狀態更新時間"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ShipmentType = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    ReceiverName = table.Column<string>(type: "text", nullable: true, comment: "收件人姓名"),
                    ReceiverPhone = table.Column<string>(type: "text", nullable: true, comment: "收件人手機號碼"),
                    StoreId = table.Column<string>(type: "text", nullable: true, comment: "超商門市代號"),
                    StoreName = table.Column<string>(type: "text", nullable: true, comment: "超商門市名稱"),
                    CVSPaymentNo = table.Column<string>(type: "text", nullable: true, comment: "超商繳費代碼（貨到付款時使用）"),
                    ZipCode = table.Column<string>(type: "text", nullable: true, comment: "收件地址郵遞區號"),
                    Address = table.Column<string>(type: "text", nullable: true, comment: "收件詳細地址"),
                    BookingNote = table.Column<string>(type: "text", nullable: true, comment: "宅配備註（預約時段等）")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shipments", x => x.Id);
                },
                comment: "物流出貨記錄（TPH：CVS 超取 / Home 宅配）");

            migrationBuilder.CreateTable(
                name: "Sizes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "尺寸名稱（S、M、L、XL 等）"),
                    Type = table.Column<int>(type: "integer", nullable: false, comment: "尺寸分類（衣服、鞋子等）"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sizes", x => x.Id);
                },
                comment: "尺寸選項");

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "標籤名稱（唯一）"),
                    Type = table.Column<int>(type: "integer", nullable: false, comment: "標籤類型（材質、風格、季節等）"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                },
                comment: "商品標籤");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "顯示名稱"),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "電子信箱（唯一）"),
                    Roles = table.Column<int>(type: "integer", nullable: false, comment: "角色旗標（Flags enum，可複選）"),
                    Avatar_Url = table.Column<string>(type: "text", nullable: true, comment: "頭像圖片網址"),
                    Avatar_PublicId = table.Column<string>(type: "text", nullable: true, comment: "Cloudinary Public ID"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                },
                comment: "使用者主表");

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CartId = table.Column<Guid>(type: "uuid", nullable: false, comment: "所屬購物車 ID"),
                    SkuId = table.Column<Guid>(type: "uuid", nullable: false, comment: "商品 SKU ID"),
                    Quantity = table.Column<int>(type: "integer", nullable: false, comment: "數量")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItems_Carts_CartId",
                        column: x => x.CartId,
                        principalTable: "Carts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "購物車項目");

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false, comment: "所屬訂單 ID"),
                    SkuId = table.Column<Guid>(type: "uuid", nullable: false, comment: "下單時的 SKU ID"),
                    ProductName = table.Column<string>(type: "text", nullable: false, comment: "下單當下的商品名稱快照"),
                    Color = table.Column<string>(type: "text", nullable: false, comment: "下單當下的顏色快照"),
                    Size = table.Column<string>(type: "text", nullable: true, comment: "下單當下的尺寸快照"),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false, comment: "下單當下的單價快照"),
                    Quantity = table.Column<int>(type: "integer", nullable: false, comment: "購買數量"),
                    TotalPrice = table.Column<decimal>(type: "numeric", nullable: false, comment: "該明細小計（單價 × 數量）")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "訂單明細");

            migrationBuilder.CreateTable(
                name: "ProductTags",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false, comment: "商品 ID"),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false, comment: "標籤 ID")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTags", x => new { x.ProductId, x.TagId });
                    table.ForeignKey(
                        name: "FK_ProductTags_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "商品與標籤的對應關係");

            migrationBuilder.CreateTable(
                name: "ProductVariants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false, comment: "所屬商品 ID"),
                    Color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "顏色名稱")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductVariants_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "商品款式（依顏色區分）");

            migrationBuilder.CreateTable(
                name: "UserAuthProviders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false, comment: "所屬使用者 ID"),
                    Provider = table.Column<int>(type: "integer", nullable: false, comment: "登入提供者（Local、Google、Line）"),
                    ProviderUserId = table.Column<string>(type: "text", nullable: true, comment: "第三方平台的使用者識別碼（sub）"),
                    PasswordHash = table.Column<string>(type: "text", nullable: true, comment: "本地登入的雜湊密碼（第三方登入為 null）"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAuthProviders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAuthProviders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "使用者登入方式（本地密碼 / 第三方 OAuth）");

            migrationBuilder.CreateTable(
                name: "ProductImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false, comment: "所屬款式 ID"),
                    Url = table.Column<string>(type: "text", nullable: false, comment: "圖片網址"),
                    PublicId = table.Column<string>(type: "text", nullable: false, comment: "Cloudinary Public ID，用於更新或刪除"),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, comment: "是否為主圖"),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, comment: "排序順序（由小到大）")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductImages_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "商品圖片");

            migrationBuilder.CreateTable(
                name: "ProductSkus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false, comment: "所屬款式 ID"),
                    SizeId = table.Column<Guid>(type: "uuid", nullable: true, comment: "所屬尺寸 ID（無尺寸商品為 null）"),
                    Price = table.Column<decimal>(type: "numeric", nullable: false, comment: "售價"),
                    StockQuantity = table.Column<int>(type: "integer", nullable: false, comment: "目前庫存數量"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductSkus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductSkus_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "商品 SKU（款式 × 尺寸的最小庫存單位）");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId",
                table: "CartItems",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEvents_Status",
                table: "OutboxEvents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductVariantId",
                table: "ProductImages",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSkus_ProductVariantId",
                table: "ProductSkus",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId",
                table: "ProductVariants",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAuthProviders_UserId",
                table: "UserAuthProviders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "OutboxEvents");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "ProductImages");

            migrationBuilder.DropTable(
                name: "ProductSkus");

            migrationBuilder.DropTable(
                name: "ProductTags");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Shipments");

            migrationBuilder.DropTable(
                name: "Sizes");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "UserAuthProviders");

            migrationBuilder.DropTable(
                name: "Carts");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "ProductVariants");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
