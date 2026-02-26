using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Migrations
{
    /// <inheritdoc />
    public partial class AddRentalModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Rental",
                columns: table => new
                {
                    RentalID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PropertyID = table.Column<string>(type: "char(10)", maxLength: 10, nullable: false),
                    TenantName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TenantCNIC = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TenantPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TenantEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    TenantAddress = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MonthlyRent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SecurityDeposit = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AdvanceRent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "PKR"),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationMonths = table.Column<int>(type: "int", nullable: false),
                    PaymentDueDayOfMonth = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Active"),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rental", x => x.RentalID);
                    table.ForeignKey(
                        name: "FK_Rental_Property_PropertyID",
                        column: x => x.PropertyID,
                        principalTable: "Property",
                        principalColumn: "PropertyID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RentalPayments",
                columns: table => new
                {
                    RentalPaymentID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RentalID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BillingYear = table.Column<int>(type: "int", nullable: false),
                    BillingMonth = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AmountDue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReferenceNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentalPayments", x => x.RentalPaymentID);
                    table.ForeignKey(
                        name: "FK_RentalPayments_Rental_RentalID",
                        column: x => x.RentalID,
                        principalTable: "Rental",
                        principalColumn: "RentalID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rental_PropertyID",
                table: "Rental",
                column: "PropertyID",
                unique: true,
                filter: "[Status] = 'Active'");

            migrationBuilder.CreateIndex(
                name: "IX_Rental_Status_StartDate",
                table: "Rental",
                columns: new[] { "Status", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_RentalPayments_BillingYear_BillingMonth",
                table: "RentalPayments",
                columns: new[] { "BillingYear", "BillingMonth" });

            migrationBuilder.CreateIndex(
                name: "IX_RentalPayments_RentalID",
                table: "RentalPayments",
                column: "RentalID");

            migrationBuilder.CreateIndex(
                name: "IX_RentalPayments_Status_DueDate",
                table: "RentalPayments",
                columns: new[] { "Status", "DueDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RentalPayments");

            migrationBuilder.DropTable(
                name: "Rental");
        }
    }
}
