using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Migrations
{
    /// <inheritdoc />
    public partial class AddNDCColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // If NDC table does not exist, create it with all columns (original + new)
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[NDC]', N'U') IS NULL
BEGIN
    CREATE TABLE [NDC] (
        [NDCID] char(10) NOT NULL,
        [CustomerID] char(10) NULL,
        [NDCType] nvarchar(100) NULL,
        [Title] nvarchar(500) NULL,
        [WorkFlowStatus] nvarchar(500) NULL,
        [Comments] nvarchar(max) NULL,
        [IssuedDate] datetime2 NOT NULL DEFAULT GETDATE(),
        [Remarks] nvarchar(255) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [CreatedBy] nvarchar(100) NULL,
        [NDCExpiryDate] date NULL,
        [TotalDueAmount] decimal(18,2) NULL,
        [TotalDueInstallments] decimal(18,2) NULL,
        [AllPaymentClear] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_NDC] PRIMARY KEY ([NDCID]),
        CONSTRAINT [FK_NDC_Customers_CustomerID] FOREIGN KEY ([CustomerID]) REFERENCES [Customers] ([CustomerID]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_NDC_CustomerID] ON [NDC] ([CustomerID]);
END
ELSE
BEGIN
    -- Table exists: add new columns only if missing
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[NDC]') AND name = N'CreatedAt')
        ALTER TABLE [NDC] ADD [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE();
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[NDC]') AND name = N'CreatedBy')
        ALTER TABLE [NDC] ADD [CreatedBy] nvarchar(100) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[NDC]') AND name = N'NDCExpiryDate')
        ALTER TABLE [NDC] ADD [NDCExpiryDate] date NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[NDC]') AND name = N'TotalDueAmount')
        ALTER TABLE [NDC] ADD [TotalDueAmount] decimal(18,2) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[NDC]') AND name = N'TotalDueInstallments')
        ALTER TABLE [NDC] ADD [TotalDueInstallments] decimal(18,2) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[NDC]') AND name = N'AllPaymentClear')
        ALTER TABLE [NDC] ADD [AllPaymentClear] bit NOT NULL DEFAULT 0;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "CreatedAt", table: "NDC");
            migrationBuilder.DropColumn(name: "CreatedBy", table: "NDC");
            migrationBuilder.DropColumn(name: "NDCExpiryDate", table: "NDC");
            migrationBuilder.DropColumn(name: "TotalDueAmount", table: "NDC");
            migrationBuilder.DropColumn(name: "TotalDueInstallments", table: "NDC");
            migrationBuilder.DropColumn(name: "AllPaymentClear", table: "NDC");
        }
    }
}
