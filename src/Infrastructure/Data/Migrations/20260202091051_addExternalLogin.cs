using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class addExternalLogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExternalLogins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false, comment: "使用者 Id"),
                    LoginProvider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "外部登入提供者"),
                    ProviderUserId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "外部使用者唯一識別(sub)"),
                    EmailAtLinkTime = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "綁定時的 Email"),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "最後登入時間"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalLogins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalLogins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalLogins_LoginProvider_ProviderUserId",
                table: "ExternalLogins",
                columns: new[] { "LoginProvider", "ProviderUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalLogins_UserId",
                table: "ExternalLogins",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalLogins");
        }
    }
}