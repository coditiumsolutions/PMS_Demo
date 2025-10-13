-- ============================================
-- ADD CONSTRAINTS: Enforce 1 Property = 1 Customer Rule
-- RUN THIS AFTER CleanupDuplicateAllotments.sql
-- ============================================

USE PMS;
GO

-- Step 1: Add UNIQUE constraint on PropertyID in Allotment table
-- This ensures one property can only have ONE allotment record
PRINT '=== ADDING UNIQUE CONSTRAINT ON PROPERTYID ==='

-- First, check if constraint already exists
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Allotment_PropertyID' AND object_id = OBJECT_ID('Allotment'))
BEGIN
    PRINT 'Constraint UQ_Allotment_PropertyID already exists. Dropping it first...';
    ALTER TABLE Allotment DROP CONSTRAINT UQ_Allotment_PropertyID;
END

-- Add the unique constraint
ALTER TABLE Allotment 
ADD CONSTRAINT UQ_Allotment_PropertyID UNIQUE (PropertyID);

PRINT 'SUCCESS: Unique constraint added on PropertyID';
PRINT 'Business Rule Enforced: 1 Property = 1 Allotment (1 Customer)';

-- Step 2: Verify the constraint
PRINT '=== VERIFYING CONSTRAINT ==='
SELECT 
    i.name as ConstraintName,
    c.name as ColumnName,
    t.name as TableName
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE i.name = 'UQ_Allotment_PropertyID';

PRINT '=== CONSTRAINT SUCCESSFULLY APPLIED ==='
PRINT 'From now on:';
PRINT '- Each property can have only ONE allotment';
PRINT '- Attempting to create duplicate allotments will result in an error';
PRINT '- If a customer wants another plot, create a NEW customer entry with new RegistrationID';

GO

