using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceTransferTableWithNewSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Replace old Transfer table with new schema (CustomerID, WorkFlowStatus, Seller/Buyer, etc.)
            // Use SQL so we can do IF EXISTS and avoid errors if already applied or table was replaced by script.
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'dbo.Transfer', N'U') IS NOT NULL
                BEGIN
                    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Transfer_Property_PropertyID')
                        ALTER TABLE [dbo].[Transfer] DROP CONSTRAINT [FK_Transfer_Property_PropertyID];
                    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Transfer_Customers_FromCustomerID')
                        ALTER TABLE [dbo].[Transfer] DROP CONSTRAINT [FK_Transfer_Customers_FromCustomerID];
                    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Transfer_Customers_ToCustomerID')
                        ALTER TABLE [dbo].[Transfer] DROP CONSTRAINT [FK_Transfer_Customers_ToCustomerID];
                    DROP TABLE [dbo].[Transfer];
                END
            ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'dbo.Transfer', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[Transfer] (
                        [TransferID] nvarchar(50) NOT NULL,
                        [CustomerID] char(10) NOT NULL,
                        [WorkFlowStatus] nvarchar(100) NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [SellerName] nvarchar(200) NULL,
                        [SellerFatherName] nvarchar(200) NULL,
                        [SellerCNIC] nvarchar(200) NULL,
                        [SellerContact] nvarchar(200) NULL,
                        [SellerAddress] nvarchar(200) NULL,
                        [BuyerName] nvarchar(200) NULL,
                        [BuyerFatherName] nvarchar(200) NULL,
                        [BuyerCNIC] nvarchar(200) NULL,
                        [BuyerContact] nvarchar(200) NULL,
                        [BuyerAddress] nvarchar(200) NULL,
                        [BuyerCity] nvarchar(200) NULL,
                        [BuyerCountry] nvarchar(200) NULL,
                        [BuyerAttachments] nvarchar(max) NULL,
                        [SellerAttachments] nvarchar(max) NULL,
                        [TransferFeeDue] float NULL,
                        [TransferFeePaid] float NULL,
                        [PaymentDate] date NULL,
                        [PaymentMode] nvarchar(200) NULL,
                        [PaymentChallanNo] nvarchar(200) NULL,
                        [Details] nvarchar(max) NULL,
                        [CROComments] nvarchar(max) NULL,
                        [AccountsComments] nvarchar(max) NULL,
                        [TransferComments] nvarchar(max) NULL,
                        CONSTRAINT [PK_Transfer] PRIMARY KEY ([TransferID]),
                        CONSTRAINT [FK_Transfer_Customers_CustomerID] FOREIGN KEY ([CustomerID]) REFERENCES [Customers] ([CustomerID])
                    );
                    CREATE INDEX [IX_Transfer_CustomerID] ON [dbo].[Transfer] ([CustomerID]);
                    CREATE INDEX [IX_Transfer_WorkFlowStatus] ON [dbo].[Transfer] ([WorkFlowStatus]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transfer_Customers_CustomerID",
                table: "Transfer");

            migrationBuilder.DropTable(
                name: "Transfer");

            migrationBuilder.CreateTable(
                name: "Transfer",
                columns: table => new
                {
                    TransferID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FromCustomerID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ToCustomerID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PropertyID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transfer", x => x.TransferID);
                    table.ForeignKey(
                        name: "FK_Transfer_Customers_FromCustomerID",
                        column: x => x.FromCustomerID,
                        principalTable: "Customers",
                        principalColumn: "CustomerID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transfer_Customers_ToCustomerID",
                        column: x => x.ToCustomerID,
                        principalTable: "Customers",
                        principalColumn: "CustomerID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transfer_Property_PropertyID",
                        column: x => x.PropertyID,
                        principalTable: "Property",
                        principalColumn: "PropertyID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transfer_FromCustomerID",
                table: "Transfer",
                column: "FromCustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_Transfer_PropertyID",
                table: "Transfer",
                column: "PropertyID");

            migrationBuilder.CreateIndex(
                name: "IX_Transfer_ToCustomerID",
                table: "Transfer",
                column: "ToCustomerID");
        }
    }
}
