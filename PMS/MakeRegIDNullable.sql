-- ============================================
-- MAKE RegID NULLABLE IN CUSTOMERS TABLE
-- Allow customers to be created without registration
-- ============================================

USE PMS;
GO

PRINT '=== MAKING RegID NULLABLE IN CUSTOMERS TABLE ==='

-- Check current constraint
SELECT 
    COLUMN_NAME,
    IS_NULLABLE,
    DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Customers' 
AND COLUMN_NAME = 'RegID';

-- Alter column to allow NULL
ALTER TABLE Customers
ALTER COLUMN RegID CHAR(10) NULL;

PRINT '✅ RegID is now NULLABLE'
PRINT ''
PRINT 'Customers can now be created with or without a Registration ID'
PRINT ''

-- Verify change
SELECT 
    COLUMN_NAME,
    IS_NULLABLE,
    DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Customers' 
AND COLUMN_NAME = 'RegID';

GO

