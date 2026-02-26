-- Add Floor column to Property table (MSSQL)
-- Run this if you update the database manually instead of using EF migrations.

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Property') AND name = N'Floor'
)
BEGIN
    ALTER TABLE dbo.Property
    ADD Floor NVARCHAR(50) NULL;
END
GO
