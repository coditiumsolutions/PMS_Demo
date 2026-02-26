-- Add PaymentStatus to Configuration table (used by Payment/RecordPayment and Payment/EditPayment dropdowns).
-- Format matches existing entries (e.g. allotmenttypes): ConfigValue is comma-separated list.
-- Run once on your database.

IF NOT EXISTS (SELECT 1 FROM Configuration WHERE ConfigKey = 'paymentstatus')
BEGIN
    INSERT INTO Configuration (ConfigKey, Category, ConfigValue, Description, CreatedAt, UpdatedAt, UpdatedBy)
    VALUES (
        'paymentstatus',
        'Payment',
        'Completed,Pending,Processing,Approved',
        'Payment status options for Record Payment and Edit Payment',
        GETDATE(),
        NULL,
        NULL
    );
END
GO
