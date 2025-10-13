-- ===========================================================
-- JUBA SMART CITY PMS - SAMPLE DATA
-- ===========================================================
-- Run this script after creating all tables
-- Database: PMS
-- Server: 172.20.229.3
-- ===========================================================

USE PMS;
GO

-- ===========================================================
-- CONFIGURATION DATA
-- ===========================================================
INSERT INTO Configuration (ConfigKey, Category, ConfigValue, Description) VALUES
('cities', 'Cities', 'Juba,Wau,Malakal,Yei,Bor,Rumbek,Torit,Aweil,Bentiu,Kuacjok', 'Major cities in South Sudan'),
('countries', 'Countries', 'South Sudan', 'Country list'),
('years', 'Years', '2023,2024,2025,2026,2027,2028,2029,2030', 'Available years'),
('property_types', 'PropertyTypes', 'Plot,House,Apartment,Villa,Shop,Office,Warehouse', 'Types of properties'),
('project_types', 'ProjectTypes', 'Residential,Commercial,Mixed-Use,Industrial', 'Types of projects'),
('payment_methods', 'PaymentMethods', 'Cash,Bank Transfer,Cheque,Online,Mobile Money', 'Payment methods'),
('property_sizes', 'Sizes', '5 Marla,10 Marla,1 Kanal,2 Kanal,5 Kanal', 'Common property sizes'),
('blocks', 'Blocks', 'A,B,C,D,E,F,G,H,I,J', 'Property blocks');
GO

-- ===========================================================
-- ROLES & USERS
-- ===========================================================
-- Sample Roles
INSERT INTO ACL (RoleID, RoleName, Permissions) VALUES
('ROLE001', 'Admin', 'All,Manage Users,View Reports,Edit,Delete,Create'),
('ROLE002', 'Manager', 'View Reports,Edit,Create'),
('ROLE003', 'Staff', 'View,Create');
GO

-- Sample Users (Password for all: Admin@123)
INSERT INTO Users (UserID, FullName, Email, PasswordHash, RoleID, IsActive, CreatedAt) VALUES
('USER001', 'Admin User', 'admin@juba.com', '$2a$11$xQqKvZ9Z9YqYZQYZqYZqY.YqYZqYZqYZqYZqYZqYZqYZqYZqYZqY', 'ROLE001', 1, GETDATE()),
('USER002', 'John Manager', 'manager@juba.com', '$2a$11$xQqKvZ9Z9YqYZQYZqYZqY.YqYZqYZqYZqYZqYZqYZqYZqYZqYZqY', 'ROLE002', 1, GETDATE()),
('USER003', 'Jane Staff', 'staff@juba.com', '$2a$11$xQqKvZ9Z9YqYZQYZqYZqY.YqYZqYZqYZqYZqYZqYZqYZqYZqYZqY', 'ROLE003', 1, GETDATE());
GO

-- ===========================================================
-- PROJECTS & PROPERTIES
-- ===========================================================
-- Sample Projects
INSERT INTO Projects (ProjectID, ProjectName, Type, Location, Description, CreatedAt) VALUES
('PROJ001', 'Juba Residential Estate', 'Residential', 'Juba City Center', 'Premium residential development', GETDATE()),
('PROJ002', 'Wau Commercial Plaza', 'Commercial', 'Wau Downtown', 'Modern commercial complex', GETDATE());
GO

-- Sample Properties
INSERT INTO Property (PropertyID, ProjectID, PlotNo, Street, PlotType, [Block], PropertyType, Size, Status, CreatedAt) VALUES
('PROP001', 'PROJ001', 'P-101', 'Main Street', 'Residential', 'A', 'Plot', '5 Marla', 'Available', GETDATE()),
('PROP002', 'PROJ001', 'P-102', 'Main Street', 'Residential', 'A', 'Plot', '10 Marla', 'Available', GETDATE()),
('PROP003', 'PROJ001', 'P-103', 'Garden Street', 'Residential', 'B', 'House', '1 Kanal', 'Allotted', GETDATE()),
('PROP004', 'PROJ002', 'C-201', 'Commerce Avenue', 'Commercial', 'C', 'Shop', '500 sqft', 'Available', GETDATE()),
('PROP005', 'PROJ002', 'C-202', 'Commerce Avenue', 'Commercial', 'C', 'Office', '1000 sqft', 'Allotted', GETDATE());
GO

-- ===========================================================
-- PAYMENT PLANS
-- ===========================================================
INSERT INTO PaymentPlan (PlanID, ProjectID, PlanName, TotalAmount, DurationMonths, Frequency, Description, CreatedAt) VALUES
('PLAN001', 'PROJ001', '3-Year Installment Plan', 500000.00, 36, 'Monthly', 'Standard 3-year monthly payment plan', GETDATE()),
('PLAN002', 'PROJ001', '5-Year Installment Plan', 750000.00, 60, 'Monthly', 'Extended 5-year monthly payment plan', GETDATE()),
('PLAN003', 'PROJ002', '2-Year Commercial Plan', 1200000.00, 24, 'Quarterly', 'Commercial property quarterly plan', GETDATE());
GO

-- ===========================================================
-- REGISTRATIONS & CUSTOMERS
-- ===========================================================
-- Sample Registrations (created over last 6 months)
INSERT INTO Registration (RegID, FullName, CNIC, Phone, Email, CreatedAt, Status) VALUES
('REG001', 'Michael Garang', '12345-6789012-3', '+211-912-345-678', 'michael.g@example.com', DATEADD(MONTH, -5, GETDATE()), 'Approved'),
('REG002', 'Sarah Akol', '23456-7890123-4', '+211-912-345-679', 'sarah.a@example.com', DATEADD(MONTH, -4, GETDATE()), 'Approved'),
('REG003', 'David Deng', '34567-8901234-5', '+211-912-345-680', 'david.d@example.com', DATEADD(MONTH, -3, GETDATE()), 'Approved'),
('REG004', 'Grace Nyandeng', '45678-9012345-6', '+211-912-345-681', 'grace.n@example.com', DATEADD(MONTH, -2, GETDATE()), 'Approved'),
('REG005', 'James Machar', '56789-0123456-7', '+211-912-345-682', 'james.m@example.com', DATEADD(MONTH, -1, GETDATE()), 'Approved'),
('REG006', 'Mary Bol', '67890-1234567-8', '+211-912-345-683', 'mary.b@example.com', DATEADD(DAY, -15, GETDATE()), 'Pending');
GO

-- Sample Customers (created over last 6 months for chart data)
INSERT INTO Customers (CustomerID, RegID, PlanID, FullName, FatherName, CNIC, Phone, Email, City, Country, SubProject, RegisteredSize, Status, CreatedAt) VALUES
('CUST001', 'REG001', 'PLAN001', 'Michael Garang', 'John Garang', '12345-6789012-3', '+211-912-345-678', 'michael.g@example.com', 'Juba', 'South Sudan', 'Phase 1', '5 Marla', 'Active', DATEADD(MONTH, -5, GETDATE())),
('CUST002', 'REG002', 'PLAN001', 'Sarah Akol', 'Peter Akol', '23456-7890123-4', '+211-912-345-679', 'sarah.a@example.com', 'Juba', 'South Sudan', 'Phase 1', '10 Marla', 'Active', DATEADD(MONTH, -4, GETDATE())),
('CUST003', 'REG003', 'PLAN002', 'David Deng', 'William Deng', '34567-8901234-5', '+211-912-345-680', 'david.d@example.com', 'Wau', 'South Sudan', 'Phase 2', '1 Kanal', 'Active', DATEADD(MONTH, -3, GETDATE())),
('CUST004', 'REG004', 'PLAN003', 'Grace Nyandeng', 'Joseph Nyandeng', '45678-9012345-6', '+211-912-345-681', 'grace.n@example.com', 'Juba', 'South Sudan', 'Commercial', '500 sqft', 'Active', DATEADD(MONTH, -2, GETDATE())),
('CUST005', 'REG005', 'PLAN002', 'James Machar', 'Abraham Machar', '56789-0123456-7', '+211-912-345-682', 'james.m@example.com', 'Malakal', 'South Sudan', 'Phase 2', '10 Marla', 'Active', DATEADD(MONTH, -1, GETDATE()));
GO

-- ===========================================================
-- PAYMENT SCHEDULES
-- ===========================================================
INSERT INTO PaymentSchedule (ScheduleID, PlanID, PaymentDescription, InstallmentNo, DueDate, Amount, SurchargeApplied, SurchargeRate) VALUES
-- For PLAN001 (Monthly payments)
('SCH001', 'PLAN001', 'First Installment', 1, DATEADD(MONTH, -4, GETDATE()), 13888.89, 1, 0.05),
('SCH002', 'PLAN001', 'Second Installment', 2, DATEADD(MONTH, -3, GETDATE()), 13888.89, 1, 0.05),
('SCH003', 'PLAN001', 'Third Installment', 3, DATEADD(MONTH, -2, GETDATE()), 13888.89, 1, 0.05),
('SCH004', 'PLAN001', 'Fourth Installment', 4, DATEADD(MONTH, -1, GETDATE()), 13888.89, 1, 0.05),
('SCH005', 'PLAN001', 'Fifth Installment', 5, DATEADD(DAY, 15, GETDATE()), 13888.89, 1, 0.05),
('SCH006', 'PLAN001', 'Sixth Installment', 6, DATEADD(MONTH, 1, GETDATE()), 13888.89, 1, 0.05),

-- For PLAN002 (Monthly payments)
('SCH007', 'PLAN002', 'First Installment', 1, DATEADD(MONTH, -2, GETDATE()), 12500.00, 1, 0.05),
('SCH008', 'PLAN002', 'Second Installment', 2, DATEADD(MONTH, -1, GETDATE()), 12500.00, 1, 0.05),
('SCH009', 'PLAN002', 'Third Installment', 3, DATEADD(DAY, 10, GETDATE()), 12500.00, 1, 0.05),

-- For PLAN003 (Quarterly payments)
('SCH010', 'PLAN003', 'Q1 Payment', 1, DATEADD(MONTH, -3, GETDATE()), 50000.00, 1, 0.05),
('SCH011', 'PLAN003', 'Q2 Payment', 2, GETDATE(), 50000.00, 1, 0.05),
('SCH012', 'PLAN003', 'Q3 Payment', 3, DATEADD(MONTH, 3, GETDATE()), 50000.00, 1, 0.05);
GO

-- ===========================================================
-- PAYMENTS (Distributed over last 6 months for chart data)
-- ===========================================================
INSERT INTO Payments (PaymentID, ScheduleID, CustomerID, PaymentDate, Amount, Method, ReferenceNo, Status, Remarks) VALUES
-- Payments from 5 months ago
('PAY001', 'SCH001', 'CUST001', DATEADD(MONTH, -5, GETDATE()), 13888.89, 'Bank Transfer', 'TXN-2024-001', 'Completed', 'First payment received'),
('PAY002', 'SCH007', 'CUST003', DATEADD(MONTH, -5, GETDATE()), 12500.00, 'Cash', 'CASH-001', 'Completed', 'Cash payment'),

-- Payments from 4 months ago
('PAY003', 'SCH001', 'CUST001', DATEADD(MONTH, -4, GETDATE()), 5000.00, 'Mobile Money', 'MM-2024-001', 'Completed', 'Partial payment'),
('PAY004', 'SCH002', 'CUST002', DATEADD(MONTH, -4, GETDATE()), 13888.89, 'Bank Transfer', 'TXN-2024-002', 'Completed', 'Second installment'),

-- Payments from 3 months ago
('PAY005', 'SCH003', 'CUST001', DATEADD(MONTH, -3, GETDATE()), 13888.89, 'Cheque', 'CHQ-001234', 'Completed', 'Cheque payment'),
('PAY006', 'SCH008', 'CUST003', DATEADD(MONTH, -3, GETDATE()), 12500.00, 'Bank Transfer', 'TXN-2024-003', 'Completed', 'Payment received'),
('PAY007', 'SCH010', 'CUST004', DATEADD(MONTH, -3, GETDATE()), 50000.00, 'Bank Transfer', 'TXN-2024-004', 'Completed', 'Q1 payment'),

-- Payments from 2 months ago
('PAY008', 'SCH004', 'CUST002', DATEADD(MONTH, -2, GETDATE()), 13888.89, 'Online', 'ONL-2024-001', 'Completed', 'Online payment'),
('PAY009', 'SCH007', 'CUST003', DATEADD(MONTH, -2, GETDATE()), 8000.00, 'Cash', 'CASH-002', 'Completed', 'Partial payment'),

-- Payments from 1 month ago
('PAY010', 'SCH004', 'CUST002', DATEADD(MONTH, -1, GETDATE()), 7000.00, 'Mobile Money', 'MM-2024-002', 'Completed', 'Partial payment'),
('PAY011', 'SCH008', 'CUST003', DATEADD(MONTH, -1, GETDATE()), 12500.00, 'Bank Transfer', 'TXN-2024-005', 'Completed', 'Second installment'),

-- Recent payments (current month)
('PAY012', 'SCH005', 'CUST001', DATEADD(DAY, -15, GETDATE()), 13888.89, 'Bank Transfer', 'TXN-2024-006', 'Completed', 'Recent payment'),
('PAY013', 'SCH009', 'CUST005', DATEADD(DAY, -10, GETDATE()), 12500.00, 'Cash', 'CASH-003', 'Completed', 'Cash payment'),
('PAY014', 'SCH011', 'CUST004', DATEADD(DAY, -5, GETDATE()), 25000.00, 'Bank Transfer', 'TXN-2024-007', 'Completed', 'Partial Q2 payment');
GO

-- ===========================================================
-- ALLOTMENTS
-- ===========================================================
INSERT INTO Allotment (AllotmentID, PropertyID, CustomerID, AllottedBy, AllotmentDate, AllottmentType, WorkFlowStatus) VALUES
('ALOT001', 'PROP003', 'CUST003', 'USER001', DATEADD(MONTH, -3, GETDATE()), 'Direct', 'Approved'),
('ALOT002', 'PROP005', 'CUST004', 'USER001', DATEADD(MONTH, -2, GETDATE()), 'Direct', 'Approved'),
('ALOT003', 'PROP001', 'CUST001', 'USER002', DATEADD(DAY, -10, GETDATE()), 'Balloting', 'Pending');
GO

-- ===========================================================
-- SUMMARY
-- ===========================================================
-- This sample data includes:
-- - 3 Roles (Admin, Manager, Staff)
-- - 3 Users
-- - 2 Projects
-- - 5 Properties (3 Available, 2 Allotted)
-- - 3 Payment Plans
-- - 6 Registrations
-- - 5 Customers (spread over last 6 months)
-- - 12 Payment Schedules
-- - 14 Payments (spread over last 6 months - for chart data)
-- - 3 Allotments
-- - 8 Configuration entries
--
-- Charts will show:
-- - Customer Registration Trend: 1 customer per month for 5 months
-- - Monthly Payment Collection: Payments distributed across 6 months
-- - Property Status: 3 Available, 2 Allotted
-- - Payment Status: Mix of overdue, upcoming, and paid schedules
-- ===========================================================
