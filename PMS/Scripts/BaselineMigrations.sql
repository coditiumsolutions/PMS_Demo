-- Baseline EF migrations: mark existing migrations as applied so "dotnet ef database update" does not recreate tables.
-- Run this against your PMS database when the schema already exists (e.g. created from db.txt) but __EFMigrationsHistory is empty.
-- Connect to your database (e.g. -d PMS or -d PMSAbbas) when running this script.

IF OBJECT_ID(N'[__EFMigrationsHistory]', N'U') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END
GO

-- Insert all current migrations so EF considers them already applied (do not insert duplicates)
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20251106101633_RequireAllottmentTypeAndWorkFlowStatus_InAllotment')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20251106101633_RequireAllottmentTypeAndWorkFlowStatus_InAllotment', N'8.0.10');
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20251110184904_AddNationalityAndKinDocumentsToCustomer')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20251110184904_AddNationalityAndKinDocumentsToCustomer', N'8.0.10');
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20251111075310_AddCurrencySupportToPaymentPlan')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20251111075310_AddCurrencySupportToPaymentPlan', N'8.0.10');
GO

PRINT 'Baseline complete. You can now run: dotnet ef database update';
GO
