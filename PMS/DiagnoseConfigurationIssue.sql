-- ============================================
-- DIAGNOSE CONFIGURATION ISSUE
-- Check what's currently in the Configuration table
-- ============================================

USE PMS;
GO

PRINT '=== CHECKING CONFIGURATION TABLE ==='
PRINT ''

-- Check if Configuration table exists
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Configuration')
BEGIN
    PRINT 'Configuration table EXISTS'
    PRINT ''
    
    -- Check total count
    DECLARE @TotalCount INT = (SELECT COUNT(*) FROM Configuration);
    PRINT 'Total rows in Configuration table: ' + CAST(@TotalCount AS VARCHAR);
    PRINT ''
    
    -- Show all configurations
    PRINT '=== ALL CONFIGURATION ROWS ==='
    SELECT 
        ConfigKey,
        Category,
        LEFT(ConfigValue, 100) as ConfigValue_Preview,
        Description
    FROM Configuration
    ORDER BY Category, ConfigKey;
    
    PRINT ''
    PRINT '=== COUNT BY CATEGORY ==='
    SELECT 
        Category,
        COUNT(*) as RowCount
    FROM Configuration
    GROUP BY Category
    ORDER BY Category;
    
    PRINT ''
    PRINT '=== CHECKING FOR CORRECT PATTERN (lowercase keys) ==='
    SELECT 
        ConfigKey,
        Category,
        LEFT(ConfigValue, 60) as ConfigValue_Preview
    FROM Configuration
    WHERE ConfigKey IN ('cities', 'countries', 'sizes', 'years', 'blocks');
    
    PRINT ''
    PRINT '=== CHECKING FOR INCORRECT PATTERN (uppercase keys) ==='
    SELECT 
        ConfigKey,
        Category,
        ConfigValue
    FROM Configuration
    WHERE ConfigKey LIKE 'CITY_%' 
       OR ConfigKey LIKE 'COUNTRY_%' 
       OR ConfigKey LIKE 'SIZE_%';
    
    IF @TotalCount = 0
    BEGIN
        PRINT ''
        PRINT '⚠️ WARNING: Configuration table is EMPTY!'
        PRINT 'ACTION REQUIRED: Run SeedConfigurationData_CORRECT.sql'
    END
    ELSE IF NOT EXISTS (SELECT * FROM Configuration WHERE ConfigKey = 'cities')
    BEGIN
        PRINT ''
        PRINT '⚠️ WARNING: No configuration found with ConfigKey = ''cities'''
        PRINT 'ACTION REQUIRED:'
        PRINT '1. Run CleanupIncorrectConfigurations.sql (to remove wrong format)'
        PRINT '2. Run SeedConfigurationData_CORRECT.sql (to seed correct format)'
    END
    ELSE
    BEGIN
        PRINT ''
        PRINT '✅ Configuration table looks good!'
        PRINT 'Data is in correct format (key-value pattern)'
    END
END
ELSE
BEGIN
    PRINT '❌ ERROR: Configuration table does NOT exist!'
    PRINT 'ACTION REQUIRED: Check database schema'
END

GO

