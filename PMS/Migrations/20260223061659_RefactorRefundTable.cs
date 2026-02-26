using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Migrations
{
    /// <inheritdoc />
    public partial class RefactorRefundTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Refund",
                columns: table => new
                {
                    RefundID       = table.Column<string>(type: "char(10)", maxLength: 10, nullable: false),
                    CustomerID     = table.Column<string>(type: "char(10)", maxLength: 10, nullable: true),
                    RefundType     = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PaidAmount     = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    DeductionAmount= table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    RefundedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    Reason         = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    WorkflowStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, defaultValue: "Initiated"),
                    SelectedPaymentIDs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy      = table.Column<string>(type: "char(10)", maxLength: 10, nullable: true),
                    CreatedAt      = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ApprovedBy     = table.Column<string>(type: "char(10)", maxLength: 10, nullable: true),
                    ApprovedAt     = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes          = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Refund", x => x.RefundID);
                    table.ForeignKey("FK_Refund_Customers_CustomerID", x => x.CustomerID, "Customers", "CustomerID", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_Refund_Users_ApprovedBy",    x => x.ApprovedBy,  "Users",     "UserID",     onDelete: ReferentialAction.NoAction);
                    table.ForeignKey("FK_Refund_Users_CreatedBy",     x => x.CreatedBy,   "Users",     "UserID",     onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex("IX_Refund_CustomerID", "Refund", "CustomerID");
            migrationBuilder.CreateIndex("IX_Refund_CreatedBy",  "Refund", "CreatedBy");
            migrationBuilder.CreateIndex("IX_Refund_ApprovedBy", "Refund", "ApprovedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Users_AuditedBy",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Refund_Users_ApprovedBy",
                table: "Refund");

            migrationBuilder.DropForeignKey(
                name: "FK_Refund_Users_CreatedBy",
                table: "Refund");

            migrationBuilder.DropIndex(
                name: "IX_Refund_CreatedBy",
                table: "Refund");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Refund");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Refund");

            migrationBuilder.DropColumn(
                name: "DeductionAmount",
                table: "Refund");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Refund");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "Refund");

            migrationBuilder.DropColumn(
                name: "SelectedPaymentIDs",
                table: "Refund");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                table: "Refund");

            migrationBuilder.RenameColumn(
                name: "RefundedAmount",
                table: "Refund",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "RefundType",
                table: "Refund",
                newName: "Status");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerID",
                table: "Refund",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "char(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ApprovedBy",
                table: "Refund",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "char(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RefundID",
                table: "Refund",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(10)",
                oldMaxLength: 10);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Users_AuditedBy",
                table: "Payments",
                column: "AuditedBy",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Refund_Users_ApprovedBy",
                table: "Refund",
                column: "ApprovedBy",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
