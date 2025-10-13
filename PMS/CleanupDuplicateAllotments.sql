-- ============================================
-- CLEANUP SCRIPT: Remove Duplicate Allotments
-- Business Rule: 1 Property = 1 Customer (or 0)
-- ============================================

USE PMS;
GO

-- Step 1: Check for properties with multiple allotments
PRINT '=== CHECKING FOR PROPERTIES WITH MULTIPLE ALLOTMENTS ==='
SELECT 
    PropertyID, 
    COUNT(*) as AllotmentCount
FROM Allotment
GROUP BY PropertyID
HAVING COUNT(*) > 1
ORDER BY COUNT(*) DESC;

-- Step 2: Check for customers with multiple properties
PRINT '=== CHECKING FOR CUSTOMERS WITH MULTIPLE PROPERTIES ==='
SELECT 
    CustomerID, 
    COUNT(DISTINCT PropertyID) as PropertyCount
FROM Allotment
GROUP BY CustomerID
HAVING COUNT(DISTINCT PropertyID) > 1
ORDER BY COUNT(DISTINCT PropertyID) DESC;

-- Step 3: View all duplicate allotments with details
PRINT '=== DETAILED VIEW OF DUPLICATE ALLOTMENTS ==='
SELECT 
    a.AllotmentID,
    a.PropertyID,
    a.CustomerID,
    c.FullName as CustomerName,
    a.AllotmentDate,
    a.AllottmentType,
    a.WorkFlowStatus,
    p.PlotNo,
    p.Block
FROM Allotment a
INNER JOIN Customers c ON a.CustomerID = c.CustomerID
INNER JOIN Property p ON a.PropertyID = p.PropertyID
WHERE a.PropertyID IN (
    SELECT PropertyID 
    FROM Allotment 
    GROUP BY PropertyID 
    HAVING COUNT(*) > 1
)
ORDER BY a.PropertyID, a.AllotmentDate;

-- Step 4: BACKUP THE ALLOTMENT TABLE (IMPORTANT!)
PRINT '=== CREATING BACKUP TABLE ==='
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'Allotment_Backup') AND type in (N'U'))
    DROP TABLE Allotment_Backup;

SELECT * INTO Allotment_Backup FROM Allotment;
PRINT 'Backup created: Allotment_Backup table';

-- Step 5: DELETE DUPLICATES (Keep only the LATEST allotment per property)
-- This keeps the most recent allotment based on AllotmentDate
PRINT '=== DELETING DUPLICATE ALLOTMENTS (Keeping Latest Only) ==='

-- Delete all but the latest allotment for each property
DELETE FROM Allotment
WHERE AllotmentID NOT IN (
    SELECT MAX(AllotmentID)
    FROM Allotment
    GROUP BY PropertyID
);

PRINT 'Duplicate allotments deleted!';

-- Step 6: Verify cleanup
PRINT '=== VERIFICATION: Checking for remaining duplicates ==='
SELECT 
    PropertyID, 
    COUNT(*) as AllotmentCount
FROM Allotment
GROUP BY PropertyID
HAVING COUNT(*) > 1;

PRINT 'If no results above, cleanup successful!';

-- Step 7: Show summary
PRINT '=== CLEANUP SUMMARY ==='
SELECT 
    (SELECT COUNT(*) FROM Allotment_Backup) as TotalBeforeCleanup,
    (SELECT COUNT(*) FROM Allotment) as TotalAfterCleanup,
    (SELECT COUNT(*) FROM Allotment_Backup) - (SELECT COUNT(*) FROM Allotment) as RecordsDeleted;

GO

