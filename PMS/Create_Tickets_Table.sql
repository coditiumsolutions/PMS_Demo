-- Create Tickets table for customer care / CRO ticket management
-- Run once on your PMS database. Email is not UNIQUE so multiple tickets per customer are allowed.

IF OBJECT_ID(N'[dbo].[Tickets]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Tickets] (
        [TicketID]       CHAR(10)       NOT NULL,
        [CustomerID]     NVARCHAR(150)  NULL,
        [Email]          NVARCHAR(150)  NULL,
        [Contact]        NVARCHAR(256)  NULL,
        [Title]          NVARCHAR(MAX)  NULL,
        [Description]    NVARCHAR(MAX)  NULL,
        [CROComments]    NVARCHAR(MAX)  NULL,
        [Status]         NVARCHAR(256)  NULL,
        [CreatedBy]      NVARCHAR(256)  NULL,
        [AssignedTo]     NVARCHAR(256)  NULL,
        [TicketClosingDate] DATETIME    NULL,
        [CreatedAt]      DATETIME       NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_Tickets] PRIMARY KEY ([TicketID])
    );
    PRINT 'Table Tickets created.';
END
ELSE
    PRINT 'Table Tickets already exists.';
GO
