using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class addProductImageAndUserImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AvatarBytes",
                table: "Users",
                type: "bigint",
                nullable: true,
                comment: "頭像大小(Bytes)");

            migrationBuilder.AddColumn<string>(
                name: "AvatarFormat",
                table: "Users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                comment: "頭像格式");

            migrationBuilder.AddColumn<int>(
                name: "AvatarHeight",
                table: "Users",
                type: "integer",
                nullable: true,
                comment: "頭像高度");

            migrationBuilder.AddColumn<string>(
                name: "AvatarPublicId",
                table: "Users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                comment: "頭像 PublicId");

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Users",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true,
                comment: "頭像 URL");

            migrationBuilder.AddColumn<int>(
                name: "AvatarWidth",
                table: "Users",
                type: "integer",
                nullable: true,
                comment: "頭像寬度");

            migrationBuilder.CreateTable(
                name: "ProductImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false, comment: "商品 ID"),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false, comment: "圖片 URL"),
                    PublicId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "圖片 PublicId"),
                    Format = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "圖片格式"),
                    Width = table.Column<int>(type: "integer", nullable: false, comment: "圖片寬度"),
                    Height = table.Column<int>(type: "integer", nullable: false, comment: "圖片高度"),
                    Bytes = table.Column<long>(type: "bigint", nullable: false, comment: "圖片大小(Bytes)"),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "是否為封面圖"),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "排序"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductImages_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId",
                table: "ProductImages",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId_IsPrimary",
                table: "ProductImages",
                columns: new[] { "ProductId", "IsPrimary" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductImages");

            migrationBuilder.DropColumn(
                name: "AvatarBytes",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AvatarFormat",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AvatarHeight",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AvatarPublicId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AvatarWidth",
                table: "Users");
        }
    }
}