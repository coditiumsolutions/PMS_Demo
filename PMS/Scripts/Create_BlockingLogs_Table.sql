-- Create BlockingLogs table for customer blocking/unblocking with reason and optional attachment.
-- Also log each action to ActivityLog (done by application). Idempotent: run safe if table exists.
USE PMSAbbas;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BlockingLogs')
BEGIN
    CREATE TABLE BlockingLogs (
        BlockingLogID INT IDENTITY(1,1) PRIMARY KEY,
        CustomerID CHAR(10) NOT NULL,
        UserID CHAR(10) NOT NULL,
        ActionDate DATETIME DEFAULT GETDATE(),
        PreviousStatus NVARCHAR(50) NULL,
        NewStatus NVARCHAR(50) NULL,
        Reason NVARCHAR(MAX) NOT NULL,
        AttachmentPath NVARCHAR(500) NULL
    );
    ALTER TABLE BlockingLogs ADD CONSTRAINT FK_BlockingLogs_Customer FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID);
    ALTER TABLE BlockingLogs ADD CONSTRAINT FK_BlockingLogs_User FOREIGN KEY (UserID) REFERENCES Users(UserID);
    PRINT 'BlockingLogs table created.';
END
ELSE
    PRINT 'BlockingLogs table already exists.';
GO
