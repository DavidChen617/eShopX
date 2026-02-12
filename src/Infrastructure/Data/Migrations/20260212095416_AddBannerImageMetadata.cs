using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBannerImageMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ImageBytes",
                table: "Banners",
                type: "bigint",
                nullable: true,
                comment: "圖片大小(Byte)");

            migrationBuilder.AddColumn<string>(
                name: "ImageFormat",
                table: "Banners",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                comment: "圖片格式");

            migrationBuilder.AddColumn<int>(
                name: "ImageHeight",
                table: "Banners",
                type: "integer",
                nullable: true,
                comment: "圖片高度");

            migrationBuilder.AddColumn<string>(
                name: "ImagePublicId",
                table: "Banners",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                comment: "圖片 PublicId");

            migrationBuilder.AddColumn<int>(
                name: "ImageWidth",
                table: "Banners",
                type: "integer",
                nullable: true,
                comment: "圖片寬度");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageBytes",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "ImageFormat",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "ImageHeight",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "ImagePublicId",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "ImageWidth",
                table: "Banners");
        }
    }
}
