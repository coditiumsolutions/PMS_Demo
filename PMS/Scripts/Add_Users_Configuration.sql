-- Add Users category to Configuration: Departments and Designations (comma-separated).
-- Run once. Safe to re-run (IF NOT EXISTS).

IF NOT EXISTS (SELECT 1 FROM Configuration WHERE ConfigKey = 'departments')
BEGIN
    INSERT INTO Configuration (ConfigKey, Category, ConfigValue, Description, CreatedAt, UpdatedAt, UpdatedBy)
    VALUES (
        'departments',
        'Users',
        'Admin,IT,Sales,Accounts,CRO,HR,Operations,Finance,Marketing',
        'User departments (comma-separated)',
        GETDATE(),
        NULL,
        NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM Configuration WHERE ConfigKey = 'designations')
BEGIN
    INSERT INTO Configuration (ConfigKey, Category, ConfigValue, Description, CreatedAt, UpdatedAt, UpdatedBy)
    VALUES (
        'designations',
        'Users',
        'System Administrator,Manager,CRO,Sales Officer,Accountant,HR Officer,Executive',
        'User designations (comma-separated)',
        GETDATE(),
        NULL,
        NULL
    );
END
GO
