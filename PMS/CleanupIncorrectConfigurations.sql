-- ============================================
-- CLEANUP INCORRECT CONFIGURATION DATA
-- Removes the incorrectly seeded configuration rows
-- ============================================

USE PMS;
GO

PRINT '=== CLEANING UP INCORRECT CONFIGURATION DATA ==='

-- Delete all the incorrectly seeded configurations
DELETE FROM Configuration 
WHERE ConfigKey IN (
    -- Cities
    'CITY_001', 'CITY_002', 'CITY_003', 'CITY_004', 'CITY_005',
    'CITY_006', 'CITY_007', 'CITY_008', 'CITY_009', 'CITY_010',
    
    -- Countries
    'COUNTRY_001', 'COUNTRY_002', 'COUNTRY_003', 'COUNTRY_004', 'COUNTRY_005', 'COUNTRY_006',
    
    -- Sizes
    'SIZE_001', 'SIZE_002', 'SIZE_003', 'SIZE_004', 'SIZE_005',
    'SIZE_006', 'SIZE_007', 'SIZE_008', 'SIZE_009', 'SIZE_010',
    
    -- Blocks
    'BLOCK_001', 'BLOCK_002', 'BLOCK_003', 'BLOCK_004',
    'BLOCK_005', 'BLOCK_006', 'BLOCK_007', 'BLOCK_008',
    
    -- Property Types
    'PTYPE_001', 'PTYPE_002', 'PTYPE_003', 'PTYPE_004', 'PTYPE_005',
    
    -- Project Types
    'PRTYPE_001', 'PRTYPE_002', 'PRTYPE_003', 'PRTYPE_004', 'PRTYPE_005',
    
    -- Payment Methods
    'PMETHOD_001', 'PMETHOD_002', 'PMETHOD_003', 'PMETHOD_004', 'PMETHOD_005',
    
    -- Status
    'STATUS_001', 'STATUS_002', 'STATUS_003', 'STATUS_004', 'STATUS_005',
    'STATUS_006', 'STATUS_007', 'STATUS_008', 'STATUS_009', 'STATUS_010'
);

-- Delete incorrectly seeded years
DELETE FROM Configuration 
WHERE Category = 'Years' AND ConfigKey LIKE 'YEAR_%';

PRINT 'Cleanup completed!';
PRINT '';

-- Show remaining configurations
PRINT '=== REMAINING CONFIGURATIONS ==='
SELECT ConfigKey, Category, LEFT(ConfigValue, 50) as ConfigValue_Preview, Description
FROM Configuration
ORDER BY Category, ConfigKey;

PRINT '';
PRINT 'Total Remaining Records: ' + CAST((SELECT COUNT(*) FROM Configuration) AS VARCHAR);

GO

