-- NDC Section in Configuration table
-- Run once to add NDC-related settings (or add via Settings > Configuration UI).
USE PMS;
GO

-- Insert only if key does not exist (idempotent)
IF NOT EXISTS (SELECT 1 FROM Configuration WHERE ConfigKey = 'NDCWorkFlowStatus')
INSERT INTO Configuration (ConfigKey, Category, ConfigValue, Description, CreatedAt)
VALUES ('NDCWorkFlowStatus', 'NDC', 'Initiated,Approved,Declined', 'NDC workflow statuses (comma-separated)', GETDATE());

IF NOT EXISTS (SELECT 1 FROM Configuration WHERE ConfigKey = 'NDCExpiry')
INSERT INTO Configuration (ConfigKey, Category, ConfigValue, Description, CreatedAt)
VALUES ('NDCExpiry', 'NDC', '14', 'NDC validity in days from creation', GETDATE());

IF NOT EXISTS (SELECT 1 FROM Configuration WHERE ConfigKey = 'NDCStartNormal')
INSERT INTO Configuration (ConfigKey, Category, ConfigValue, Description, CreatedAt)
VALUES ('NDCStartNormal', 'NDC', '3', 'Issued date = creation date + this many days (unless type contains Urgent)', GETDATE());

IF NOT EXISTS (SELECT 1 FROM Configuration WHERE ConfigKey = 'NDCStartUrgent')
INSERT INTO Configuration (ConfigKey, Category, ConfigValue, Description, CreatedAt)
VALUES ('NDCStartUrgent', 'NDC', '0', 'Issued date = creation date + this many days when NDC Type contains Urgent', GETDATE());

IF NOT EXISTS (SELECT 1 FROM Configuration WHERE ConfigKey = 'NDCType')
INSERT INTO Configuration (ConfigKey, Category, ConfigValue, Description, CreatedAt)
VALUES ('NDCType', 'NDC', 'Normal Transfer,Urgent Transfer,Family Transfer,Death Transfer', 'NDC types (comma-separated)', GETDATE());

GO
