-- Seed Admin permission for all modules for every user with RoleID = 'ADMIN001'.
-- Safe to re-run (INSERT only when no row exists for that UserID+ModuleKey).

DECLARE @ModuleKeys TABLE (ModuleKey NVARCHAR(50));
INSERT INTO @ModuleKeys (ModuleKey) VALUES
 ('Home'),('Registration'),('Customer'),('Transfer'),('NDC'),('Project'),('Dealer'),('Property'),('Payment'),
 ('Allotment'),('Rental'),('SalesInquiry'),('Reports'),('Account'),('Settings'),('ActivityLog'),
 ('AccountsManagement'),('Ticket'),('TesSQL'),('InquiryApi');

INSERT INTO UserModulePermission (UserID, ModuleKey, Permission)
SELECT u.UserID, m.ModuleKey, 'Admin'
FROM Users u
CROSS JOIN @ModuleKeys m
WHERE u.RoleID = 'ADMIN001'
  AND NOT EXISTS (SELECT 1 FROM UserModulePermission p WHERE p.UserID = u.UserID AND p.ModuleKey = m.ModuleKey);
GO
