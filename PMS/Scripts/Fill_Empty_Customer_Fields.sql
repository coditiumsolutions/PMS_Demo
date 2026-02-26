-- Fill empty/NULL customer fields with test data (test DB only)
USE PMSAbbas;
GO

-- 1) Set Country where NULL
UPDATE Customers SET Country = N'Pakistan' WHERE NULLIF(LTRIM(RTRIM(ISNULL(Country,''))),'') IS NULL;

-- 2) Set City where NULL
UPDATE Customers SET City = N'Karachi' WHERE NULLIF(LTRIM(RTRIM(ISNULL(City,''))),'') IS NULL;

-- 3) Set MailingAddress where NULL
UPDATE Customers SET MailingAddress = N'Test Address, Karachi, Pakistan' 
WHERE NULLIF(LTRIM(RTRIM(ISNULL(MailingAddress,''))),'') IS NULL;

-- 4) Set Status where NULL
UPDATE Customers SET Status = N'Active' WHERE NULLIF(LTRIM(RTRIM(ISNULL(Status,''))),'') IS NULL;

-- 5) Where both CNIC and PassportNo are NULL, set CNIC to valid test format
UPDATE Customers 
SET CNIC = N'11111-1111111-1' 
WHERE NULLIF(LTRIM(RTRIM(ISNULL(CNIC,''))),'') IS NULL 
  AND NULLIF(LTRIM(RTRIM(ISNULL(PassportNo,''))),'') IS NULL;

-- 6) Where CNIC is invalid (e.g. 1-1-1) and PassportNo is NULL, set PassportNo so at least one ID exists
UPDATE Customers 
SET PassportNo = N'PASS' + REPLACE(REPLACE(CustomerID,' ',''),'-','') 
WHERE NULLIF(LTRIM(RTRIM(ISNULL(PassportNo,''))),'') IS NULL 
  AND (CNIC NOT LIKE N'%-%-%' OR LEN(CNIC) < 13);

-- Report
SELECT 'After fill:' AS Step;
SELECT CustomerID, FullName, City, Country, MailingAddress, CNIC, PassportNo, Status
FROM Customers 
WHERE CustomerID IN ('1C1500108','APT00101','APT00103','ZH 100003');
GO
