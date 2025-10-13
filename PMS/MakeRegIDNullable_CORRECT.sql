-- ============================================
-- MAKE RegID NULLABLE IN CUSTOMERS TABLE
-- Properly handles foreign key and index dependencies
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

-- Step 2: Drop the foreign key constraint
PRINT ''
PRINT 'Step 2: Dropping foreign key constraint...'
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Customers_Registration_RegID')
BEGIN
    ALTER TABLE Customers DROP CONSTRAINT FK_Customers_Registration_RegID;
    PRINT 'Foreign key constraint dropped'
END
ELSE
BEGIN
    PRINT 'No foreign key constraint found'
END

-- Step 3: Drop the index
PRINT ''
PRINT 'Step 3: Dropping index...'
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Customers_RegID' AND object_id = OBJECT_ID('Customers'))
BEGIN
    DROP INDEX IX_Customers_RegID ON Customers;
    PRINT 'Index dropped'
END
ELSE
BEGIN
    PRINT 'No index found'
END

-- Step 4: Alter column to NVARCHAR(10) NULL (matching Registration table)
PRINT ''
PRINT 'Step 4: Altering column to NVARCHAR(10) NULL...'
ALTER TABLE Customers
ALTER COLUMN RegID NVARCHAR(10) NULL;
PRINT 'RegID is now NVARCHAR(10) NULL'

-- Step 5: Recreate the foreign key constraint (allowing NULL)
PRINT ''
PRINT 'Step 5: Recreating foreign key constraint...'
ALTER TABLE Customers
ADD CONSTRAINT FK_Customers_Registration_RegID 
FOREIGN KEY (RegID) REFERENCES Registration(RegID)
ON DELETE SET NULL;
PRINT 'Foreign key constraint recreated with ON DELETE SET NULL'

-- Step 6: Recreate the index
PRINT ''
PRINT 'Step 6: Recreating index...'
CREATE INDEX IX_Customers_RegID ON Customers(RegID);
PRINT 'Index recreated'

-- Step 7: Verify the change
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
PRINT 'Data type: NVARCHAR(10) NULL'
PRINT 'Customers can be created with or without a Registration ID'
PRINT 'Foreign key and index have been recreated properly'

GO

