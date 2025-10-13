-- ============================================
-- ADD SUB PROJECTS CONFIGURATION
-- Quick script to add SubProjects to existing configurations
-- ============================================

USE PMS;
GO

-- Check if subprojects already exists
IF EXISTS (SELECT * FROM Configuration WHERE ConfigKey = 'subprojects')
BEGIN
    PRINT 'SubProjects configuration already exists. Updating...'
    
    UPDATE Configuration
    SET ConfigValue = 'Phase 1,Phase 2,Phase 3,Block A Extension,Block B Extension,Commercial Zone,Residential Zone',
        Description = 'Sub-project names within main projects',
        UpdatedAt = GETDATE()
    WHERE ConfigKey = 'subprojects';
    
    PRINT 'SubProjects configuration updated!'
END
ELSE
BEGIN
    PRINT 'Adding SubProjects configuration...'
    
    INSERT INTO Configuration (ConfigKey, Category, ConfigValue, Description, CreatedAt)
    VALUES ('subprojects', 'SubProjects', 'Phase 1,Phase 2,Phase 3,Block A Extension,Block B Extension,Commercial Zone,Residential Zone', 
            'Sub-project names within main projects', GETDATE());
    
    PRINT 'SubProjects configuration added!'
END

GO

-- Verify
PRINT ''
PRINT '=== VERIFICATION ==='
SELECT 
    ConfigKey,
    Category,
    ConfigValue,
    Description
FROM Configuration
WHERE ConfigKey = 'subprojects';

PRINT ''
PRINT '✅ SubProjects configuration is ready!'
PRINT 'Values: Phase 1, Phase 2, Phase 3, Block A Extension, Block B Extension, Commercial Zone, Residential Zone'

GO

