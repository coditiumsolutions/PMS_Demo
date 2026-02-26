-- Update Currency from SSP to PKR in PaymentPlan table
-- Run this script to update all existing payment plans

USE PMS;
GO

-- Update all PaymentPlan records that have Currency = 'SSP' to 'PKR'
UPDATE PaymentPlan
SET Currency = 'PKR'
WHERE Currency = 'SSP' OR Currency IS NULL OR Currency = '';

-- Verify the update
SELECT PlanID, PlanName, Currency, TotalAmount
FROM PaymentPlan
ORDER BY PlanID;

-- Show count of updated records
SELECT COUNT(*) AS UpdatedRecords
FROM PaymentPlan
WHERE Currency = 'PKR';

GO
