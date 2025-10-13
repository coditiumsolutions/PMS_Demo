-- ===========================================================
-- Add CustomerID to Payments Table
-- ===========================================================
-- Run this ONLY if the column doesn't exist yet
-- Check first if column exists to avoid duplicate column error
-- ===========================================================

USE PMS;
GO

-- Check if column exists, if not add it
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID('Payments') 
               AND name = 'CustomerID')
BEGIN
    ALTER TABLE Payments
    ADD CustomerID NVARCHAR(10) NULL;
    
    PRINT 'CustomerID column added successfully';
END
ELSE
BEGIN
    PRINT 'CustomerID column already exists';
END
GO

-- Add foreign key constraint if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.foreign_keys 
               WHERE name = 'FK_Payments_Customer' 
               AND parent_object_id = OBJECT_ID('Payments'))
BEGIN
    ALTER TABLE Payments
    ADD CONSTRAINT FK_Payments_Customer 
    FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID);
    
    PRINT 'Foreign key constraint added successfully';
END
ELSE
BEGIN
    PRINT 'Foreign key constraint already exists';
END
GO

-- Update existing payments with CustomerID based on PaymentSchedule -> PaymentPlan -> Customer
UPDATE p
SET p.CustomerID = c.CustomerID
FROM Payments p
INNER JOIN PaymentSchedule ps ON p.ScheduleID = ps.ScheduleID
INNER JOIN PaymentPlan pp ON ps.PlanID = pp.PlanID
INNER JOIN Customers c ON c.PlanID = pp.PlanID
WHERE p.CustomerID IS NULL;
GO

PRINT 'Existing payments updated with CustomerID';
GO

