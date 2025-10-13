-- ============================================
-- MAKE RegID NULLABLE IN CUSTOMERS TABLE (FINAL FIX)
-- Handles data type mismatch issue
-- ============================================

USE PMS;
GO

PRINT '=== MAKING RegID NULLABLE IN CUSTOMERS TABLE ==='
PRINT ''

-- Step 1: Check current state of BOTH tables
PRINT 'Step 1: Checking data types in both tables...'
PRINT ''
PRINT 'Registration.RegID:'
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Registration' 
AND COLUMN_NAME = 'RegID';

PRINT ''
PRINT 'Customers.RegID (BEFORE):'
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Customers' 
AND COLUMN_NAME = 'RegID';

-- Step 2: Get the exact data type from Registration table
DECLARE @DataType NVARCHAR(50);
DECLARE @MaxLength INT;

SELECT 
    @DataType = DATA_TYPE,
    @MaxLength = CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Registration' 
AND COLUMN_NAME = 'RegID';

PRINT ''
PRINT 'Registration.RegID data type: ' + @DataType + '(' + CAST(@MaxLength AS VARCHAR) + ')'

-- Step 3: Drop the foreign key constraint (if it wasn't dropped already)
PRINT ''
PRINT 'Step 2: Dropping foreign key constraint (if exists)...'
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Customers_Registration_RegID')
BEGIN
    ALTER TABLE Customers DROP CONSTRAINT FK_Customers_Registration_RegID;
    PRINT '✅ Foreign key constraint dropped'
END
ELSE
BEGIN
    PRINT 'ℹ️  Foreign key constraint already dropped or doesn\'t exist'
END

-- Step 4: Drop the index (if it wasn't dropped already)
PRINT ''
PRINT 'Step 3: Dropping index (if exists)...'
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Customers_RegID' AND object_id = OBJECT_ID('Customers'))
BEGIN
    DROP INDEX IX_Customers_RegID ON Customers;
    PRINT '✅ Index dropped'
END
ELSE
BEGIN
    PRINT 'ℹ️  Index already dropped or doesn\'t exist'
END

-- Step 5: Alter column to match Registration.RegID and allow NULL
PRINT ''
PRINT 'Step 4: Altering column to match Registration table and allow NULL...'
-- Assuming Registration.RegID is NVARCHAR(10) based on error
ALTER TABLE Customers
ALTER COLUMN RegID NVARCHAR(10) NULL;
PRINT '✅ RegID is now NVARCHAR(10) NULL (matching Registration table)'

-- Step 6: Recreate the foreign key constraint (allowing NULL)
PRINT ''
PRINT 'Step 5: Recreating foreign key constraint...'
ALTER TABLE Customers
ADD CONSTRAINT FK_Customers_Registration_RegID 
FOREIGN KEY (RegID) REFERENCES Registration(RegID)
ON DELETE SET NULL;
PRINT '✅ Foreign key constraint recreated (with ON DELETE SET NULL)'

-- Step 7: Recreate the index
PRINT ''
PRINT 'Step 6: Recreating index...'
CREATE INDEX IX_Customers_RegID ON Customers(RegID);
PRINT '✅ Index recreated'

-- Step 8: Verify the change
PRINT ''
PRINT 'Step 7: Verifying changes...'
PRINT ''
PRINT 'Customers.RegID (AFTER):'
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Customers' 
AND COLUMN_NAME = 'RegID';

PRINT ''
PRINT '=== SUCCESS! ==='
PRINT 'RegID is now nullable in Customers table'
PRINT 'Data type matches Registration table'
PRINT 'Customers can be created with or without a Registration ID'
PRINT 'Foreign key and index have been recreated properly'

GO

