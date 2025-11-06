-- Add ProjectID column to Customers table
-- This allows customers to be directly associated with a project
-- The ProjectID will be used to determine the customer prefix for ID generation

USE PMS;
GO

-- Check if column already exists, drop it if it does
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Customers') AND name = 'ProjectID')
BEGIN
    -- Drop foreign key constraint if it exists
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Customers_Projects')
    BEGIN
        ALTER TABLE Customers DROP CONSTRAINT FK_Customers_Projects;
    END
    -- Drop the column
    ALTER TABLE Customers DROP COLUMN ProjectID;
END
GO

-- Get the exact column definition from Projects.ProjectID using INFORMATION_SCHEMA
DECLARE @FullColumnDefinition NVARCHAR(MAX);
DECLARE @DataType NVARCHAR(50);
DECLARE @CharacterMaximumLength INT;
DECLARE @Collation NVARCHAR(50);

SELECT 
    @DataType = DATA_TYPE,
    @CharacterMaximumLength = CHARACTER_MAXIMUM_LENGTH,
    @Collation = COLLATION_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo' 
  AND TABLE_NAME = 'Projects' 
  AND COLUMN_NAME = 'ProjectID';

-- Build the complete data type definition
IF @CharacterMaximumLength IS NOT NULL
BEGIN
    SET @FullColumnDefinition = @DataType + '(' + CAST(@CharacterMaximumLength AS VARCHAR) + ')';
END
ELSE
BEGIN
    SET @FullColumnDefinition = @DataType;
END

-- Add collation if it exists
IF @Collation IS NOT NULL
BEGIN
    SET @FullColumnDefinition = @FullColumnDefinition + ' COLLATE ' + @Collation;
END

-- Add ProjectID column with the exact same definition as Projects.ProjectID
DECLARE @Sql NVARCHAR(MAX);
SET @Sql = N'ALTER TABLE Customers ADD ProjectID ' + @FullColumnDefinition + ' NULL;';

PRINT 'Column Definition: ' + @FullColumnDefinition; -- Debug output
PRINT 'Executing: ' + @Sql; -- Debug output

EXEC sp_executesql @Sql;
GO

-- Add foreign key constraint
ALTER TABLE Customers
ADD CONSTRAINT FK_Customers_Projects 
FOREIGN KEY (ProjectID) REFERENCES Projects(ProjectID);
GO

-- Optional: Update existing customers to set ProjectID based on their PaymentPlan's Project
-- This is a one-time migration for existing data
UPDATE c
SET c.ProjectID = pp.ProjectID
FROM Customers c
INNER JOIN PaymentPlan pp ON c.PlanID = pp.PlanID
WHERE c.ProjectID IS NULL AND pp.ProjectID IS NOT NULL;
GO

-- After verifying the data, you can make ProjectID NOT NULL if needed:
-- ALTER TABLE Customers
-- ALTER COLUMN ProjectID CHAR(10) NOT NULL;
-- GO

