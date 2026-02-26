-- Replace old Transfer table with new schema (Customer file / ownership transfer)
-- Workflow: Created > At the Desk of Accounts > At the Desk of Transfer Approval > Approved

USE PMS;
GO

-- Drop FK from Property to Transfer (old schema)
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Transfer_Property_PropertyID')
    ALTER TABLE [dbo].[Transfer] DROP CONSTRAINT [FK_Transfer_Property_PropertyID];
GO

-- Drop FKs from Transfer to Customers (old schema)
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Transfer_Customers_FromCustomerID')
    ALTER TABLE [dbo].[Transfer] DROP CONSTRAINT [FK_Transfer_Customers_FromCustomerID];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Transfer_Customers_ToCustomerID')
    ALTER TABLE [dbo].[Transfer] DROP CONSTRAINT [FK_Transfer_Customers_ToCustomerID];
GO

-- Drop old Transfer table
IF OBJECT_ID(N'dbo.Transfer', N'U') IS NOT NULL
    DROP TABLE [dbo].[Transfer];
GO

-- Create new Transfer table
CREATE TABLE [dbo].[Transfer] (
    TransferID       NVARCHAR(50) PRIMARY KEY,
    CustomerID       NVARCHAR(10) NOT NULL,
    WorkFlowStatus   NVARCHAR(100),
    CreatedAt        DATETIME DEFAULT GETDATE(),

    SellerName       NVARCHAR(200),
    SellerFatherName NVARCHAR(200),
    SellerCNIC       NVARCHAR(200),
    SellerContact    NVARCHAR(200),
    SellerAddress    NVARCHAR(200),

    BuyerName        NVARCHAR(200),
    BuyerFatherName  NVARCHAR(200),
    BuyerCNIC        NVARCHAR(200),
    BuyerContact     NVARCHAR(200),
    BuyerAddress     NVARCHAR(200),
    BuyerCity        NVARCHAR(200),
    BuyerCountry     NVARCHAR(200),
    BuyerAttachments NVARCHAR(MAX),  -- JSON
    SellerAttachments NVARCHAR(MAX), -- JSON

    TransferFeeDue   FLOAT,
    TransferFeePaid  FLOAT,
    PaymentDate      DATE,
    PaymentMode      NVARCHAR(200),
    PaymentChallanNo NVARCHAR(200),

    Details          NVARCHAR(MAX),
    CROComments      NVARCHAR(MAX),
    AccountsComments NVARCHAR(MAX),
    TransferComments NVARCHAR(MAX),

    CONSTRAINT FK_Transfer_Customers_CustomerID
        FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID) ON DELETE NO ACTION
);
GO

CREATE INDEX IX_Transfer_CustomerID ON [dbo].[Transfer](CustomerID);
CREATE INDEX IX_Transfer_WorkFlowStatus ON [dbo].[Transfer](WorkFlowStatus);
GO
