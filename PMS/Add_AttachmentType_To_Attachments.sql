-- Add missing AttachmentType column to Attachments table
-- Run this on the PMS database if you get "Invalid column name AttachmentType" on image upload

USE PMS;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Attachments')
    AND name = 'AttachmentType'
)
BEGIN
    ALTER TABLE dbo.Attachments
    ADD AttachmentType NVARCHAR(50) NULL; -- 'CustomerPicture', 'IDCard', 'Other'

    PRINT 'Column AttachmentType added to Attachments.';
END
ELSE
BEGIN
    PRINT 'Column AttachmentType already exists.';
END
GO
