using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false, comment: "下單使用者 ID"),
                    Status = table.Column<int>(type: "integer", nullable: false, comment: "訂單狀態"),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, comment: "訂單總金額"),
                    ShippingName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "收件人姓名"),
                    ShippingAddress = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false, comment: "收件地址"),
                    ShippingPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "收件人電話"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "商品名稱"),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "商品描述"),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, comment: "商品單價"),
                    StockQuantity = table.Column<int>(type: "integer", nullable: false, comment: "庫存數量"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true, comment: "是否上架"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "使用者名稱"),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "電子信箱"),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "聯絡電話"),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true, comment: "聯絡地址"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false, comment: "所屬訂單 ID"),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false, comment: "商品 ID（記錄用）"),
                    ProductName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "商品名稱（快照）"),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, comment: "商品單價（快照）"),
                    Quantity = table.Column<int>(type: "integer", nullable: false, comment: "購買數量"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
