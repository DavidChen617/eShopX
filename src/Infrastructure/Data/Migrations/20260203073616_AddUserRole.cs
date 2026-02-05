using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "是否為管理員");

            migrationBuilder.AddColumn<bool>(
                name: "IsSeller",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "是否為賣家");

            migrationBuilder.AddColumn<DateTime>(
                name: "SellerAppliedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true,
                comment: "賣家申請時間");

            migrationBuilder.AddColumn<DateTime>(
                name: "SellerApprovedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true,
                comment: "賣家審核通過時間");

            migrationBuilder.AddColumn<Guid>(
                name: "SellerApprovedBy",
                table: "Users",
                type: "uuid",
                nullable: true,
                comment: "審核的管理員 ID");

            migrationBuilder.AddColumn<string>(
                name: "SellerRejectionReason",
                table: "Users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "拒絕原因");

            migrationBuilder.AddColumn<int>(
                name: "SellerStatus",
                table: "Users",
                type: "integer",
                nullable: true,
                comment: "賣家申請狀態：0=申請中, 1=已通過, 2=已拒絕");

            migrationBuilder.AddColumn<Guid>(
                name: "SellerId",
                table: "Products",
                type: "uuid",
                nullable: true,
                comment: "賣家 ID");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SellerId",
                table: "Products",
                column: "SellerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Users_SellerId",
                table: "Products",
                column: "SellerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Users_SellerId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_SellerId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsSeller",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SellerAppliedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SellerApprovedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SellerApprovedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SellerRejectionReason",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SellerStatus",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SellerId",
                table: "Products");
        }
    }
}