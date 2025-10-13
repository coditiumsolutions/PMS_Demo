-- ============================================
-- FIX CONFIGURATIONS - ALL IN ONE SCRIPT
-- Cleanup + Seed in correct format
-- ============================================

USE PMS;
GO

PRINT '=== STEP 1: CLEANING UP ALL EXISTING CONFIGURATIONS ==='

-- Delete ALL existing configurations to start fresh
DELETE FROM Configuration;

PRINT 'All old configurations deleted!'
PRINT ''

-- ============================================
-- STEP 2: SEED CORRECT CONFIGURATION DATA
-- ============================================

PRINT '=== STEP 2: SEEDING CONFIGURATION DATA (CORRECT PATTERN) ==='
PRINT ''

INSERT INTO Configuration (ConfigKey, Category, ConfigValue, Description, CreatedAt)
VALUES
    -- Years (2020-2035)
    ('years', 'Years', '2020,2021,2022,2023,2024,2025,2026,2027,2028,2029,2030,2031,2032,2033,2034,2035', 
     'Available years for date selections', GETDATE()),
    
    -- Cities (South Sudan)
    ('cities', 'Cities', 'Juba,Wau,Malakal,Yei,Bor,Torit,Yambio,Rumbek,Aweil,Bentiu', 
     'South Sudan cities', GETDATE()),
    
    -- Countries
    ('countries', 'Countries', 'South Sudan,Sudan,Uganda,Kenya,Ethiopia,Egypt', 
     'Countries list', GETDATE()),
    
    -- Property Sizes
    ('sizes', 'Sizes', '5 Marla,7 Marla,10 Marla,1 Kanal,2 Kanal,250 sq meters,500 sq meters,1000 sq meters,1500 sq meters,2000 sq meters', 
     'Property sizes available', GETDATE()),
    
    -- Blocks
    ('blocks', 'Blocks', 'A,B,C,D,E,F,G,H,I,J,K,L', 
     'Property block identifiers', GETDATE()),
    
    -- Property Types
    ('propertytypes', 'PropertyTypes', 'Residential,Commercial,Industrial,Mixed-Use,Agricultural', 
     'Types of properties', GETDATE()),
    
    -- Project Types
    ('projecttypes', 'ProjectTypes', 'Housing,Commercial Complex,Mixed-Use Development,Gated Community,Smart City', 
     'Types of projects', GETDATE()),
    
    -- Payment Methods
    ('paymentmethods', 'PaymentMethods', 'Cash,Bank Transfer,Cheque,Online Payment,Mobile Money', 
     'Available payment methods', GETDATE()),
    
    -- Plot Types
    ('plottypes', 'PlotTypes', 'Corner,Park Facing,Main Road,Inner Plot,Commercial Boulevard', 
     'Types of plot locations', GETDATE()),
    
    -- Customer Status
    ('customerstatus', 'CustomerStatus', 'Active,Inactive,Suspended,Cancelled', 
     'Customer status options', GETDATE()),
    
    -- Property Status
    ('propertystatus', 'PropertyStatus', 'Available,Allotted,Reserved,Blocked', 
     'Property status options', GETDATE()),
    
    -- Workflow Status
    ('workflowstatus', 'WorkflowStatus', 'Pending,Pending Approval,Approved,Rejected,Completed,Cancelled', 
     'Workflow status options', GETDATE()),
    
    -- Payment Status
    ('paymentstatus', 'PaymentStatus', 'Pending,Completed,Failed,Refunded,Cancelled', 
     'Payment status options', GETDATE()),
    
    -- Nominee Relations
    ('nomineerelations', 'NomineeRelations', 'Father,Mother,Son,Daughter,Spouse,Brother,Sister,Other', 
     'Nominee relationship options', GETDATE()),
    
    -- Allotment Types
    ('allotmenttypes', 'AllotmentTypes', 'Regular,Transfer,Balloting,Special', 
     'Types of allotments', GETDATE()),
    
    -- Sub Projects
    ('subprojects', 'SubProjects', 'Phase 1,Phase 2,Phase 3,Block A Extension,Block B Extension,Commercial Zone,Residential Zone', 
     'Sub-project names within main projects', GETDATE());

PRINT '✅ Configuration data seeded successfully!'
PRINT ''

-- ============================================
-- STEP 3: VERIFY DATA
-- ============================================

PRINT '=== VERIFICATION ==='
PRINT ''

SELECT 
    ConfigKey,
    Category,
    LEFT(ConfigValue, 80) as ConfigValue_Preview,
    Description
FROM Configuration
ORDER BY ConfigKey;

PRINT ''
PRINT '=== SUMMARY ==='
SELECT 
    'Total Configurations' as Info,
    COUNT(*) as Count
FROM Configuration;

PRINT ''
PRINT '=== TESTING SPLIT EXAMPLE ==='
PRINT 'Cities ConfigValue:'
SELECT ConfigValue FROM Configuration WHERE ConfigKey = 'cities';

PRINT ''
PRINT '✅ DONE! Configuration table is now ready.'
PRINT ''
PRINT 'EXPECTED ROWS: 15'
PRINT 'Pattern: ConfigKey = lowercase (cities, countries, sizes)'
PRINT 'ConfigValue = Comma-separated list'
PRINT ''
PRINT 'Test in application:'
PRINT '1. Navigate to /Customer/Create'
PRINT '2. Check City, Country, Registered Size dropdowns'
PRINT '3. Should see all options'

GO

