-- ============================================
-- SEED CONFIGURATION DATA
-- Populate Configurations table with system values
-- ============================================

USE PMS;
GO

-- Clear existing configurations (optional - comment out if you want to keep existing data)
-- DELETE FROM Configuration;

-- ============================================
-- CITIES (South Sudan)
-- ============================================
Delete from Configuration where Category = 'Cities';
INSERT INTO Configuration (ConfigKey, Category, ConfigValue, Description, CreatedAt)
VALUES
    ('CITY_001', 'Cities', 'Juba', 'Capital city of South Sudan', GETDATE()),
    ('CITY_002', 'Cities', 'Wau', 'City in Western Bahr el Ghazal', GETDATE()),
    ('CITY_003', 'Cities', 'Malakal', 'City in Upper Nile State', GETDATE()),
    ('CITY_004', 'Cities', 'Yei', 'City in Central Equatoria', GETDATE()),
    ('CITY_005', 'Cities', 'Bor', 'City in Jonglei State', GETDATE()),
    ('CITY_006', 'Cities', 'Torit', 'City in Eastern Equatoria', GETDATE()),
    ('CITY_007', 'Cities', 'Yambio', 'City in Western Equatoria', GETDATE()),
    ('CITY_008', 'Cities', 'Rumbek', 'City in Lakes State', GETDATE()),
    ('CITY_009', 'Cities', 'Aweil', 'City in Northern Bahr el Ghazal', GETDATE()),
    ('CITY_010', 'Cities', 'Bentiu', 'City in Unity State', GETDATE());

-- ============================================
-- COUNTRIES
-- ============================================
INSERT INTO Configuration (ConfigKey, Category, ConfigValue, Description, CreatedAt)
VALUES
    ('COUNTRY_001', 'Countries', 'South Sudan', 'Republic of South Sudan', GETDATE()),
    ('COUNTRY_002', 'Countries', 'Sudan', 'Republic of Sudan', GETDATE()),
    ('COUNTRY_003', 'Countries', 'Uganda', 'Republic of Uganda', GETDATE()),
    ('COUNTRY_004', 'Countries', 'Kenya', 'Republic of Kenya', GETDATE()),
    ('COUNTRY_005', 'Countries', 'Ethiopia', 'Federal Democratic Republic of Ethiopia', GETDATE()),
    ('COUNTRY_006', 'Countries', 'Egypt', 'Arab Republic of Egypt', GETDATE());

-- ============================================
-- PROPERTY SIZES
-- ============================================
INSERT INTO Configuration (ConfigKey, Category, ConfigValue, Description, CreatedAt)
VALUES
    ('SIZE_001', 'Sizes', '5 Marla', 'Small residential plot (125 sq yards)', GETDATE()),
    ('SIZE_002', 'Sizes', '7 Marla', 'Medium residential plot (175 sq yards)', GETDATE()),
    ('SIZE_003', 'Sizes', '10 Marla', 'Large residential plot (250 sq yards)', GETDATE()),
    ('SIZE_004', 'Sizes', '1 Kanal', 'Extra large plot (500 sq yards)', GETDATE()),
    ('SIZE_005', 'Sizes', '2 Kanal', 'Double kanal plot (1000 sq yards)', GETDATE()),
    ('SIZE_006', 'Sizes', '250 sq meters', 'Metric measurement plot', GETDATE()),
    ('SIZE_007', 'Sizes', '500 sq meters', 'Half acre metric', GETDATE()),
    ('SIZE_008', 'Sizes', '1000 sq meters', 'One acre metric', GETDATE()),
    ('SIZE_009', 'Sizes', '1500 sq meters', 'Large commercial plot', GETDATE()),
    ('SIZE_010', 'Sizes', '2000 sq meters', 'Extra large commercial', GETDATE());

-- ============================================
-- BLOCKS (Property Block Identifiers)
-- ============================================
INSERT INTO Configuration (ConfigKey, Category, ConfigValue, Description, CreatedAt)
VALUES
    ('BLOCK_001', 'Blocks', 'A', 'Block A', GETDATE()),
    ('BLOCK_002', 'Blocks', 'B', 'Block B', GETDATE()),
    ('BLOCK_003', 'Blocks', 'C', 'Block C', GETDATE()),
    ('BLOCK_004', 'Blocks', 'D', 'Block D', GETDATE()),
    ('BLOCK_005', 'Blocks', 'E', 'Block E', GETDATE()),
    ('BLOCK_006', 'Blocks', 'F', 'Block F', GETDATE()),
    ('BLOCK_007', 'Blocks', 'G', 'Block G', GETDATE()),
    ('BLOCK_008', 'Blocks', 'H', 'Block H', GETDATE());

-- ============================================
-- PROPERTY TYPES
-- ============================================
INSERT INTO Configuration (ConfigKey, Category, ConfigValue, Description, CreatedAt)
VALUES
    ('PTYPE_001', 'PropertyTypes', 'Residential', 'Residential property', GETDATE()),
    ('PTYPE_002', 'PropertyTypes', 'Commercial', 'Commercial property', GETDATE()),
    ('PTYPE_003', 'PropertyTypes', 'Industrial', 'Industrial property', GETDATE()),
    ('PTYPE_004', 'PropertyTypes', 'Mixed-Use', 'Mixed-use property', GETDATE()),
    ('PTYPE_005', 'PropertyTypes', 'Agricultural', 'Agricultural land', GETDATE());

-- ============================================
-- PROJECT TYPES
-- ============================================
INSERT INTO Configuration (ConfigKey, Category, ConfigValue, Description, CreatedAt)
VALUES
    ('PRTYPE_001', 'ProjectTypes', 'Housing', 'Housing project', GETDATE()),
    ('PRTYPE_002', 'ProjectTypes', 'Commercial Complex', 'Commercial complex', GETDATE()),
    ('PRTYPE_003', 'ProjectTypes', 'Mixed-Use Development', 'Mixed-use development', GETDATE()),
    ('PRTYPE_004', 'ProjectTypes', 'Gated Community', 'Gated community', GETDATE()),
    ('PRTYPE_005', 'ProjectTypes', 'Smart City', 'Smart city development', GETDATE());

-- ============================================
-- PAYMENT METHODS
-- ============================================
INSERT INTO Configuration (ConfigKey, Category, ConfigValue, Description, CreatedAt)
VALUES
    ('PMETHOD_001', 'PaymentMethods', 'Cash', 'Cash payment', GETDATE()),
    ('PMETHOD_002', 'PaymentMethods', 'Bank Transfer', 'Bank transfer payment', GETDATE()),
    ('PMETHOD_003', 'PaymentMethods', 'Cheque', 'Cheque payment', GETDATE()),
    ('PMETHOD_004', 'PaymentMethods', 'Online Payment', 'Online payment gateway', GETDATE()),
    ('PMETHOD_005', 'PaymentMethods', 'Mobile Money', 'Mobile money transfer', GETDATE());

-- ============================================
-- YEARS (For Date Selections)
-- ============================================
DECLARE @CurrentYear INT = YEAR(GETDATE());
DECLARE @Year INT = @CurrentYear - 5;
DECLARE @Counter INT = 1;

WHILE @Year <= @CurrentYear + 10
BEGIN
    INSERT INTO Configuration (ConfigKey, Category, ConfigValue, Description, CreatedAt)
    VALUES (
        'YEAR_' + RIGHT('000' + CAST(@Counter AS VARCHAR(3)), 3),
        'Years',
        CAST(@Year AS VARCHAR(4)),
        'Year ' + CAST(@Year AS VARCHAR(4)),
        GETDATE()
    );
    
    SET @Year = @Year + 1;
    SET @Counter = @Counter + 1;
END;

-- ============================================
-- STATUS OPTIONS
-- ============================================
INSERT INTO Configuration (ConfigKey, Category, ConfigValue, Description, CreatedAt)
VALUES
    ('STATUS_001', 'Status', 'Active', 'Active status', GETDATE()),
    ('STATUS_002', 'Status', 'Inactive', 'Inactive status', GETDATE()),
    ('STATUS_003', 'Status', 'Pending', 'Pending status', GETDATE()),
    ('STATUS_004', 'Status', 'Approved', 'Approved status', GETDATE()),
    ('STATUS_005', 'Status', 'Rejected', 'Rejected status', GETDATE()),
    ('STATUS_006', 'Status', 'Suspended', 'Suspended status', GETDATE()),
    ('STATUS_007', 'Status', 'Cancelled', 'Cancelled status', GETDATE()),
    ('STATUS_008', 'Status', 'Available', 'Available status (for properties)', GETDATE()),
    ('STATUS_009', 'Status', 'Allotted', 'Allotted status (for properties)', GETDATE()),
    ('STATUS_010', 'Status', 'Completed', 'Completed status', GETDATE());

GO

-- ============================================
-- VERIFY DATA
-- ============================================
PRINT '=== CONFIGURATION DATA SEEDED SUCCESSFULLY ==='
SELECT Category, COUNT(*) as Count
FROM Configuration
GROUP BY Category
ORDER BY Category;

PRINT ''
PRINT 'Total Records: ' + CAST((SELECT COUNT(*) FROM Configuration) AS VARCHAR);
GO

