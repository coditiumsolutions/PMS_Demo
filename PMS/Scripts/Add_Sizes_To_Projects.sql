-- Add Sizes column to Projects (CSV storage). Run if EF migration was already applied as empty.
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Projects') AND name = 'Sizes')
BEGIN
    ALTER TABLE Projects ADD Sizes NVARCHAR(1000) NULL;
END
GO
