-- Alter ActivityLog.RefID from CHAR(10) / nvarchar(10) to nvarchar(100)
-- Run this if you prefer SQL over EF migration, or run: dotnet ef database update

USE PMS;
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ActivityLog')
BEGIN
    ALTER TABLE ActivityLog
    ALTER COLUMN RefID NVARCHAR(100) NULL;
END
GO
