-- Add missing audit columns to Payments (fixes "Invalid column name 'LastModified'" when using Record Payment search).
-- Run this once on your database (e.g. the one used by http://172.20.228.2:84).

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Payments') AND name = 'CreatedBy')
BEGIN
    ALTER TABLE Payments ADD CreatedBy NVARCHAR(10) NULL;
END
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Payments') AND name = 'CreatedAt')
BEGIN
    ALTER TABLE Payments ADD CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE();
END
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Payments') AND name = 'LastModified')
BEGIN
    ALTER TABLE Payments ADD LastModified DATETIME2 NULL;
END
GO

-- Optional: add index and FK only if Users table has UserID (e.g. standard PMS schema)
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'UserID')
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Payments_Users_CreatedBy')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('Payments') AND name = 'IX_Payments_CreatedBy')
        CREATE INDEX IX_Payments_CreatedBy ON Payments(CreatedBy);
    ALTER TABLE Payments ADD CONSTRAINT FK_Payments_Users_CreatedBy
        FOREIGN KEY (CreatedBy) REFERENCES Users(UserID) ON DELETE SET NULL;
END
GO
