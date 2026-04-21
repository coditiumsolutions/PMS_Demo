using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PMS.Data;

#nullable disable

namespace PMS.Migrations
{
    [DbContext(typeof(PMSDbContext))]
    [Migration("20260420073000_UpdatePaymentScheduleSurchargeRatePrecision")]
    /// <inheritdoc />
    public partial class UpdatePaymentScheduleSurchargeRatePrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "SurchargeRate",
                table: "PaymentSchedule",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "SurchargeRate",
                table: "PaymentSchedule",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");
        }
    }
}
