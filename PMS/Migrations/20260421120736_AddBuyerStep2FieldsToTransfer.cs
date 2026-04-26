using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Migrations
{
    /// <inheritdoc />
    public partial class AddBuyerStep2FieldsToTransfer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BuyerDOB",
                table: "Transfer",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyerEmail",
                table: "Transfer",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyerGender",
                table: "Transfer",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyerMailingAddress",
                table: "Transfer",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyerMobile",
                table: "Transfer",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyerMobile2",
                table: "Transfer",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyerNationality",
                table: "Transfer",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyerPermanentAddress",
                table: "Transfer",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyerPhone",
                table: "Transfer",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyerDOB",
                table: "Transfer");

            migrationBuilder.DropColumn(
                name: "BuyerEmail",
                table: "Transfer");

            migrationBuilder.DropColumn(
                name: "BuyerGender",
                table: "Transfer");

            migrationBuilder.DropColumn(
                name: "BuyerMailingAddress",
                table: "Transfer");

            migrationBuilder.DropColumn(
                name: "BuyerMobile",
                table: "Transfer");

            migrationBuilder.DropColumn(
                name: "BuyerMobile2",
                table: "Transfer");

            migrationBuilder.DropColumn(
                name: "BuyerNationality",
                table: "Transfer");

            migrationBuilder.DropColumn(
                name: "BuyerPermanentAddress",
                table: "Transfer");

            migrationBuilder.DropColumn(
                name: "BuyerPhone",
                table: "Transfer");
        }
    }
}
