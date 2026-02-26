-- Add USD to PKR exchange rate to Configuration table (used by Payment/CreatePaymentPlan and other currency conversion).
-- ConfigValue is the rate: 1 USD = 278 PKR.
-- Run once on your database.

IF NOT EXISTS (SELECT 1 FROM Configuration WHERE ConfigKey = 'Currency:USDToPKR')
BEGIN
    INSERT INTO Configuration (ConfigKey, Category, ConfigValue, Description, CreatedAt, UpdatedAt, UpdatedBy)
    VALUES (
        'Currency:USDToPKR',
        'Currency',
        '278',
        'Exchange rate: 1 USD = 278 PKR. Used for plan and installment amount conversion.',
        GETDATE(),
        NULL,
        NULL
    );
END
ELSE
BEGIN
    -- Optional: update to 278 if you want to set/override the rate
    UPDATE Configuration SET ConfigValue = '278', Description = 'Exchange rate: 1 USD = 278 PKR. Used for plan and installment amount conversion.' WHERE ConfigKey = 'Currency:USDToPKR';
END
GO
