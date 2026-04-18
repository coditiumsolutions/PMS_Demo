-- Adds MobileNo, MobileNo2 to Customers; makes Phone NOT NULL; backfills legacy rows.
-- Prefer: dotnet ef database update (uses EF migration 20260418064755_AddCustomerMobileNoAndMobileNo2).
-- Run against the target database if applying manually.

IF COL_LENGTH(N'dbo.Customers', N'MobileNo') IS NULL
BEGIN
    ALTER TABLE dbo.Customers ADD MobileNo NVARCHAR(50) NULL;
END
GO

IF COL_LENGTH(N'dbo.Customers', N'MobileNo2') IS NULL
BEGIN
    ALTER TABLE dbo.Customers ADD MobileNo2 NVARCHAR(50) NULL;
END
GO

UPDATE dbo.Customers
SET Phone = N'00000000000'
WHERE Phone IS NULL OR LTRIM(RTRIM(Phone)) = N'';
GO

-- Phone NOT NULL (adjust default constraint name if needed)
ALTER TABLE dbo.Customers ALTER COLUMN Phone NVARCHAR(50) NOT NULL;
GO

UPDATE dbo.Customers
SET MobileNo = Phone
WHERE MobileNo IS NULL;

UPDATE c
SET c.MobileNo = LEFT(c.Phone, 49) + N'9'
FROM dbo.Customers c
WHERE REPLACE(REPLACE(REPLACE(ISNULL(c.Phone, N''), N'+', N''), N'-', N''), N' ', N'')
    = REPLACE(REPLACE(REPLACE(ISNULL(c.MobileNo, N''), N'+', N''), N'-', N''), N' ', N'')
  AND LEN(REPLACE(REPLACE(REPLACE(ISNULL(c.Phone, N''), N'+', N''), N'-', N''), N' ', N'')) > 0;
GO

ALTER TABLE dbo.Customers ALTER COLUMN MobileNo NVARCHAR(50) NOT NULL;
GO
