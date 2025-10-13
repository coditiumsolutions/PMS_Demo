-- ============================================
-- MAKE RegID NULLABLE IN CUSTOMERS TABLE (FIXED)
-- Properly handles foreign key and index dependencies
-- ============================================

USE PMS;
GO

PRINT '=== MAKING RegID NULLABLE IN CUSTOMERS TABLE ==='
PRINT ''

-- Step 1: Check current state
PRINT 'Step 1: Checking current state...'
SELECT 
    COLUMN_NAME,
    IS_NULLABLE,
    DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Customers' 
AND COLUMN_NAME = 'RegID';

-- Step 2: Drop the foreign key constraint
PRINT ''
PRINT 'Step 2: Dropping foreign key constraint...'
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Customers_Registration_RegID')
BEGIN
    ALTER TABLE Customers DROP CONSTRAINT FK_Customers_Registration_RegID;
    PRINT '✅ Foreign key constraint dropped'
END
ELSE
BEGIN
    PRINT 'ℹ️  No foreign key constraint found'
END

-- Step 3: Drop the index
PRINT ''
PRINT 'Step 3: Dropping index...'
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Customers_RegID' AND object_id = OBJECT_ID('Customers'))
BEGIN
    DROP INDEX IX_Customers_RegID ON Customers;
    PRINT '✅ Index dropped'
END
ELSE
BEGIN
    PRINT 'ℹ️  No index found'
END

-- Step 4: Alter column to allow NULL
PRINT ''
PRINT 'Step 4: Altering column to allow NULL...'
ALTER TABLE Customers
ALTER COLUMN RegID CHAR(10) NULL;
PRINT '✅ RegID is now NULLABLE'

-- Step 5: Recreate the foreign key constraint (allowing NULL)
PRINT ''
PRINT 'Step 5: Recreating foreign key constraint...'
ALTER TABLE Customers
ADD CONSTRAINT FK_Customers_Registration_RegID 
FOREIGN KEY (RegID) REFERENCES Registration(RegID)
ON DELETE SET NULL;
PRINT '✅ Foreign key constraint recreated (with ON DELETE SET NULL)'

-- Step 6: Recreate the index
PRINT ''
PRINT 'Step 6: Recreating index...'
CREATE INDEX IX_Customers_RegID ON Customers(RegID);
PRINT '✅ Index recreated'

-- Step 7: Verify the change
PRINT ''
PRINT 'Step 7: Verifying changes...'
SELECT 
    COLUMN_NAME,
    IS_NULLABLE,
    DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Customers' 
AND COLUMN_NAME = 'RegID';

PRINT ''
PRINT '=== SUCCESS! ==='
PRINT 'RegID is now nullable in Customers table'
PRINT 'Customers can be created with or without a Registration ID'
PRINT 'Foreign key and index have been recreated properly'

GO

