using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuditRemarks",
                table: "Payments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditStatus",
                table: "Payments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditedAt",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditedBy",
                table: "Payments",
                type: "char(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_AuditedBy",
                table: "Payments",
                column: "AuditedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Users_AuditedBy",
                table: "Payments",
                column: "AuditedBy",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Users_AuditedBy",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_AuditedBy",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "AuditRemarks",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "AuditStatus",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "AuditedAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "AuditedBy",
                table: "Payments");
        }
    }
}
