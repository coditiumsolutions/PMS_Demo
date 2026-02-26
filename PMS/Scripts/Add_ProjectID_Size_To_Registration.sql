-- Add ProjectID and Size to Registration table. Run if EF migration not used.
-- Use CHAR(10) for ProjectID to match Projects.ProjectID in db.txt.
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Registration') AND name = 'ProjectID')
BEGIN
    ALTER TABLE Registration ADD ProjectID CHAR(10) NULL;
    IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Projects')
        ALTER TABLE Registration ADD CONSTRAINT FK_Registration_Project FOREIGN KEY (ProjectID) REFERENCES Projects(ProjectID) ON DELETE SET NULL;
END
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Registration') AND name = 'Size')
BEGIN
    ALTER TABLE Registration ADD Size NVARCHAR(100) NULL;
END
GO
