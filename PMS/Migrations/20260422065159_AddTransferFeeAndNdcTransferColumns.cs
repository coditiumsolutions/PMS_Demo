using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Migrations
{
    /// <inheritdoc />
    public partial class AddTransferFeeAndNdcTransferColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('NDC', 'AmountPerUnit') IS NULL
    ALTER TABLE [NDC] ADD [AmountPerUnit] decimal(18,2) NULL;
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('NDC', 'PropertySize') IS NULL
    ALTER TABLE [NDC] ADD [PropertySize] decimal(18,2) NULL;
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('NDC', 'TransferFeeAmount') IS NULL
    ALTER TABLE [NDC] ADD [TransferFeeAmount] decimal(18,2) NULL;
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[TransferFee]', N'U') IS NULL
BEGIN
    CREATE TABLE [TransferFee] (
        [Id] char(10) NOT NULL,
        [ProjectID] char(10) NOT NULL,
        [SubProject] nvarchar(100) NULL,
        [TransferType] nvarchar(100) NULL,
        [TransferPriority] nvarchar(20) NULL,
        [AmountPerUnit] decimal(18,2) NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [CreatedBy] char(10) NULL,
        [ModifiedBy] char(10) NULL,
        [Details] nvarchar(max) NULL,
        CONSTRAINT [PK_TransferFee] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TransferFee_Projects_ProjectID] FOREIGN KEY ([ProjectID]) REFERENCES [Projects]([ProjectID]) ON DELETE CASCADE
    );
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TransferFee_ProjectID' AND object_id = OBJECT_ID('TransferFee'))
    CREATE INDEX [IX_TransferFee_ProjectID] ON [TransferFee]([ProjectID]);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransferFee");

            migrationBuilder.DropColumn(
                name: "AmountPerUnit",
                table: "NDC");

            migrationBuilder.DropColumn(
                name: "PropertySize",
                table: "NDC");

            migrationBuilder.DropColumn(
                name: "TransferFeeAmount",
                table: "NDC");
        }
    }
}
