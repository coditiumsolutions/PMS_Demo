using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Migrations
{
    /// <inheritdoc />
    public partial class RebuildWaiverModuleAndAccountHead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Reason",
                table: "Waiver",
                newName: "WaiverType");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Waiver",
                newName: "WaivedAmount");

            migrationBuilder.RenameColumn(
                name: "Reason",
                table: "Penalties",
                newName: "PenaltyReason");

            migrationBuilder.RenameColumn(
                name: "AppliedOn",
                table: "Penalties",
                newName: "PenaltyDate");

            migrationBuilder.AddColumn<string>(
                name: "AccountHead",
                table: "Waiver",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                defaultValue: "Waived Off");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Waiver",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Comments",
                table: "Waiver",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Waiver",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "Waiver",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Waiver",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                defaultValue: "Initiated");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "Waiver",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "WaivedPercentage",
                table: "Waiver",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountHead",
                table: "Payments",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Waiver_CreatedBy",
                table: "Waiver",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Waiver_LastModifiedBy",
                table: "Waiver",
                column: "LastModifiedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Waiver_Users_CreatedBy",
                table: "Waiver",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Waiver_Users_LastModifiedBy",
                table: "Waiver",
                column: "LastModifiedBy",
                principalTable: "Users",
                principalColumn: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Waiver_Users_CreatedBy",
                table: "Waiver");

            migrationBuilder.DropForeignKey(
                name: "FK_Waiver_Users_LastModifiedBy",
                table: "Waiver");

            migrationBuilder.DropIndex(
                name: "IX_Waiver_CreatedBy",
                table: "Waiver");

            migrationBuilder.DropIndex(
                name: "IX_Waiver_LastModifiedBy",
                table: "Waiver");

            migrationBuilder.DropColumn(
                name: "AccountHead",
                table: "Waiver");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Waiver");

            migrationBuilder.DropColumn(
                name: "Comments",
                table: "Waiver");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Waiver");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Waiver");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Waiver");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "Waiver");

            migrationBuilder.DropColumn(
                name: "WaivedPercentage",
                table: "Waiver");

            migrationBuilder.DropColumn(
                name: "AccountHead",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "WaiverType",
                table: "Waiver",
                newName: "Reason");

            migrationBuilder.RenameColumn(
                name: "WaivedAmount",
                table: "Waiver",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "PenaltyReason",
                table: "Penalties",
                newName: "Reason");

            migrationBuilder.RenameColumn(
                name: "PenaltyDate",
                table: "Penalties",
                newName: "AppliedOn");
        }
    }
}
