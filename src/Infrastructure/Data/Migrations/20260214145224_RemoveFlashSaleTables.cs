using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFlashSaleTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlashSaleItems");

            migrationBuilder.DropTable(
                name: "FlashSaleSlots");

            migrationBuilder.DropTable(
                name: "FlashSales");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FlashSales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "活動結束時間"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true, comment: "是否啟用"),
                    StartsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "活動開始時間"),
                    Subtitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true, comment: "副標題說明"),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "秒殺活動標題"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlashSales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FlashSaleSlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FlashSaleId = table.Column<Guid>(type: "uuid", nullable: false, comment: "所屬秒殺活動"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "場次結束時間"),
                    Label = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "場次標籤（如 10:00）"),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "排序順序"),
                    StartsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "場次開始時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlashSaleSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlashSaleSlots_FlashSales_FlashSaleId",
                        column: x => x.FlashSaleId,
                        principalTable: "FlashSales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FlashSaleItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FlashSaleId = table.Column<Guid>(type: "uuid", nullable: false, comment: "所屬秒殺活動"),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false, comment: "商品 ID"),
                    SlotId = table.Column<Guid>(type: "uuid", nullable: true, comment: "所屬場次（可選）"),
                    Badge = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, comment: "標籤（Hot、限量、新品等）"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FlashPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, comment: "秒殺價"),
                    PurchaseLimit = table.Column<int>(type: "integer", nullable: false, defaultValue: 1, comment: "每人限購數量"),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "排序順序"),
                    StockRemaining = table.Column<int>(type: "integer", nullable: false, comment: "剩餘庫存"),
                    StockTotal = table.Column<int>(type: "integer", nullable: false, comment: "秒殺總庫存")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlashSaleItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlashSaleItems_FlashSaleSlots_SlotId",
                        column: x => x.SlotId,
                        principalTable: "FlashSaleSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FlashSaleItems_FlashSales_FlashSaleId",
                        column: x => x.FlashSaleId,
                        principalTable: "FlashSales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlashSaleItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlashSaleItems_FlashSaleId_SlotId_SortOrder",
                table: "FlashSaleItems",
                columns: new[] { "FlashSaleId", "SlotId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_FlashSaleItems_ProductId",
                table: "FlashSaleItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_FlashSaleItems_SlotId",
                table: "FlashSaleItems",
                column: "SlotId");

            migrationBuilder.CreateIndex(
                name: "IX_FlashSales_IsActive_StartsAt_EndsAt",
                table: "FlashSales",
                columns: new[] { "IsActive", "StartsAt", "EndsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_FlashSaleSlots_FlashSaleId_SortOrder",
                table: "FlashSaleSlots",
                columns: new[] { "FlashSaleId", "SortOrder" });
        }
    }
}
