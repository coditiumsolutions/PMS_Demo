-- Add new columns to NDC table (run on existing database if table already exists)
USE PMS;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('NDC') AND name = 'CreatedAt')
    ALTER TABLE NDC ADD CreatedAt DATETIME DEFAULT GETDATE();
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('NDC') AND name = 'CreatedBy')
    ALTER TABLE NDC ADD CreatedBy NVARCHAR(100) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('NDC') AND name = 'NDCExpiryDate')
    ALTER TABLE NDC ADD NDCExpiryDate DATE NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('NDC') AND name = 'TotalDueAmount')
    ALTER TABLE NDC ADD TotalDueAmount DECIMAL(18,2) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('NDC') AND name = 'TotalDueInstallments')
    ALTER TABLE NDC ADD TotalDueInstallments DECIMAL(18,2) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('NDC') AND name = 'AllPaymentClear')
    ALTER TABLE NDC ADD AllPaymentClear BIT NOT NULL DEFAULT 0;
GO
