-- ===========================================================
-- Property Inquiry Table (Website Sales Lead Form)
-- ===========================================================
-- Standalone table for capturing website inquiry submissions
-- Not connected to other tables - for sales/marketing leads
-- ===========================================================

USE PMS;
GO

-- Check if table already exists
IF OBJECT_ID('PropertyInquiry', 'U') IS NOT NULL
BEGIN
    PRINT 'PropertyInquiry table already exists. Skipping creation.';
END
ELSE
BEGIN
    CREATE TABLE PropertyInquiry (
        InquiryID INT IDENTITY(1,1) PRIMARY KEY,
        FullName NVARCHAR(150) NOT NULL,
        PhoneNumber NVARCHAR(50) NOT NULL,
        EmailAddress NVARCHAR(150),
        InquiryType NVARCHAR(100),
        Message NVARCHAR(MAX),
        SubmittedAt DATETIME DEFAULT GETDATE(),
        IPAddress NVARCHAR(50),
        Status NVARCHAR(50) DEFAULT 'New',
        AssignedTo NVARCHAR(100),
        FollowUpDate DATETIME NULL,
        Notes NVARCHAR(MAX),
        IsContacted BIT DEFAULT 0,
        CreatedAt DATETIME DEFAULT GETDATE()
    );
    
    PRINT 'PropertyInquiry table created successfully.';
END
GO

-- Create index for faster queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PropertyInquiry_Status' AND object_id = OBJECT_ID('PropertyInquiry'))
BEGIN
    CREATE INDEX IX_PropertyInquiry_Status ON PropertyInquiry(Status);
    PRINT 'Index on Status created.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PropertyInquiry_SubmittedAt' AND object_id = OBJECT_ID('PropertyInquiry'))
BEGIN
    CREATE INDEX IX_PropertyInquiry_SubmittedAt ON PropertyInquiry(SubmittedAt DESC);
    PRINT 'Index on SubmittedAt created.';
END
GO

-- Sample data for testing (optional - comment out if not needed)
/*
INSERT INTO PropertyInquiry (FullName, PhoneNumber, EmailAddress, InquiryType, Message, IPAddress, Status)
VALUES 
    ('John Doe', '+211-912-345-678', 'john.doe@example.com', 'Residential Plot', 'I am interested in a 10 Marla residential plot in Block A', '192.168.1.100', 'New'),
    ('Sarah Johnson', '+211-923-456-789', 'sarah.j@example.com', 'Commercial Property', 'Looking for commercial space for my business', '192.168.1.101', 'New'),
    ('Michael Brown', '+211-934-567-890', 'michael.b@example.com', 'Payment Plan Inquiry', 'What are the available payment plans for residential plots?', '192.168.1.102', 'Contacted'),
    ('Emma Wilson', '+211-945-678-901', 'emma.w@example.com', 'General Inquiry', 'I want to visit the site. When can I schedule a visit?', '192.168.1.103', 'New'),
    ('David Lee', '+211-956-789-012', 'david.lee@example.com', 'Residential Plot', 'Interested in 5 Marla plot near main road', '192.168.1.104', 'Follow-up');

PRINT 'Sample inquiry data inserted.';
*/

GO

PRINT 'PropertyInquiry table setup completed successfully!';
GO

-- ===========================================================
-- Table Structure Reference
-- ===========================================================
/*
Columns:
- InquiryID: Auto-incrementing primary key
- FullName: Customer's full name (required)
- PhoneNumber: Contact number (required)
- EmailAddress: Email address (optional)
- InquiryType: Type of inquiry (e.g., Residential Plot, Commercial, Payment Plan)
- Message: Inquiry message/details
- SubmittedAt: Timestamp when form was submitted
- IPAddress: IP address of the visitor
- Status: New, Contacted, Follow-up, Converted, Closed
- AssignedTo: Sales agent assigned to follow up
- FollowUpDate: Scheduled follow-up date
- Notes: Internal notes about the inquiry
- IsContacted: Flag indicating if customer was contacted
- CreatedAt: Record creation timestamp

Usage:
- Website visitors submit property inquiry form
- Sales team manages leads in admin panel
- Track inquiry status and follow-ups
- Convert leads to registered customers
*/

