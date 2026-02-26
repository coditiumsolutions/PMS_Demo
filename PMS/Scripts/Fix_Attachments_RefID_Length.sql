-- Ensure Attachments.RefID can store Transfer IDs (e.g. TRF-20260217-0003 = 18 chars)
-- and other longer reference IDs. Run this if upload fails with "String or binary data would be truncated".
USE PMS;
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Attachments') AND name = N'RefID')
BEGIN
    ALTER TABLE dbo.Attachments
    ALTER COLUMN RefID NVARCHAR(50) NULL;
    PRINT 'Attachments.RefID updated to NVARCHAR(50).';
END
ELSE
    PRINT 'Attachments.RefID not found - no change.';
GO
