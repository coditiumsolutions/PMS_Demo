-- Updates Customers.Status default constraint to 'Pending'
-- Safe to run multiple times.
-- If Customers.Status does not exist in current DB, script exits gracefully.

IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.columns c ON c.object_id = t.object_id
    WHERE t.name = 'Customers'
      AND c.name = 'Status'
)
BEGIN
    PRINT 'Customers.Status column not found. No changes applied.';
    RETURN;
END;

DECLARE @ConstraintName NVARCHAR(128);

SELECT @ConstraintName = dc.name
FROM sys.default_constraints dc
INNER JOIN sys.columns c
    ON c.default_object_id = dc.object_id
INNER JOIN sys.tables t
    ON t.object_id = c.object_id
WHERE t.name = 'Customers'
  AND c.name = 'Status';

IF @ConstraintName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE dbo.Customers DROP CONSTRAINT [' + @ConstraintName + ']');
END;

ALTER TABLE dbo.Customers
ADD CONSTRAINT DF_Customers_Status_Default
DEFAULT ('Pending') FOR [Status];
