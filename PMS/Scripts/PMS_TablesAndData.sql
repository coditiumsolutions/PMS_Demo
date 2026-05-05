-- ================================================================
-- PMS migration SQL (tables + data)
-- Generated: 2026-05-05 11:16:19
-- Source: localhost / PMSAbbas
-- ================================================================
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF DB_ID(N'PMSAbbas') IS NULL
BEGIN
    CREATE DATABASE [PMSAbbas];
END
GO
USE [PMSAbbas];
GO

-- ===================== TABLES ====================

/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 5/5/2026 11:16:20 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[ProductVersion] [nvarchar](32) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


GO

/****** Object:  Table [dbo].[ACL]    Script Date: 5/5/2026 11:16:20 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[ACL](
	[RoleID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[RoleName] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Permissions] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[RoleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

SET ANSI_PADDING OFF

GO

/****** Object:  Table [dbo].[ActivityLog]    Script Date: 5/5/2026 11:16:20 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[ActivityLog](
	[LogID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Action] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[RefType] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[RefID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreatedAt] [datetime] NOT NULL,
	[Details] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[LogID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

SET ANSI_PADDING OFF
ALTER TABLE [dbo].[ActivityLog] ADD  DEFAULT (getdate()) FOR [CreatedAt]
ALTER TABLE [dbo].[ActivityLog]  WITH NOCHECK ADD FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
ON DELETE SET NULL

GO

/****** Object:  Table [dbo].[Allotment]    Script Date: 5/5/2026 11:16:21 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[Allotment](
	[AllotmentID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[PropertyID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CustomerID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[AllottedBy] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[AllotmentDate] [datetime] NULL,
	[ApprovedBy] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[AllottmentType] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[WorkFlowStatus] [nvarchar](250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Comments] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[AdditionalInfo] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[AllotmentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

SET ANSI_PADDING OFF
ALTER TABLE [dbo].[Allotment] ADD  DEFAULT (getdate()) FOR [AllotmentDate]
ALTER TABLE [dbo].[Allotment]  WITH NOCHECK ADD FOREIGN KEY([AllottedBy])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[Allotment]  WITH NOCHECK ADD FOREIGN KEY([CustomerID])
REFERENCES [dbo].[Customers] ([CustomerID])
ALTER TABLE [dbo].[Allotment]  WITH NOCHECK ADD FOREIGN KEY([PropertyID])
REFERENCES [dbo].[Property] ([PropertyID])

GO

/****** Object:  Table [dbo].[Allotments]    Script Date: 5/5/2026 11:16:21 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[Allotments](
	[AllotmentID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[PropertyID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CustomerID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[AllottedBy] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[AllotmentDate] [datetime] NOT NULL,
	[AllottmentType] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[WorkFlowStatus] [nvarchar](250) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[ApprovedBy] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Status] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Remarks] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[AllotmentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

SET ANSI_PADDING OFF
ALTER TABLE [dbo].[Allotments] ADD  DEFAULT (getdate()) FOR [AllotmentDate]
ALTER TABLE [dbo].[Allotments] ADD  DEFAULT (getdate()) FOR [CreatedAt]
ALTER TABLE [dbo].[Allotments]  WITH NOCHECK ADD FOREIGN KEY([AllottedBy])
REFERENCES [dbo].[Users] ([UserID])
ON DELETE SET NULL
ALTER TABLE [dbo].[Allotments]  WITH NOCHECK ADD FOREIGN KEY([CustomerID])
REFERENCES [dbo].[Customers] ([CustomerID])
ON DELETE CASCADE
ALTER TABLE [dbo].[Allotments]  WITH NOCHECK ADD FOREIGN KEY([PropertyID])
REFERENCES [dbo].[Property] ([PropertyID])
ON DELETE CASCADE

GO

/****** Object:  Table [dbo].[Approvals]    Script Date: 5/5/2026 11:16:21 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[Approvals](
	[ApprovalID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[RefType] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[RefID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Status] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[RequestedBy] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[RequestedAt] [datetime] NULL,
	[ApprovedBy] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[ApprovedAt] [datetime] NULL,
	[RejectedBy] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[RejectedAt] [datetime] NULL,
	[Remarks] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[ApprovalID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

SET ANSI_PADDING OFF
ALTER TABLE [dbo].[Approvals]  WITH NOCHECK ADD  CONSTRAINT [FK_Approvals_ApprovedBy] FOREIGN KEY([ApprovedBy])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[Approvals] NOCHECK CONSTRAINT [FK_Approvals_ApprovedBy]
ALTER TABLE [dbo].[Approvals]  WITH NOCHECK ADD  CONSTRAINT [FK_Approvals_RejectedBy] FOREIGN KEY([RejectedBy])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[Approvals] NOCHECK CONSTRAINT [FK_Approvals_RejectedBy]
ALTER TABLE [dbo].[Approvals]  WITH NOCHECK ADD  CONSTRAINT [FK_Approvals_RequestedBy] FOREIGN KEY([RequestedBy])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[Approvals] NOCHECK CONSTRAINT [FK_Approvals_RequestedBy]

GO

/****** Object:  Table [dbo].[Attachments]    Script Date: 5/5/2026 11:16:21 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[Attachments](
	[AttachmentID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[RefType] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[RefID] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[FileName] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[FilePath] [nvarchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[FileType] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[FileSize] [bigint] NULL,
	[UploadedBy] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[UploadedAt] [datetime] NOT NULL,
	[Description] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[AttachmentType] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[AttachmentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

SET ANSI_PADDING OFF
ALTER TABLE [dbo].[Attachments] ADD  DEFAULT (getdate()) FOR [UploadedAt]
ALTER TABLE [dbo].[Attachments]  WITH NOCHECK ADD FOREIGN KEY([UploadedBy])
REFERENCES [dbo].[Users] ([UserID])
ON DELETE SET NULL

GO

/****** Object:  Table [dbo].[Balloting]    Script Date: 5/5/2026 11:16:22 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[Balloting](
	[BallotID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[ProjectID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[ConductedBy] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[ConductedAt] [datetime] NULL,
	[Remarks] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[BallotID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

SET ANSI_PADDING OFF
ALTER TABLE [dbo].[Balloting] ADD  DEFAULT (getdate()) FOR [ConductedAt]
ALTER TABLE [dbo].[Balloting]  WITH NOCHECK ADD FOREIGN KEY([ConductedBy])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[Balloting]  WITH NOCHECK ADD FOREIGN KEY([ProjectID])
REFERENCES [dbo].[Projects] ([ProjectID])

GO

/****** Object:  Table [dbo].[Ballotings]    Script Date: 5/5/2026 11:16:22 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[Ballotings](
	[BallotingID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[ProjectID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[BallotingDate] [datetime] NOT NULL,
	[Status] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[ConductedBy] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Remarks] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[BallotingID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

SET ANSI_PADDING OFF
ALTER TABLE [dbo].[Ballotings] ADD  DEFAULT (getdate()) FOR [CreatedAt]
ALTER TABLE [dbo].[Ballotings]  WITH NOCHECK ADD FOREIGN KEY([ConductedBy])
REFERENCES [dbo].[Users] ([UserID])
ON DELETE SET NULL
ALTER TABLE [dbo].[Ballotings]  WITH NOCHECK ADD FOREIGN KEY([ProjectID])
REFERENCES [dbo].[Projects] ([ProjectID])
ON DELETE SET NULL

GO

/****** Object:  Table [dbo].[BlockingLogs]    Script Date: 5/5/2026 11:16:22 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[BlockingLogs](
	[BlockingLogID] [int] IDENTITY(1,1) NOT NULL,
	[CustomerID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[UserID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[ActionDate] [datetime] NULL,
	[PreviousStatus] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[NewStatus] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Reason] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[AttachmentPath] [nvarchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[BlockingLogID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

SET ANSI_PADDING OFF
ALTER TABLE [dbo].[BlockingLogs] ADD  DEFAULT (getdate()) FOR [ActionDate]
ALTER TABLE [dbo].[BlockingLogs]  WITH CHECK ADD  CONSTRAINT [FK_BlockingLogs_Customer] FOREIGN KEY([CustomerID])
REFERENCES [dbo].[Customers] ([CustomerID])
ALTER TABLE [dbo].[BlockingLogs] CHECK CONSTRAINT [FK_BlockingLogs_Customer]
ALTER TABLE [dbo].[BlockingLogs]  WITH CHECK ADD  CONSTRAINT [FK_BlockingLogs_User] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[BlockingLogs] CHECK CONSTRAINT [FK_BlockingLogs_User]

GO

/****** Object:  Table [dbo].[Configuration]    Script Date: 5/5/2026 11:16:22 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[Configuration](
	[ConfigKey] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Category] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[ConfigValue] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Description] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreatedAt] [datetime] NOT NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[ConfigKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

SET ANSI_PADDING OFF
ALTER TABLE [dbo].[Configuration] ADD  DEFAULT (getdate()) FOR [CreatedAt]
ALTER TABLE [dbo].[Configuration]  WITH NOCHECK ADD  CONSTRAINT [FK_Configuration_Users_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Users] ([UserID])
ON DELETE SET NULL
ALTER TABLE [dbo].[Configuration] CHECK CONSTRAINT [FK_Configuration_Users_UpdatedBy]

GO

/****** Object:  Table [dbo].[CustomerLogs]    Script Date: 5/5/2026 11:16:22 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[CustomerLogs](
	[LogID] [int] IDENTITY(1,1) NOT NULL,
	[CustomerID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Action] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Remarks] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[LogID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

SET ANSI_PADDING OFF
ALTER TABLE [dbo].[CustomerLogs] ADD  DEFAULT (getdate()) FOR [CreatedAt]
ALTER TABLE [dbo].[CustomerLogs]  WITH NOCHECK ADD FOREIGN KEY([CustomerID])
REFERENCES [dbo].[Customers] ([CustomerID])

GO

/****** Object:  Table [dbo].[Customers]    Script Date: 5/5/2026 11:16:23 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[Customers](
	[CustomerID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[RegID] [nvarchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[PlanID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[FullName] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[FatherName] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CNIC] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[PassportNo] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[DOB] [date] NULL,
	[Gender] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Phone] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Email] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[MailingAddress] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[PermanentAddress] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[City] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Country] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[SubProject] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[RegisteredSize] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreatedAt] [datetime] NULL,
	[Status] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[NomineeName] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[NomineeID] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[NomineeRelation] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[AdditionalInfo] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[DealerID] [int] NULL,
	[ProjectID] [nvarchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Nationality] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[NomineeNICDocumentPath] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[NomineePicturePath] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[DealerName] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[isDealerRegistered] [int] NULL,
	[MobileNo] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[MobileNo2] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[FormNo] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[CustomerID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

SET ANSI_PADDING OFF
ALTER TABLE [dbo].[Customers] ADD  DEFAULT (N'') FOR [Phone]
ALTER TABLE [dbo].[Customers] ADD  DEFAULT (getdate()) FOR [CreatedAt]
ALTER TABLE [dbo].[Customers] ADD  CONSTRAINT [DF_Customers_Status_Default]  DEFAULT ('Pending') FOR [Status]
ALTER TABLE [dbo].[Customers] ADD  DEFAULT (N'') FOR [MobileNo]
ALTER TABLE [dbo].[Customers]  WITH NOCHECK ADD FOREIGN KEY([PlanID])
REFERENCES [dbo].[PaymentPlan] ([PlanID])
ALTER TABLE [dbo].[Customers]  WITH NOCHECK ADD FOREIGN KEY([RegID])
REFERENCES [dbo].[Registration] ([RegID])
ON DELETE SET NULL

GO

/****** Object:  Table [dbo].[Dealers]    Script Date: 5/5/2026 11:16:23 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Dealers](
	[DealerID] [int] IDENTITY(1,1) NOT NULL,
	[DealershipName] [nvarchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[RegisterationDate] [date] NOT NULL,
	[Status] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[MembershipType] [nchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[OwnerName] [nvarchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[OwnerCNIC] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[MobileNo] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[PhoneNumber] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Email] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Address] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[OwnerDetails] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Details] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[IncentivePercentage] [float] NULL,
PRIMARY KEY CLUSTERED 
(
	[DealerID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

ALTER TABLE [dbo].[Dealers] ADD  DEFAULT ((5.0)) FOR [IncentivePercentage]

GO

/****** Object:  Table [dbo].[DuplicateFileTransfer]    Script Date: 5/5/2026 11:16:23 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[DuplicateFileTransfer](
	[Id] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CustomerID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CustomerName] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CustomerCNIC] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Created_at] [datetime] NOT NULL,
	[Created_by] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Modified_by] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Status] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Comments] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[FeeDue] [decimal](18, 2) NULL,
	[FeePaid] [decimal](18, 2) NULL,
	[ChallanID] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[BankName] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[InstrumentNo] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[DepositDate] [date] NULL,
	[PaymentMethod] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
 CONSTRAINT [PK_DuplicateFileTransfer] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

SET ANSI_PADDING OFF
ALTER TABLE [dbo].[DuplicateFileTransfer] ADD  CONSTRAINT [DF_DuplicateFileTransfer_Created_at]  DEFAULT (getdate()) FOR [Created_at]
ALTER TABLE [dbo].[DuplicateFileTransfer]  WITH CHECK ADD  CONSTRAINT [FK_DuplicateFileTransfer_Customers] FOREIGN KEY([CustomerID])
REFERENCES [dbo].[Customers] ([CustomerID])
ALTER TABLE [dbo].[DuplicateFileTransfer] CHECK CONSTRAINT [FK_DuplicateFileTransfer_Customers]

GO

/****** Object:  Table [dbo].[JointOwner]    Script Date: 5/5/2026 11:16:23 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[JointOwner](
	[Id] [nvarchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CustomerID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[JointOwnerName] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CNIC] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Contact] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Address] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Percentage] [decimal](5, 2) NULL,
	[Created_at] [datetime] NOT NULL,
	[Created_by] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Modified_by] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Details] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[FatherName] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

SET ANSI_PADDING OFF
SET ANSI_PADDING ON

/****** Object:  Index [IX_JointOwner_CustomerID]    Script Date: 5/5/2026 11:16:23 AM ******/
CREATE NONCLUSTERED INDEX [IX_JointOwner_CustomerID] ON [dbo].[JointOwner]
(
	[CustomerID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
ALTER TABLE [dbo].[JointOwner] ADD  CONSTRAINT [DF_JointOwner_CreatedAt]  DEFAULT (getdate()) FOR [Created_at]
ALTER TABLE [dbo].[JointOwner]  WITH CHECK ADD  CONSTRAINT [FK_JointOwner_Customers] FOREIGN KEY([CustomerID])
REFERENCES [dbo].[Customers] ([CustomerID])
ON DELETE CASCADE
ALTER TABLE [dbo].[JointOwner] CHECK CONSTRAINT [FK_JointOwner_Customers]

GO

/****** Object:  Table [dbo].[NDC]    Script Date: 5/5/2026 11:16:23 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[NDC](
	[NDCID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CustomerID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[NDCType] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Title] [nvarchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[WorkFlowStatus] [nvarchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Comments] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[IssuedDate] [datetime2](7) NOT NULL,
	[Remarks] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[CreatedBy] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[NDCExpiryDate] [date] NULL,
	[TotalDueAmount] [decimal](18, 2) NULL,
	[TotalDueInstallments] [decimal](18, 2) NULL,
	[AllPaymentClear] [bit] NOT NULL,
	[AmountPerUnit] [decimal](18, 2) NULL,
	[PropertySize] [decimal](18, 2) NULL,
	[TransferFeeAmount] [decimal](18, 2) NULL,
	[RemainingDues] [decimal](18, 2) NULL,
 CONSTRAINT [PK_NDC] PRIMARY KEY CLUSTERED 
(
	[NDCID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

SET ANSI_PADDING OFF
SET ANSI_PADDING ON

/****** Object:  Index [IX_NDC_CustomerID]    Script Date: 5/5/2026 11:16:23 AM ******/
CREATE NONCLUSTERED INDEX [IX_NDC_CustomerID] ON [dbo].[NDC]
(
	[CustomerID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
ALTER TABLE [dbo].[NDC] ADD  DEFAULT (getdate()) FOR [IssuedDate]
ALTER TABLE [dbo].[NDC] ADD  DEFAULT (getdate()) FOR [CreatedAt]
ALTER TABLE [dbo].[NDC] ADD  DEFAULT ((0)) FOR [AllPaymentClear]
ALTER TABLE [dbo].[NDC]  WITH CHECK ADD  CONSTRAINT [FK_NDC_Customers_CustomerID] FOREIGN KEY([CustomerID])
REFERENCES [dbo].[Customers] ([CustomerID])
ON DELETE CASCADE
ALTER TABLE [dbo].[NDC] CHECK CONSTRAINT [FK_NDC_Customers_CustomerID]

GO

/****** Object:  Table [dbo].[NDCs]    Script Date: 5/5/2026 11:16:24 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[NDCs](
	[NDCID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CustomerID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[PropertyID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[IssuedDate] [datetime] NOT NULL,
	[ExpiryDate] [datetime] NULL,
	[Status] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[IssuedBy] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Remarks] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[NDCID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

SET ANSI_PADDING OFF
ALTER TABLE [dbo].[NDCs] ADD  DEFAULT (getdate()) FOR [CreatedAt]
ALTER TABLE [dbo].[NDCs]  WITH NOCHECK ADD FOREIGN KEY([CustomerID])
REFERENCES [dbo].[Customers] ([CustomerID])
ON DELETE CASCADE
ALTER TABLE [dbo].[NDCs]  WITH NOCHECK ADD FOREIGN KEY([IssuedBy])
REFERENCES [dbo].[Users] ([UserID])
ON DELETE SET NULL
ALTER TABLE [dbo].[NDCs]  WITH NOCHECK ADD FOREIGN KEY([PropertyID])
REFERENCES [dbo].[Property] ([PropertyID])
ON DELETE SET NULL

GO

/****** Object:  Table [dbo].[Notifications]    Script Date: 5/5/2026 11:16:24 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[Notifications](
	[NotificationID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[UserID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Message] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[IsRead] [bit] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[NotificationID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

SET ANSI_PADDING OFF
ALTER TABLE [dbo].[Notifications] ADD  DEFAULT ((0)) FOR [IsRead]
ALTER TABLE [dbo].[Notifications] ADD  DEFAULT (getdate()) FOR [CreatedAt]
ALTER TABLE [dbo].[Notifications]  WITH NOCHECK ADD FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
ON DELETE CASCADE

GO

/****** Object:  Table [dbo].[PaymentPlan]    Script Date: 5/5/2026 11:16:24 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[PaymentPlan](
	[PlanID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[ProjectID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[PlanName] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[TotalAmount] [decimal](18, 2) NULL,
	[DurationMonths] [int] NULL,
	[Frequency] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Description] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreatedAt] [datetime] NULL,
	[Currency] [nvarchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[ExchangeRate] [decimal](18, 4) NULL,
	[TotalAmountUSD] [decimal](18, 2) NULL,
	[RegisteredSize] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[SubProject] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[PlanID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

SET ANSI_PADDING OFF
ALTER TABLE [dbo].[PaymentPlan] ADD  DEFAULT (getdate()) FOR [CreatedAt]
ALTER TABLE [dbo].[PaymentPlan] ADD  DEFAULT ('SSP') FOR [Currency]

GO

/****** Object:  Table [dbo].[Payments]    Script Date: 5/5/2026 11:16:24 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[Payments](
	[PaymentID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[ScheduleID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CustomerID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[PaymentDate] [datetime] NOT NULL,
	[PaymentMethod] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[ReferenceNumber] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Status] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Remarks] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreatedBy] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreatedAt] [datetime] NOT NULL,
	[AuditRemarks] [nvarchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[AuditStatus] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[AuditedAt] [datetime2](7) NULL,
	[AuditedBy] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[AccountHead] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[DepositDate] [date] NULL,
	[BankName] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[PaymentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

SET ANSI_PADDING OFF
SET ANSI_PADDING ON

/****** Object:  Index [IX_Payments_AuditedBy]    Script Date: 5/5/2026 11:16:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_Payments_AuditedBy] ON [dbo].[Payments]
(
	[AuditedBy] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
ALTER TABLE [dbo].[Payments] ADD  DEFAULT ('Pending') FOR [Status]
ALTER TABLE [dbo].[Payments] ADD  DEFAULT (getdate()) FOR [CreatedAt]
ALTER TABLE [dbo].[Payments] ADD  DEFAULT (N'Pending') FOR [AuditStatus]
ALTER TABLE [dbo].[Payments]  WITH NOCHECK ADD FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Users] ([UserID])
ON DELETE SET NULL
ALTER TABLE [dbo].[Payments]  WITH NOCHECK ADD FOREIGN KEY([CustomerID])
REFERENCES [dbo].[Customers] ([CustomerID])
ON DELETE CASCADE
ALTER TABLE [dbo].[Payments]  WITH NOCHECK ADD FOREIGN KEY([ScheduleID])
REFERENCES [dbo].[PaymentSchedule] ([ScheduleID])
ON DELETE SET NULL
ALTER TABLE [dbo].[Payments]  WITH CHECK ADD  CONSTRAINT [FK_Payments_Users_AuditedBy] FOREIGN KEY([AuditedBy])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[Payments] CHECK CONSTRAINT [FK_Payments_Users_AuditedBy]
/****** Object:  Trigger [dbo].[TRG_Payments_ValidateBankReferenceCrossCustomer]    Script Date: 5/5/2026 11:16:24 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF
CREATE   TRIGGER TRG_Payments_ValidateBankReferenceCrossCustomer ON Payments AFTER INSERT, UPDATE AS BEGIN SET NOCOUNT ON; IF EXISTS ( SELECT 1 FROM inserted i JOIN Payments p ON p.PaymentID <> i.PaymentID AND ISNULL(LTRIM(RTRIM(i.BankName)),'') <> '' AND ISNULL(LTRIM(RTRIM(i.ReferenceNumber)),'') <> '' AND ISNULL(LTRIM(RTRIM(p.BankName)),'') = LTRIM(RTRIM(i.BankName)) AND ISNULL(LTRIM(RTRIM(p.ReferenceNumber)),'') = LTRIM(RTRIM(i.ReferenceNumber)) AND ISNULL(LTRIM(RTRIM(p.CustomerID)),'') <> ISNULL(LTRIM(RTRIM(i.CustomerID)),'') ) BEGIN RAISERROR('The combination of Bank Name and Reference Number is already used by another customer.',16,1); ROLLBACK TRANSACTION; RETURN; END END;
ALTER TABLE [dbo].[Payments] ENABLE TRIGGER [TRG_Payments_ValidateBankReferenceCrossCustomer]

GO

/****** Object:  Table [dbo].[PaymentSchedule]    Script Date: 5/5/2026 11:16:25 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[PaymentSchedule](
	[ScheduleID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[PlanID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[PaymentDescription] [nvarchar](250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[InstallmentNo] [int] NULL,
	[DueDate] [date] NULL,
	[Amount] [decimal](18, 2) NULL,
	[SurchargeApplied] [bit] NULL,
	[SurchargeRate] [decimal](18, 6) NOT NULL,
	[Description] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[AmountUSD] [decimal](18, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[ScheduleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

SET ANSI_PADDING OFF
ALTER TABLE [dbo].[PaymentSchedule] ADD  DEFAULT ((1)) FOR [SurchargeApplied]
ALTER TABLE [dbo].[PaymentSchedule]  WITH NOCHECK ADD FOREIGN KEY([PlanID])
REFERENCES [dbo].[PaymentPlan] ([PlanID])

GO

/****** Object:  Table [dbo].[Penalties]    Script Date: 5/5/2026 11:16:25 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[Penalties](
	[PenaltyID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CustomerID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[PenaltyDate] [datetime] NOT NULL,
	[PenaltyReason] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Status] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Remarks] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[PenaltyID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

SET ANSI_PADDING OFF
ALTER TABLE [dbo].[Penalties] ADD  DEFAULT (getdate()) FOR [CreatedAt]
ALTER TABLE [dbo].[Penalties]  WITH NOCHECK ADD FOREIGN KEY([CustomerID])
REFERENCES [dbo].[Customers] ([CustomerID])
ON DELETE CASCADE

GO

/****** Object:  Table [dbo].[Possession]    Script Date: 5/5/2026 11:16:25 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[Possession](
	[PossessionID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[PropertyID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CustomerID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[PossessionDate] [datetime] NULL,
	[WorkFlowStatus] [nvarchar](250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Comments] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Remarks] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[PossessionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

SET ANSI_PADDING OFF
ALTER TABLE [dbo].[Possession] ADD  DEFAULT (getdate()) FOR [PossessionDate]
ALTER TABLE [dbo].[Possession]  WITH NOCHECK ADD FOREIGN KEY([CustomerID])
REFERENCES [dbo].[Customers] ([CustomerID])
ALTER TABLE [dbo].[Possession]  WITH NOCHECK ADD FOREIGN KEY([PropertyID])
REFERENCES [dbo].[Property] ([PropertyID])

GO

/****** Object:  Table [dbo].[Possessions]    Script Date: 5/5/2026 11:16:25 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[Possessions](
	[PossessionID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[PropertyID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CustomerID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[PossessionDate] [datetime] NOT NULL,
	[Status] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Remarks] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[PossessionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

SET ANSI_PADDING OFF
ALTER TABLE [dbo].[Possessions] ADD  DEFAULT (getdate()) FOR [CreatedAt]
ALTER TABLE [dbo].[Possessions]  WITH NOCHECK ADD  CONSTRAINT [FK_Possessions_Customer] FOREIGN KEY([CustomerID])
REFERENCES [dbo].[Customers] ([CustomerID])
ALTER TABLE [dbo].[Possessions] NOCHECK CONSTRAINT [FK_Possessions_Customer]
ALTER TABLE [dbo].[Possessions]  WITH NOCHECK ADD  CONSTRAINT [FK_Possessions_Property] FOREIGN KEY([PropertyID])
REFERENCES [dbo].[Property] ([PropertyID])
ALTER TABLE [dbo].[Possessions] NOCHECK CONSTRAINT [FK_Possessions_Property]

GO

/****** Object:  Table [dbo].[Projects]    Script Date: 5/5/2026 11:16:25 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[Projects](
	[ProjectID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[ProjectName] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Prefix] [nvarchar](7) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Type] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Location] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Description] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreatedAt] [datetime] NULL,
	[Sizes] [nvarchar](1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[PropertyTypes] [nvarchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[SubProjects] [nvarchar](1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[ProjectID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

SET ANSI_PADDING OFF
ALTER TABLE [dbo].[Projects] ADD  DEFAULT (getdate()) FOR [CreatedAt]

GO

/****** Object:  Table [dbo].[ProjectSubProjects]    Script Date: 5/5/2026 11:16:26 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[ProjectSubProjects](
	[Id] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[ProjectID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SubProjectName] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Prefix] [nvarchar](7) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

SET ANSI_PADDING OFF
SET ANSI_PADDING ON

/****** Object:  Index [UX_ProjectSubProjects_Project_Prefix]    Script Date: 5/5/2026 11:16:26 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_ProjectSubProjects_Project_Prefix] ON [dbo].[ProjectSubProjects]
(
	[ProjectID] ASC,
	[Prefix] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
SET ANSI_PADDING ON

/****** Object:  Index [UX_ProjectSubProjects_Project_SubProjectName]    Script Date: 5/5/2026 11:16:26 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_ProjectSubProjects_Project_SubProjectName] ON [dbo].[ProjectSubProjects]
(
	[ProjectID] ASC,
	[SubProjectName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
ALTER TABLE [dbo].[ProjectSubProjects] ADD  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
ALTER TABLE [dbo].[ProjectSubProjects]  WITH CHECK ADD  CONSTRAINT [FK_ProjectSubProjects_Projects_ProjectID] FOREIGN KEY([ProjectID])
REFERENCES [dbo].[Projects] ([ProjectID])
ON DELETE CASCADE
ALTER TABLE [dbo].[ProjectSubProjects] CHECK CONSTRAINT [FK_ProjectSubProjects_Projects_ProjectID]

GO

/****** Object:  Table [dbo].[Property]    Script Date: 5/5/2026 11:16:26 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[Property](
	[PropertyID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[ProjectID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[PlotNo] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Street] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[PlotType] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Block] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[PropertyType] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Size] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Status] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreatedAt] [datetime] NULL,
	[AdditionalInfo] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[DealerID] [int] NULL,
	[Floor] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[SubProject] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[PropertyID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

SET ANSI_PADDING OFF
ALTER TABLE [dbo].[Property] ADD  DEFAULT ('Available') FOR [Status]
ALTER TABLE [dbo].[Property] ADD  DEFAULT (getdate()) FOR [CreatedAt]
ALTER TABLE [dbo].[Property]  WITH NOCHECK ADD FOREIGN KEY([ProjectID])
REFERENCES [dbo].[Projects] ([ProjectID])

GO

/****** Object:  Table [dbo].[PropertyInquiry]    Script Date: 5/5/2026 11:16:26 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[PropertyInquiry](
	[InquiryID] [int] IDENTITY(1,1) NOT NULL,
	[FullName] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[PhoneNumber] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[EmailAddress] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[InquiryType] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Message] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[SubmittedAt] [datetime] NULL,
	[IPAddress] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Status] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[AssignedTo] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[FollowUpDate] [datetime] NULL,
	[Notes] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[IsContacted] [bit] NULL,
	[CreatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[InquiryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

ALTER TABLE [dbo].[PropertyInquiry] ADD  DEFAULT (getdate()) FOR [SubmittedAt]
ALTER TABLE [dbo].[PropertyInquiry] ADD  DEFAULT ('New') FOR [Status]
ALTER TABLE [dbo].[PropertyInquiry] ADD  DEFAULT ((0)) FOR [IsContacted]
ALTER TABLE [dbo].[PropertyInquiry] ADD  DEFAULT (getdate()) FOR [CreatedAt]

GO

/****** Object:  Table [dbo].[PropertyLogs]    Script Date: 5/5/2026 11:16:26 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[PropertyLogs](
	[LogID] [int] IDENTITY(1,1) NOT NULL,
	[PropertyID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Action] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[OldValue] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[NewValue] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Remarks] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[LogID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

SET ANSI_PADDING OFF
ALTER TABLE [dbo].[PropertyLogs] ADD  DEFAULT (getdate()) FOR [CreatedAt]
ALTER TABLE [dbo].[PropertyLogs]  WITH NOCHECK ADD  CONSTRAINT [FK_PropertyLogs_Property] FOREIGN KEY([PropertyID])
REFERENCES [dbo].[Property] ([PropertyID])
ALTER TABLE [dbo].[PropertyLogs] NOCHECK CONSTRAINT [FK_PropertyLogs_Property]
ALTER TABLE [dbo].[PropertyLogs]  WITH NOCHECK ADD  CONSTRAINT [FK_PropertyLogs_Users] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[PropertyLogs] NOCHECK CONSTRAINT [FK_PropertyLogs_Users]

GO

/****** Object:  Table [dbo].[Refund]    Script Date: 5/5/2026 11:16:26 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[Refund](
	[RefundID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CustomerID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[RefundType] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[PaidAmount] [decimal](18, 2) NOT NULL,
	[DeductionAmount] [decimal](18, 2) NOT NULL,
	[RefundedAmount] [decimal](18, 2) NOT NULL,
	[Reason] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[WorkflowStatus] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[SelectedPaymentIDs] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreatedBy] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[ApprovedBy] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[ApprovedAt] [datetime2](7) NULL,
	[Notes] [nvarchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
 CONSTRAINT [PK_Refund] PRIMARY KEY CLUSTERED 
(
	[RefundID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

SET ANSI_PADDING OFF
SET ANSI_PADDING ON

/****** Object:  Index [IX_Refund_ApprovedBy]    Script Date: 5/5/2026 11:16:26 AM ******/
CREATE NONCLUSTERED INDEX [IX_Refund_ApprovedBy] ON [dbo].[Refund]
(
	[ApprovedBy] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
SET ANSI_PADDING ON

/****** Object:  Index [IX_Refund_CreatedBy]    Script Date: 5/5/2026 11:16:26 AM ******/
CREATE NONCLUSTERED INDEX [IX_Refund_CreatedBy] ON [dbo].[Refund]
(
	[CreatedBy] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
SET ANSI_PADDING ON

/****** Object:  Index [IX_Refund_CustomerID]    Script Date: 5/5/2026 11:16:26 AM ******/
CREATE NONCLUSTERED INDEX [IX_Refund_CustomerID] ON [dbo].[Refund]
(
	[CustomerID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
ALTER TABLE [dbo].[Refund] ADD  DEFAULT ((0.0)) FOR [PaidAmount]
ALTER TABLE [dbo].[Refund] ADD  DEFAULT ((0.0)) FOR [DeductionAmount]
ALTER TABLE [dbo].[Refund] ADD  DEFAULT ((0.0)) FOR [RefundedAmount]
ALTER TABLE [dbo].[Refund] ADD  DEFAULT (N'Initiated') FOR [WorkflowStatus]
ALTER TABLE [dbo].[Refund] ADD  DEFAULT (getdate()) FOR [CreatedAt]
ALTER TABLE [dbo].[Refund]  WITH CHECK ADD  CONSTRAINT [FK_Refund_Customers_CustomerID] FOREIGN KEY([CustomerID])
REFERENCES [dbo].[Customers] ([CustomerID])
ON DELETE CASCADE
ALTER TABLE [dbo].[Refund] CHECK CONSTRAINT [FK_Refund_Customers_CustomerID]
ALTER TABLE [dbo].[Refund]  WITH CHECK ADD  CONSTRAINT [FK_Refund_Users_ApprovedBy] FOREIGN KEY([ApprovedBy])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[Refund] CHECK CONSTRAINT [FK_Refund_Users_ApprovedBy]
ALTER TABLE [dbo].[Refund]  WITH CHECK ADD  CONSTRAINT [FK_Refund_Users_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[Refund] CHECK CONSTRAINT [FK_Refund_Users_CreatedBy]

GO

/****** Object:  Table [dbo].[RefundCheques]    Script Date: 5/5/2026 11:16:27 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[RefundCheques](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RefundID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[ChequeNo] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[ChequeDate] [date] NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[Bank] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Details] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[created_at] [datetime2](3) NOT NULL,
	[created_by] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[modified_by] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
 CONSTRAINT [PK_RefundCheques] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

SET ANSI_PADDING OFF
SET ANSI_PADDING ON

/****** Object:  Index [IX_RefundCheques_RefundID]    Script Date: 5/5/2026 11:16:27 AM ******/
CREATE NONCLUSTERED INDEX [IX_RefundCheques_RefundID] ON [dbo].[RefundCheques]
(
	[RefundID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
ALTER TABLE [dbo].[RefundCheques] ADD  CONSTRAINT [DF_RefundCheques_created_at]  DEFAULT (sysutcdatetime()) FOR [created_at]
ALTER TABLE [dbo].[RefundCheques]  WITH CHECK ADD  CONSTRAINT [FK_RefundCheques_Refund] FOREIGN KEY([RefundID])
REFERENCES [dbo].[Refund] ([RefundID])
ON DELETE CASCADE
ALTER TABLE [dbo].[RefundCheques] CHECK CONSTRAINT [FK_RefundCheques_Refund]

GO

/****** Object:  Table [dbo].[Refunds]    Script Date: 5/5/2026 11:16:27 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[Refunds](
	[RefundID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CustomerID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[RefundDate] [datetime] NOT NULL,
	[RefundReason] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[ApprovedBy] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Status] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Remarks] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[RefundID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

SET ANSI_PADDING OFF
ALTER TABLE [dbo].[Refunds] ADD  DEFAULT (getdate()) FOR [CreatedAt]
ALTER TABLE [dbo].[Refunds]  WITH NOCHECK ADD FOREIGN KEY([ApprovedBy])
REFERENCES [dbo].[Users] ([UserID])
ON DELETE SET NULL
ALTER TABLE [dbo].[Refunds]  WITH NOCHECK ADD FOREIGN KEY([CustomerID])
REFERENCES [dbo].[Customers] ([CustomerID])
ON DELETE CASCADE

GO

/****** Object:  Table [dbo].[Registration]    Script Date: 5/5/2026 11:16:27 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[Registration](
	[RegID] [nvarchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[FullName] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CNIC] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Phone] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Email] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreatedAt] [datetime] NULL,
	[Status] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[ProjectID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Size] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[PassportNo] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[SubProject] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[FormNo] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[RegID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

SET ANSI_PADDING OFF
SET ANSI_PADDING ON

/****** Object:  Index [IX_Registration_ProjectID]    Script Date: 5/5/2026 11:16:27 AM ******/
CREATE NONCLUSTERED INDEX [IX_Registration_ProjectID] ON [dbo].[Registration]
(
	[ProjectID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
ALTER TABLE [dbo].[Registration] ADD  DEFAULT (getdate()) FOR [CreatedAt]
ALTER TABLE [dbo].[Registration] ADD  DEFAULT ('Pending') FOR [Status]
ALTER TABLE [dbo].[Registration]  WITH CHECK ADD  CONSTRAINT [FK_Registration_Projects_ProjectID] FOREIGN KEY([ProjectID])
REFERENCES [dbo].[Projects] ([ProjectID])
ON DELETE SET NULL
ALTER TABLE [dbo].[Registration] CHECK CONSTRAINT [FK_Registration_Projects_ProjectID]

GO

/****** Object:  Table [dbo].[Rental]    Script Date: 5/5/2026 11:16:27 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[Rental](
	[RentalID] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[PropertyID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[TenantName] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[TenantCNIC] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[TenantPhone] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[TenantEmail] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[TenantAddress] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[MonthlyRent] [decimal](18, 2) NOT NULL,
	[SecurityDeposit] [decimal](18, 2) NULL,
	[AdvanceRent] [decimal](18, 2) NULL,
	[Currency] [nvarchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[StartDate] [date] NOT NULL,
	[EndDate] [date] NULL,
	[DurationMonths] [int] NOT NULL,
	[PaymentDueDayOfMonth] [int] NULL,
	[Status] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Notes] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NULL,
 CONSTRAINT [PK_Rental] PRIMARY KEY CLUSTERED 
(
	[RentalID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

SET ANSI_PADDING OFF
SET ANSI_PADDING ON

/****** Object:  Index [IX_Rental_PropertyID]    Script Date: 5/5/2026 11:16:27 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Rental_PropertyID] ON [dbo].[Rental]
(
	[PropertyID] ASC
)
WHERE ([Status]='Active')
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
SET ANSI_PADDING ON

/****** Object:  Index [IX_Rental_Status_StartDate]    Script Date: 5/5/2026 11:16:27 AM ******/
CREATE NONCLUSTERED INDEX [IX_Rental_Status_StartDate] ON [dbo].[Rental]
(
	[Status] ASC,
	[StartDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
ALTER TABLE [dbo].[Rental] ADD  DEFAULT (N'PKR') FOR [Currency]
ALTER TABLE [dbo].[Rental] ADD  DEFAULT (N'Active') FOR [Status]
ALTER TABLE [dbo].[Rental]  WITH CHECK ADD  CONSTRAINT [FK_Rental_Property_PropertyID] FOREIGN KEY([PropertyID])
REFERENCES [dbo].[Property] ([PropertyID])
ALTER TABLE [dbo].[Rental] CHECK CONSTRAINT [FK_Rental_Property_PropertyID]

GO

/****** Object:  Table [dbo].[RentalPayments]    Script Date: 5/5/2026 11:16:28 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[RentalPayments](
	[RentalPaymentID] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[RentalID] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[BillingYear] [int] NOT NULL,
	[BillingMonth] [int] NOT NULL,
	[DueDate] [date] NOT NULL,
	[AmountDue] [decimal](18, 2) NOT NULL,
	[AmountPaid] [decimal](18, 2) NOT NULL,
	[PaidOn] [datetime2](7) NULL,
	[PaymentMethod] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[ReferenceNo] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Status] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Remarks] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_RentalPayments] PRIMARY KEY CLUSTERED 
(
	[RentalPaymentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

/****** Object:  Index [IX_RentalPayments_BillingYear_BillingMonth]    Script Date: 5/5/2026 11:16:28 AM ******/
CREATE NONCLUSTERED INDEX [IX_RentalPayments_BillingYear_BillingMonth] ON [dbo].[RentalPayments]
(
	[BillingYear] ASC,
	[BillingMonth] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
SET ANSI_PADDING ON

/****** Object:  Index [IX_RentalPayments_RentalID]    Script Date: 5/5/2026 11:16:28 AM ******/
CREATE NONCLUSTERED INDEX [IX_RentalPayments_RentalID] ON [dbo].[RentalPayments]
(
	[RentalID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
SET ANSI_PADDING ON

/****** Object:  Index [IX_RentalPayments_Status_DueDate]    Script Date: 5/5/2026 11:16:28 AM ******/
CREATE NONCLUSTERED INDEX [IX_RentalPayments_Status_DueDate] ON [dbo].[RentalPayments]
(
	[Status] ASC,
	[DueDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
ALTER TABLE [dbo].[RentalPayments] ADD  DEFAULT (N'Pending') FOR [Status]
ALTER TABLE [dbo].[RentalPayments]  WITH CHECK ADD  CONSTRAINT [FK_RentalPayments_Rental_RentalID] FOREIGN KEY([RentalID])
REFERENCES [dbo].[Rental] ([RentalID])
ON DELETE CASCADE
ALTER TABLE [dbo].[RentalPayments] CHECK CONSTRAINT [FK_RentalPayments_Rental_RentalID]

GO

/****** Object:  Table [dbo].[Tickets]    Script Date: 5/5/2026 11:16:28 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[Tickets](
	[TicketID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CustomerID] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Email] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Contact] [nvarchar](256) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Title] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Description] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CROComments] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Status] [nvarchar](256) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreatedBy] [nvarchar](256) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[AssignedTo] [nvarchar](256) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[TicketClosingDate] [datetime] NULL,
	[CreatedAt] [datetime] NOT NULL,
 CONSTRAINT [PK_Tickets] PRIMARY KEY CLUSTERED 
(
	[TicketID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

SET ANSI_PADDING OFF
ALTER TABLE [dbo].[Tickets] ADD  DEFAULT (getdate()) FOR [CreatedAt]

GO

/****** Object:  Table [dbo].[Transfer]    Script Date: 5/5/2026 11:16:28 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[Transfer](
	[TransferID] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CustomerID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[WorkFlowStatus] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[SellerName] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[SellerFatherName] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[SellerCNIC] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[SellerContact] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[SellerAddress] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[BuyerName] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[BuyerFatherName] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[BuyerCNIC] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[BuyerContact] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[BuyerAddress] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[BuyerCity] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[BuyerCountry] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[BuyerAttachments] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[SellerAttachments] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[TransferFeeDue] [float] NULL,
	[TransferFeePaid] [float] NULL,
	[PaymentDate] [date] NULL,
	[PaymentMode] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[PaymentChallanNo] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Details] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CROComments] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[AccountsComments] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[TransferComments] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[BuyerBiometric] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[SellerBiometric] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[BuyerPassportNo] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[BuyerDOB] [date] NULL,
	[BuyerEmail] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[BuyerGender] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[BuyerMailingAddress] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[BuyerMobile] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[BuyerMobile2] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[BuyerNationality] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[BuyerPermanentAddress] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[BuyerPhone] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[PaymentMethod] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[BankName] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[PaymentDetails] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
 CONSTRAINT [PK_Transfer] PRIMARY KEY CLUSTERED 
(
	[TransferID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

SET ANSI_PADDING OFF
SET ANSI_PADDING ON

/****** Object:  Index [IX_Transfer_CustomerID]    Script Date: 5/5/2026 11:16:28 AM ******/
CREATE NONCLUSTERED INDEX [IX_Transfer_CustomerID] ON [dbo].[Transfer]
(
	[CustomerID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
SET ANSI_PADDING ON

/****** Object:  Index [IX_Transfer_WorkFlowStatus]    Script Date: 5/5/2026 11:16:28 AM ******/
CREATE NONCLUSTERED INDEX [IX_Transfer_WorkFlowStatus] ON [dbo].[Transfer]
(
	[WorkFlowStatus] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
ALTER TABLE [dbo].[Transfer]  WITH CHECK ADD  CONSTRAINT [FK_Transfer_Customers_CustomerID] FOREIGN KEY([CustomerID])
REFERENCES [dbo].[Customers] ([CustomerID])
ALTER TABLE [dbo].[Transfer] CHECK CONSTRAINT [FK_Transfer_Customers_CustomerID]

GO

/****** Object:  Table [dbo].[TransferFee]    Script Date: 5/5/2026 11:16:28 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[TransferFee](
	[Id] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[ProjectID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SubProject] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[TransferType] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[TransferPriority] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[AmountPerUnit] [decimal](18, 2) NOT NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[CreatedBy] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[ModifiedBy] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Details] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
 CONSTRAINT [PK_TransferFee] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

SET ANSI_PADDING OFF
SET ANSI_PADDING ON

/****** Object:  Index [IX_TransferFee_ProjectID]    Script Date: 5/5/2026 11:16:28 AM ******/
CREATE NONCLUSTERED INDEX [IX_TransferFee_ProjectID] ON [dbo].[TransferFee]
(
	[ProjectID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
ALTER TABLE [dbo].[TransferFee]  WITH CHECK ADD  CONSTRAINT [FK_TransferFee_Projects_ProjectID] FOREIGN KEY([ProjectID])
REFERENCES [dbo].[Projects] ([ProjectID])
ON DELETE CASCADE
ALTER TABLE [dbo].[TransferFee] CHECK CONSTRAINT [FK_TransferFee_Projects_ProjectID]

GO

/****** Object:  Table [dbo].[TransferJointOwners]    Script Date: 5/5/2026 11:16:29 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[TransferJointOwners](
	[Id] [nvarchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[TransferID] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CustomerID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[JointOwnerName] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CNIC] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Contact] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Address] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Percentage] [decimal](5, 2) NULL,
	[Created_at] [datetime] NOT NULL,
	[Created_by] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Modified_by] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Details] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[FatherName] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

SET ANSI_PADDING OFF
SET ANSI_PADDING ON

/****** Object:  Index [IX_TransferJointOwners_CustomerID]    Script Date: 5/5/2026 11:16:29 AM ******/
CREATE NONCLUSTERED INDEX [IX_TransferJointOwners_CustomerID] ON [dbo].[TransferJointOwners]
(
	[CustomerID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
SET ANSI_PADDING ON

/****** Object:  Index [IX_TransferJointOwners_TransferID]    Script Date: 5/5/2026 11:16:29 AM ******/
CREATE NONCLUSTERED INDEX [IX_TransferJointOwners_TransferID] ON [dbo].[TransferJointOwners]
(
	[TransferID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
ALTER TABLE [dbo].[TransferJointOwners] ADD  CONSTRAINT [DF_TransferJointOwners_CreatedAt]  DEFAULT (getdate()) FOR [Created_at]
ALTER TABLE [dbo].[TransferJointOwners]  WITH CHECK ADD  CONSTRAINT [FK_TransferJointOwners_Customers] FOREIGN KEY([CustomerID])
REFERENCES [dbo].[Customers] ([CustomerID])
ON DELETE CASCADE
ALTER TABLE [dbo].[TransferJointOwners] CHECK CONSTRAINT [FK_TransferJointOwners_Customers]
ALTER TABLE [dbo].[TransferJointOwners]  WITH CHECK ADD  CONSTRAINT [FK_TransferJointOwners_Transfer] FOREIGN KEY([TransferID])
REFERENCES [dbo].[Transfer] ([TransferID])
ON DELETE CASCADE
ALTER TABLE [dbo].[TransferJointOwners] CHECK CONSTRAINT [FK_TransferJointOwners_Transfer]

GO

/****** Object:  Table [dbo].[Transfers]    Script Date: 5/5/2026 11:16:29 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[Transfers](
	[TransferID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[PropertyID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[FromCustomerID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[ToCustomerID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[TransferDate] [datetime] NOT NULL,
	[TransferReason] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[ApprovedBy] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Status] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Remarks] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[TransferID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

SET ANSI_PADDING OFF
ALTER TABLE [dbo].[Transfers] ADD  DEFAULT (getdate()) FOR [CreatedAt]
ALTER TABLE [dbo].[Transfers]  WITH NOCHECK ADD FOREIGN KEY([ApprovedBy])
REFERENCES [dbo].[Users] ([UserID])
ON DELETE SET NULL
ALTER TABLE [dbo].[Transfers]  WITH NOCHECK ADD FOREIGN KEY([FromCustomerID])
REFERENCES [dbo].[Customers] ([CustomerID])
ALTER TABLE [dbo].[Transfers]  WITH NOCHECK ADD FOREIGN KEY([PropertyID])
REFERENCES [dbo].[Property] ([PropertyID])
ON DELETE CASCADE
ALTER TABLE [dbo].[Transfers]  WITH NOCHECK ADD FOREIGN KEY([ToCustomerID])
REFERENCES [dbo].[Customers] ([CustomerID])

GO

/****** Object:  Table [dbo].[UserModulePermission]    Script Date: 5/5/2026 11:16:29 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[UserModulePermission](
	[UserID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[ModuleKey] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Permission] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
 CONSTRAINT [PK_UserModulePermission] PRIMARY KEY CLUSTERED 
(
	[UserID] ASC,
	[ModuleKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

SET ANSI_PADDING OFF
ALTER TABLE [dbo].[UserModulePermission]  WITH CHECK ADD  CONSTRAINT [FK_UserModulePermission_Users_UserID] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
ON DELETE CASCADE
ALTER TABLE [dbo].[UserModulePermission] CHECK CONSTRAINT [FK_UserModulePermission_Users_UserID]

GO

/****** Object:  Table [dbo].[Users]    Script Date: 5/5/2026 11:16:29 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[Users](
	[UserID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[FullName] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Email] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[PasswordHash] [nvarchar](256) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[RoleID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[IsActive] [bit] NULL,
	[CreatedAt] [datetime] NULL,
	[Department] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Designation] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[UserType] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

SET ANSI_PADDING OFF
ALTER TABLE [dbo].[Users] ADD  DEFAULT ((1)) FOR [IsActive]
ALTER TABLE [dbo].[Users] ADD  DEFAULT (getdate()) FOR [CreatedAt]
ALTER TABLE [dbo].[Users]  WITH NOCHECK ADD FOREIGN KEY([RoleID])
REFERENCES [dbo].[ACL] ([RoleID])

GO

/****** Object:  Table [dbo].[UserSessions]    Script Date: 5/5/2026 11:16:29 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[UserSessions](
	[SessionID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[UserID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[LoginTime] [datetime] NULL,
	[LogoutTime] [datetime] NULL,
	[IPAddress] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[DeviceInfo] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[SessionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

SET ANSI_PADDING OFF
ALTER TABLE [dbo].[UserSessions] ADD  DEFAULT (getdate()) FOR [LoginTime]
ALTER TABLE [dbo].[UserSessions]  WITH NOCHECK ADD FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])

GO

/****** Object:  Table [dbo].[Waiver]    Script Date: 5/5/2026 11:16:30 AM ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[Waiver](
	[WaiverID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[WaiverType] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CustomerID] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Status] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[AccountHead] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[TotalAmount] [decimal](18, 2) NULL,
	[WaivedAmount] [decimal](18, 2) NULL,
	[WaivedPercentage] [decimal](18, 2) NULL,
	[Comments] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[ApprovedBy] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreatedBy] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[LastModifiedBy] [char](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreatedAt] [datetime] NULL,
	[ApprovedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[WaiverID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

SET ANSI_PADDING OFF
ALTER TABLE [dbo].[Waiver] ADD  DEFAULT ('Surcharge Waiver') FOR [WaiverType]
ALTER TABLE [dbo].[Waiver] ADD  DEFAULT ('Initiated') FOR [Status]
ALTER TABLE [dbo].[Waiver] ADD  DEFAULT ('Waived Off') FOR [AccountHead]
ALTER TABLE [dbo].[Waiver] ADD  DEFAULT (getdate()) FOR [CreatedAt]
ALTER TABLE [dbo].[Waiver]  WITH CHECK ADD FOREIGN KEY([ApprovedBy])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[Waiver]  WITH CHECK ADD FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[Waiver]  WITH CHECK ADD FOREIGN KEY([CustomerID])
REFERENCES [dbo].[Customers] ([CustomerID])
ALTER TABLE [dbo].[Waiver]  WITH CHECK ADD FOREIGN KEY([LastModifiedBy])
REFERENCES [dbo].[Users] ([UserID])

GO

-- ====================== DATA =====================

EXEC sp_msforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';
GO

INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20251106101633_RequireAllottmentTypeAndWorkFlowStatus_InAllotment', N'8.0.10');
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20251110184904_AddNationalityAndKinDocumentsToCustomer', N'8.0.10');
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20251111075310_AddCurrencySupportToPaymentPlan', N'8.0.10');
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260217080045_ReplaceTransferTableWithNewSchema', N'8.0.10');
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260217093140_AddAttachmentTypeToAttachmentsPatch', N'8.0.10');
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260217094750_AddRentalModule', N'8.0.10');
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260217094912_MakeRentalDatesDateOnly', N'8.0.10');
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260221082544_AddSizesToProject', N'8.0.10');
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260221083053_AddProjectAndSizeToRegistration', N'8.0.10');
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260221100000_AddFloorToProperty', N'8.0.10');
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260221120000_AddNDCColumns', N'8.0.10');
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260221220514_AddPropertyTypesToProject', N'8.0.0');
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260221224336_WidenAttachmentsRefID', N'8.0.10');
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260221225630_AddTransferBiometricColumns', N'8.0.10');
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260222081954_AddUserDesignationDepartmentUserType', N'8.0.10');
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260222095423_AddUserModulePermission', N'8.0.10');
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260223051442_AddPassportNoToRegistration', N'8.0.10');
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260223054955_AddBuyerPassportNoToTransfer', N'8.0.10');
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260223055802_AddPaymentAuditFields', N'8.0.10');
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260223061659_RefactorRefundTable', N'8.0.10');
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260223085141_AddIsDealerRegisteredAndDealerNameToCustomer', N'8.0.10');
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260418064755_AddCustomerMobileNoAndMobileNo2', N'8.0.10');
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260418074615_AddActivityLogDetails', N'8.0.10');
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260420043354_AddSubProjectsToProject', N'8.0.10');
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260420054313_AddSubProjectsToProject', N'8.0.10');
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260420073000_UpdatePaymentScheduleSurchargeRatePrecision', N'8.0.10');
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260421073850_AddSubProjectToRegistration', N'8.0.10');
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260421102126_AddSubProjectToProperty', N'8.0.10');
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260421120736_AddBuyerStep2FieldsToTransfer', N'8.0.10');
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260422065159_AddTransferFeeAndNdcTransferColumns', N'8.0.10');
GO

INSERT INTO [dbo].[ACL] ([RoleID], [RoleName], [Permissions]) VALUES ('ADMIN001  ', N'Admin', N'All');
INSERT INTO [dbo].[ACL] ([RoleID], [RoleName], [Permissions]) VALUES ('MANAGER01 ', N'Manager', N'Customers,Properties,Payments');
INSERT INTO [dbo].[ACL] ([RoleID], [RoleName], [Permissions]) VALUES ('ROLE001   ', N'Admin', N'All,Manage Users,View Reports,Edit,Delete,Create');
INSERT INTO [dbo].[ACL] ([RoleID], [RoleName], [Permissions]) VALUES ('ROLE002   ', N'Manager', N'View Reports,Edit,Create');
INSERT INTO [dbo].[ACL] ([RoleID], [RoleName], [Permissions]) VALUES ('ROLE003   ', N'Staff', N'View,Create');
INSERT INTO [dbo].[ACL] ([RoleID], [RoleName], [Permissions]) VALUES ('STAFF001  ', N'Staff', N'Customers,Properties');
GO

SET IDENTITY_INSERT [dbo].[ActivityLog] ON;
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1647, 'USER00002 ', N'Create Project', N'Project', '8465DCE064', '2026-05-04 12:01:00.4170000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1648, 'USER00002 ', N'Delete Project', N'Project', '8465DCE064', '2026-05-04 12:01:58.4070000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1649, 'USER00002 ', N'Create Project', N'Project', '05AFED226D', '2026-05-04 12:08:29.2330000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1650, 'USER00002 ', N'Update Project', N'Project', '05AFED226D', '2026-05-04 12:09:36.2830000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1651, 'USER00002 ', N'Create Registration', N'Registration', 'REG0000001', '2026-05-04 12:09:54.2830000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1652, 'USER00002 ', N'Create Payment Plan', N'PaymentPlan', '8BD2A7A52E', '2026-05-04 12:12:18.6470000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1653, 'USER00002 ', N'Create Dealer', N'Dealer', '1007      ', '2026-05-04 12:13:35.3300000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1654, 'USER00002 ', N'Customer Creation - Muhammad Abbas (CNIC: 42201-4377318-1)', N'Customer', 'ZKBK-00001', '2026-05-04 12:13:52.5730000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1655, 'USER00002 ', N'Upload CustomerPicture', N'Attachment', '55D95FC11F', '2026-05-04 12:13:58.1370000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1656, 'USER00002 ', N'Upload IDCard', N'Attachment', '369EF5DEFF', '2026-05-04 12:14:02.8430000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1657, 'USER00002 ', N'Bulk status update by Abbas: Pending -> Active at 2026-05-04 12:14:12. Comments: as', N'Customer', 'ZKBK-00001', '2026-05-04 12:14:12.4700000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1658, 'USER00002 ', N'Record Payment - Abbas', N'Payment', 'E6C6570D36', '2026-05-04 12:23:21.5070000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1659, 'USER00002 ', N'Record Payment - Abbas', N'Payment', '49AC0B0A73', '2026-05-04 12:23:39.4030000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1660, 'USER00002 ', N'Record Payment - Abbas', N'Payment', 'ABC299E11A', '2026-05-04 12:24:05.9500000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1661, 'USER00002 ', N'Payment ABC299E11A audit: Approved. Remarks: —', N'Payment', 'ABC299E11A', '2026-05-04 12:24:18.2200000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1662, 'USER00002 ', N'Payment 49AC0B0A73 audit: Approved. Remarks: —', N'Payment', '49AC0B0A73', '2026-05-04 12:24:22.6230000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1663, 'USER00002 ', N'Payment E6C6570D36 audit: Approved. Remarks: —', N'Payment', 'E6C6570D36', '2026-05-04 12:24:26.7370000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1664, 'USER00002 ', N'Record Payment - Abbas', N'Payment', '39FF0D3FAF', '2026-05-04 13:05:27.9330000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1665, 'USER00002 ', N'Record Payment - Abbas', N'Payment', '3AF027AA4A', '2026-05-04 13:05:44.8630000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1666, 'USER00002 ', N'Payment 3AF027AA4A audit: Approved. Remarks: asd', N'Payment', '3AF027AA4A', '2026-05-04 13:06:24.3530000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1667, 'USER00002 ', N'Payment 39FF0D3FAF audit: Approved. Remarks: asd', N'Payment', '39FF0D3FAF', '2026-05-04 13:06:29.4630000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1668, 'USER00002 ', N'Create Property', N'Property', '8E4D62EB2F', '2026-05-04 13:09:16.8830000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1669, 'USER00002 ', N'Allot Property', N'Allotment', 'EAAEF0CAC8', '2026-05-04 13:11:26.4970000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1670, 'USER00002 ', N'Create Registration', N'Registration', 'REG0000002', '2026-05-04 13:47:45.7200000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1671, 'USER00002 ', N'Customer Creation - Muhammad Abbas (CNIC: 84097-3548348-8)', N'Customer', 'ZKBK-00002', '2026-05-04 13:49:36.9770000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1672, 'USER00002 ', N'Upload CustomerPicture', N'Attachment', '0A91959474', '2026-05-04 13:49:43.2270000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1673, 'USER00002 ', N'Upload IDCard', N'Attachment', 'F1355F4A37', '2026-05-04 13:49:48.6330000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1674, 'USER00002 ', N'Customer Updation - Muhammad Abbas (CNIC: 84097-3548348-8)', N'Customer', 'ZKBK-00002', '2026-05-04 13:49:52.2200000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1675, 'USER00002 ', N'Customer Creation - Muhammad Abbas (CNIC: 42201-4377318-1)', N'Customer', 'ZKBK-00003', '2026-05-04 13:50:51.1200000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1676, 'USER00002 ', N'Upload CustomerPicture', N'Attachment', 'AD29DA3B3E', '2026-05-04 13:50:56.0100000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1677, 'USER00002 ', N'Upload IDCard', N'Attachment', 'E6679C3561', '2026-05-04 13:51:00.9030000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1678, 'USER00002 ', N'Customer Updation - Muhammad Abbas (CNIC: 42201-4377318-1)', N'Customer', 'ZKBK-00003', '2026-05-04 13:51:03.5070000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1679, 'USER00002 ', N'Create Transfer Fee: 05AFED226D / Residential / Normal Transfer / Normal / 500.00', N'TransferFee', 'CD3E3776AE', '2026-05-04 13:52:50.0700000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1680, 'USER00002 ', N'Create Transfer Fee: 05AFED226D / Residential / Urgent Transfer / Urgent / 1,000.00', N'TransferFee', '524A15CD26', '2026-05-04 13:53:10.4230000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1681, 'USER00002 ', N'Create Transfer Fee: 05AFED226D / Residential / Duplicate File Transfer / Normal / 5,000.00', N'TransferFee', '8664B009D5', '2026-05-04 13:53:21.5330000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1682, 'USER00002 ', N'Create NDC', N'NDC', 'A6960101D6', '2026-05-04 13:54:08.4230000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1683, 'USER00002 ', N'View NDC Details', N'NDC', 'A6960101D6', '2026-05-04 13:54:08.5500000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1684, 'USER00002 ', N'Edit NDC', N'NDC', 'A6960101D6', '2026-05-04 13:54:31.3570000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1685, 'USER00002 ', N'View NDC Details', N'NDC', 'A6960101D6', '2026-05-04 13:54:31.4030000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1686, 'USER00002 ', N'Print NDC', N'NDC', 'A6960101D6', '2026-05-04 13:54:33.8300000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1687, 'USER00002 ', N'Edit NDC', N'NDC', 'A6960101D6', '2026-05-04 13:56:50.8000000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1688, 'USER00002 ', N'View NDC Details', N'NDC', 'A6960101D6', '2026-05-04 13:56:50.8400000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1689, 'USER00002 ', N'Delete NDC', N'NDC', 'A6960101D6', '2026-05-04 13:57:01.2130000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1690, 'USER00002 ', N'Create NDC', N'NDC', 'C7AE323964', '2026-05-04 13:57:14.9100000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1691, 'USER00002 ', N'View NDC Details', N'NDC', 'C7AE323964', '2026-05-04 13:57:15.0370000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1692, 'USER00002 ', N'Delete NDC', N'NDC', 'C7AE323964', '2026-05-04 13:58:22.3670000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1693, 'USER00002 ', N'Create NDC', N'NDC', '2908655571', '2026-05-04 13:58:34.2470000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1694, 'USER00002 ', N'View NDC Details', N'NDC', '2908655571', '2026-05-04 13:58:34.3730000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1695, 'USER00002 ', N'Create Project', N'Project', '4B994075B2', '2026-05-04 16:08:44.4700000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1696, 'USER00002 ', N'Create Payment Plan', N'PaymentPlan', '3C1176238B', '2026-05-04 16:29:49.6630000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1697, 'USER00002 ', N'Installment updated (schedule 901DB7A97E)', N'PaymentSchedule', '901DB7A97E', '2026-05-04 16:31:18.8000000', N'{"eventType":"PaymentScheduleEdit","timestampUtc":"2026-05-04T23:31:18.7164846Z","actorUserId":"USER00002 ","actorUserName":"Abbas","planId":"3C1176238B","planName":"1Bed(800)","scheduleId":"901DB7A97E","installmentNo":12,"customersAssignedCount":0,"scheduleAmountPkr":{"from":85500.00,"to":585500},"scheduleAmountUsd":{"from":307.55,"to":2106.12},"planTotalPkr":{"from":10000000.00,"to":10000000.00},"planTotalUsd":{"from":35971.22,"to":35971.22},"projectedSchedulesTotalPkrAfterEdit":7689500.00,"planTotalAdjusted":false,"changeReason":null}');
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1698, 'USER00002 ', N'Installment updated (schedule 77F52F4D57)', N'PaymentSchedule', '77F52F4D57', '2026-05-04 16:31:35.3270000', N'{"eventType":"PaymentScheduleEdit","timestampUtc":"2026-05-04T23:31:35.3258555Z","actorUserId":"USER00002 ","actorUserName":"Abbas","planId":"3C1176238B","planName":"1Bed(800)","scheduleId":"77F52F4D57","installmentNo":24,"customersAssignedCount":0,"scheduleAmountPkr":{"from":85500.00,"to":585500},"scheduleAmountUsd":{"from":307.55,"to":2106.12},"planTotalPkr":{"from":10000000.00,"to":10000000.00},"planTotalUsd":{"from":35971.22,"to":35971.22},"projectedSchedulesTotalPkrAfterEdit":8189500.00,"planTotalAdjusted":false,"changeReason":null}');
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1699, 'USER00002 ', N'Installment updated (schedule 9FE46C79A2)', N'PaymentSchedule', '9FE46C79A2', '2026-05-04 16:31:52.3130000', N'{"eventType":"PaymentScheduleEdit","timestampUtc":"2026-05-04T23:31:52.3132122Z","actorUserId":"USER00002 ","actorUserName":"Abbas","planId":"3C1176238B","planName":"1Bed(800)","scheduleId":"9FE46C79A2","installmentNo":36,"customersAssignedCount":0,"scheduleAmountPkr":{"from":85500.00,"to":585500},"scheduleAmountUsd":{"from":307.55,"to":2106.12},"planTotalPkr":{"from":10000000.00,"to":10000000.00},"planTotalUsd":{"from":35971.22,"to":35971.22},"projectedSchedulesTotalPkrAfterEdit":8689500.00,"planTotalAdjusted":false,"changeReason":null}');
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1700, 'USER00002 ', N'Installment updated (schedule C37BBF9D6F)', N'PaymentSchedule', 'C37BBF9D6F', '2026-05-04 16:32:12.1100000', N'{"eventType":"PaymentScheduleEdit","timestampUtc":"2026-05-04T23:32:12.1098469Z","actorUserId":"USER00002 ","actorUserName":"Abbas","planId":"3C1176238B","planName":"1Bed(800)","scheduleId":"C37BBF9D6F","installmentNo":48,"customersAssignedCount":0,"scheduleAmountPkr":{"from":85500.00,"to":585500},"scheduleAmountUsd":{"from":307.55,"to":2106.12},"planTotalPkr":{"from":10000000.00,"to":10000000.00},"planTotalUsd":{"from":35971.22,"to":35971.22},"projectedSchedulesTotalPkrAfterEdit":9189500.00,"planTotalAdjusted":false,"changeReason":null}');
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1701, 'USER00002 ', N'Installment updated (schedule 9F41ECEACA)', N'PaymentSchedule', '9F41ECEACA', '2026-05-04 16:32:34.2400000', N'{"eventType":"PaymentScheduleEdit","timestampUtc":"2026-05-04T23:32:34.2401584Z","actorUserId":"USER00002 ","actorUserName":"Abbas","planId":"3C1176238B","planName":"1Bed(800)","scheduleId":"9F41ECEACA","installmentNo":49,"customersAssignedCount":0,"scheduleAmountPkr":{"from":85500.00,"to":585500},"scheduleAmountUsd":{"from":307.55,"to":2106.12},"planTotalPkr":{"from":10000000.00,"to":10000000.00},"planTotalUsd":{"from":35971.22,"to":35971.22},"projectedSchedulesTotalPkrAfterEdit":9689500.00,"planTotalAdjusted":false,"changeReason":null}');
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1702, 'USER00002 ', N'Installment updated (schedule 9F41ECEACA)', N'PaymentSchedule', '9F41ECEACA', '2026-05-04 16:33:03.8170000', N'{"eventType":"PaymentScheduleEdit","timestampUtc":"2026-05-04T23:33:03.8167168Z","actorUserId":"USER00002 ","actorUserName":"Abbas","planId":"3C1176238B","planName":"1Bed(800)","scheduleId":"9F41ECEACA","installmentNo":49,"customersAssignedCount":0,"scheduleAmountPkr":{"from":585500.00,"to":585500},"scheduleAmountUsd":{"from":2106.12,"to":2106.12},"planTotalPkr":{"from":10000000.00,"to":10000000.00},"planTotalUsd":{"from":35971.22,"to":35971.22},"projectedSchedulesTotalPkrAfterEdit":9689500.00,"planTotalAdjusted":false,"changeReason":null}');
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1703, 'USER00002 ', N'Installment updated (schedule CDCBBF28ED)', N'PaymentSchedule', 'CDCBBF28ED', '2026-05-04 16:34:40.1200000', N'{"eventType":"PaymentScheduleEdit","timestampUtc":"2026-05-04T23:34:40.1209475Z","actorUserId":"USER00002 ","actorUserName":"Abbas","planId":"3C1176238B","planName":"1Bed(800)","scheduleId":"CDCBBF28ED","installmentNo":30,"customersAssignedCount":0,"scheduleAmountPkr":{"from":85500.00,"to":396000},"scheduleAmountUsd":{"from":307.55,"to":1424.46},"planTotalPkr":{"from":10000000.00,"to":10000000.00},"planTotalUsd":{"from":35971.22,"to":35971.22},"projectedSchedulesTotalPkrAfterEdit":10000000.00,"planTotalAdjusted":false,"changeReason":null}');
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1704, 'USER00002 ', N'Installment updated (schedule A3FCFB8592)', N'PaymentSchedule', 'A3FCFB8592', '2026-05-04 16:41:23.2970000', N'{"eventType":"PaymentScheduleEdit","timestampUtc":"2026-05-04T23:41:23.2949947Z","actorUserId":"USER00002 ","actorUserName":"Abbas","planId":"3C1176238B","planName":"1Bed(800)","scheduleId":"A3FCFB8592","installmentNo":50,"customersAssignedCount":0,"scheduleAmountPkr":{"from":1000000.00,"to":0},"scheduleAmountUsd":{"from":3597.12,"to":0},"planTotalPkr":{"from":10000000.00,"to":10000000.00},"planTotalUsd":{"from":35971.22,"to":35971.22},"projectedSchedulesTotalPkrAfterEdit":9000000.00,"planTotalAdjusted":false,"changeReason":null}');
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1705, 'USER00002 ', N'Delete Payment Schedule', N'PaymentSchedule', 'A3FCFB8592', '2026-05-04 16:42:20.2270000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1706, 'USER00002 ', N'Installment updated (schedule 9F41ECEACA)', N'PaymentSchedule', '9F41ECEACA', '2026-05-04 16:42:36.7430000', N'{"eventType":"PaymentScheduleEdit","timestampUtc":"2026-05-04T23:42:36.7437567Z","actorUserId":"USER00002 ","actorUserName":"Abbas","planId":"3C1176238B","planName":"1Bed(800)","scheduleId":"9F41ECEACA","installmentNo":49,"customersAssignedCount":0,"scheduleAmountPkr":{"from":585500.00,"to":1585500},"scheduleAmountUsd":{"from":2106.12,"to":5703.24},"planTotalPkr":{"from":10000000.00,"to":10000000.00},"planTotalUsd":{"from":35971.22,"to":35971.22},"projectedSchedulesTotalPkrAfterEdit":10000000.00,"planTotalAdjusted":false,"changeReason":null}');
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1707, 'USER00002 ', N'Customer Creation - Imran Abbas (CNIC: 11111-1234567-7)', N'Customer', 'ZKBQ-Q0001', '2026-05-04 17:30:51.5400000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1708, 'USER00002 ', N'Upload CustomerPicture', N'Attachment', '3436329E20', '2026-05-04 17:31:08.1330000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1709, 'USER00002 ', N'Upload IDCard', N'Attachment', 'FB9E4F6D09', '2026-05-04 17:31:19.8800000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1710, 'USER00002 ', N'Upload Other', N'Attachment', '0A70B936B2', '2026-05-04 17:31:33.7400000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1711, 'USER00002 ', N'Customer Updation - Imran Abbas (CNIC: 11111-1234567-7)', N'Customer', 'ZKBQ-Q0001', '2026-05-04 17:31:45.4530000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1712, 'USER00002 ', N'Bulk status update by Abbas: Pending -> Active at 2026-05-04 17:32:07. Comments: okay', N'Customer', 'ZKBQ-Q0001', '2026-05-04 17:32:07.6800000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1713, 'USER00002 ', N'Create Transfer Fee: 4B994075B2 / Residential / Urgent Transfer / Urgent / 149.98', N'TransferFee', '0C59798220', '2026-05-04 17:37:51.6170000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1714, 'USER00002 ', N'Create NDC', N'NDC', 'B17D57795E', '2026-05-04 17:41:19.1430000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1715, 'USER00002 ', N'View NDC Details', N'NDC', 'B17D57795E', '2026-05-04 17:41:19.2600000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1716, 'USER00002 ', N'Print NDC', N'NDC', 'B17D57795E', '2026-05-04 17:41:27.2400000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1717, 'USER00002 ', N'Print NDC', N'NDC', 'B17D57795E', '2026-05-04 17:42:25.6630000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1718, 'USER00002 ', N'Print NDC', N'NDC', 'B17D57795E', '2026-05-04 17:42:26.2330000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1719, 'USER00002 ', N'Print NDC', N'NDC', 'B17D57795E', '2026-05-04 17:42:26.3530000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1720, 'USER00002 ', N'Print NDC', N'NDC', 'B17D57795E', '2026-05-04 17:42:26.4730000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1721, 'USER00002 ', N'Print NDC', N'NDC', 'B17D57795E', '2026-05-04 17:42:26.6130000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1722, 'USER00002 ', N'Print NDC', N'NDC', 'B17D57795E', '2026-05-04 17:42:26.7470000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1723, 'USER00002 ', N'Print NDC', N'NDC', 'B17D57795E', '2026-05-04 17:42:26.9000000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1724, 'USER00002 ', N'Print NDC', N'NDC', 'B17D57795E', '2026-05-04 17:42:27.0470000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1725, 'USER00002 ', N'Print NDC', N'NDC', 'B17D57795E', '2026-05-04 17:42:27.1800000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1726, 'USER00002 ', N'Print NDC', N'NDC', 'B17D57795E', '2026-05-04 17:42:27.3300000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1727, 'USER00002 ', N'Print NDC', N'NDC', 'B17D57795E', '2026-05-04 17:42:27.4330000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1728, 'USER00002 ', N'Print NDC', N'NDC', 'B17D57795E', '2026-05-04 17:42:27.5500000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1729, 'USER00002 ', N'Print NDC', N'NDC', 'B17D57795E', '2026-05-04 17:42:27.6930000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1730, 'USER00002 ', N'Print NDC', N'NDC', 'B17D57795E', '2026-05-04 17:42:27.8130000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1731, 'USER00002 ', N'Print NDC', N'NDC', 'B17D57795E', '2026-05-04 17:42:27.9830000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1732, 'USER00002 ', N'Duplicate file transfer DFT0000001 initiated for ZKBQ-Q0001.', N'DuplicateFileTransfer', 'DFT0000001', '2026-05-04 17:43:57.4530000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1733, 'USER00002 ', N'Duplicate file transfer DFT0000001 approved (no customer/payment changes).', N'DuplicateFileTransfer', 'DFT0000001', '2026-05-04 17:44:14.6830000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1734, 'USER00002 ', N'Update Project', N'Project', '05AFED226D', '2026-05-04 18:58:20.5930000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1735, 'USER00002 ', N'Update Project', N'Project', '05AFED226D', '2026-05-04 18:59:32.7630000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1736, 'USER00002 ', N'Import Properties - 19 imported', N'Property', 'Bulk      ', '2026-05-04 19:09:12.1870000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1737, 'USER00002 ', N'Create Project', N'Project', '6291909CEB', '2026-05-04 19:14:23.6330000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1738, 'USER00002 ', N'Customer Creation - Muhammad Abbas (CNIC: 11111-1111111-1)', N'Customer', 'ZKBK-A0001', '2026-05-04 19:33:15.3170000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1739, 'USER00002 ', N'Upload CustomerPicture', N'Attachment', '50C3D258BE', '2026-05-04 19:33:22.7100000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1740, 'USER00002 ', N'Upload IDCard', N'Attachment', '9F18779AF3', '2026-05-04 19:33:30.4230000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1741, 'USER00002 ', N'Customer Updation - Muhammad Abbas (CNIC: 11111-1111111-1)', N'Customer', 'ZKBK-A0001', '2026-05-04 19:33:35.0130000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1742, 'USER00002 ', N'Bulk status update by Abbas: Pending -> Active at 2026-05-04 19:33:46. Comments: as', N'Customer', 'ZKBK-A0001', '2026-05-04 19:33:46.5700000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1743, 'USER00002 ', N'Create Dealer', N'Dealer', '1008      ', '2026-05-04 19:39:02.9930000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1744, 'USER00002 ', N'Customer Creation - Muhammad Abbas (CNIC: 11111-1111111-1)', N'Customer', 'ZKBQ-A0001', '2026-05-04 19:40:31.3370000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1745, 'USER00002 ', N'Upload CustomerPicture', N'Attachment', '634A6B452E', '2026-05-04 19:40:39.0400000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1746, 'USER00002 ', N'Upload IDCard', N'Attachment', '44D8408C55', '2026-05-04 19:40:47.5830000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1747, 'USER00002 ', N'Create Project', N'Project', '1B09539CF3', '2026-05-04 19:42:09.6670000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1748, 'USER00002 ', N'Bulk status update by Abbas: Pending -> Active at 2026-05-04 19:45:01. Comments: hgh', N'Customer', 'ZKBQ-A0001', '2026-05-04 19:45:01.9070000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1749, 'USER00002 ', N'Update Property', N'Property', '115465D603', '2026-05-04 19:52:23.8330000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1750, 'USER00002 ', N'Customer Updation - Muhammad Abbas (CNIC: 11111-1111111-1)', N'Customer', 'ZKBQ-A0001', '2026-05-04 19:52:38.6400000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1751, 'USER00002 ', N'Customer Updation - Muhammad Abbas (CNIC: 42201-4377318-1)', N'Customer', 'ZKBK-00003', '2026-05-04 19:53:45.2830000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1752, 'USER00002 ', N'Customer Updation - Muhammad Abbas (CNIC: 42201-4377318-1)', N'Customer', 'ZKBK-00003', '2026-05-04 19:54:13.5330000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1753, 'USER00002 ', N'Assigned property 115465D603 to customer ZKBK-00003', N'Allotment', '115465D603', '2026-05-04 19:54:14.5470000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1754, 'USER00002 ', N'Customer Updation - Muhammad Abbas (CNIC: 84097-3548348-8)', N'Customer', 'ZKBK-00002', '2026-05-04 19:55:34.3300000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1755, 'USER00002 ', N'Allot Property', N'Allotment', '5317E6F26F', '2026-05-04 19:56:46.4200000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1756, 'USER00002 ', N'Joint owner added: test one', N'JointOwner', '755E83AAE4', '2026-05-04 20:01:13.8370000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1757, 'USER00002 ', N'Joint owner added: test two', N'JointOwner', 'D3E61F8D7C', '2026-05-04 20:01:51.2570000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1758, 'USER00002 ', N'Customer Updation - Muhammad Abbas (CNIC: 42201-4377318-1)', N'Customer', 'ZKBK-00003', '2026-05-04 20:01:57.6830000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1759, 'USER00002 ', N'Joint owner added: joint owner one', N'JointOwner', '44894D7D40', '2026-05-04 20:02:29.0630000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1760, 'USER00002 ', N'Joint owner added: joint owner two', N'JointOwner', '2B0769C2D5', '2026-05-04 20:02:55.7230000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1761, 'USER00002 ', N'Customer Updation - Muhammad Abbas (CNIC: 11111-1111111-1)', N'Customer', 'ZKBQ-A0001', '2026-05-04 20:03:00.6270000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1762, 'USER00002 ', N'Create Project', N'Project', 'B8A9BD6C14', '2026-05-04 20:16:15.5070000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1763, 'USER00002 ', N'Create Payment Plan', N'PaymentPlan', 'B192D88D71', '2026-05-04 20:19:47.6470000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1764, 'USER00002 ', N'Login', N'User', 'USER00002 ', '2026-05-04 20:21:33.7900000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1765, 'USER00002 ', N'Login', N'User', 'USER00002 ', '2026-05-04 20:22:16.8730000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1766, 'USER00002 ', N'Login', N'User', 'USER00002 ', '2026-05-04 20:24:16.6270000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1767, 'USER00002 ', N'Login', N'User', 'USER00002 ', '2026-05-04 20:24:16.8200000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1768, 'USER00002 ', N'Create Payment Plan', N'PaymentPlan', '97FD2C3291', '2026-05-04 20:29:58.1600000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1769, 'USER00002 ', N'Installment updated (schedule 0EB614CE14)', N'PaymentSchedule', '0EB614CE14', '2026-05-04 20:51:11.6070000', N'{"eventType":"PaymentScheduleEdit","timestampUtc":"2026-05-05T03:51:11.5529565Z","actorUserId":"USER00002 ","actorUserName":"Abbas","planId":"97FD2C3291","planName":"2A/1865.94","scheduleId":"0EB614CE14","installmentNo":12,"customersAssignedCount":0,"scheduleAmountPkr":{"from":100000.00,"to":600000},"scheduleAmountUsd":{"from":359.71,"to":2158.27},"planTotalPkr":{"from":10000000.00,"to":10000000.00},"planTotalUsd":{"from":35971.22,"to":35971.22},"projectedSchedulesTotalPkrAfterEdit":7400000.00,"planTotalAdjusted":false,"changeReason":null}');
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1770, 'USER00002 ', N'Installment updated (schedule 612D738E52)', N'PaymentSchedule', '612D738E52', '2026-05-04 20:51:32.6100000', N'{"eventType":"PaymentScheduleEdit","timestampUtc":"2026-05-05T03:51:32.6102832Z","actorUserId":"USER00002 ","actorUserName":"Abbas","planId":"97FD2C3291","planName":"2A/1865.94","scheduleId":"612D738E52","installmentNo":24,"customersAssignedCount":0,"scheduleAmountPkr":{"from":100000.00,"to":600000},"scheduleAmountUsd":{"from":359.71,"to":2158.27},"planTotalPkr":{"from":10000000.00,"to":10000000.00},"planTotalUsd":{"from":35971.22,"to":35971.22},"projectedSchedulesTotalPkrAfterEdit":7900000.00,"planTotalAdjusted":false,"changeReason":null}');
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1771, 'USER00002 ', N'Installment updated (schedule E36A32566A)', N'PaymentSchedule', 'E36A32566A', '2026-05-04 20:51:56.8930000', N'{"eventType":"PaymentScheduleEdit","timestampUtc":"2026-05-05T03:51:56.8925279Z","actorUserId":"USER00002 ","actorUserName":"Abbas","planId":"97FD2C3291","planName":"2A/1865.94","scheduleId":"E36A32566A","installmentNo":30,"customersAssignedCount":0,"scheduleAmountPkr":{"from":100000.00,"to":600000},"scheduleAmountUsd":{"from":359.71,"to":2158.27},"planTotalPkr":{"from":10000000.00,"to":10000000.00},"planTotalUsd":{"from":35971.22,"to":35971.22},"projectedSchedulesTotalPkrAfterEdit":8400000.00,"planTotalAdjusted":false,"changeReason":null}');
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1772, 'USER00002 ', N'Installment updated (schedule 3105E5042C)', N'PaymentSchedule', '3105E5042C', '2026-05-04 20:52:20.1330000', N'{"eventType":"PaymentScheduleEdit","timestampUtc":"2026-05-05T03:52:20.1345233Z","actorUserId":"USER00002 ","actorUserName":"Abbas","planId":"97FD2C3291","planName":"2A/1865.94","scheduleId":"3105E5042C","installmentNo":36,"customersAssignedCount":0,"scheduleAmountPkr":{"from":100000.00,"to":600000},"scheduleAmountUsd":{"from":359.71,"to":2158.27},"planTotalPkr":{"from":10000000.00,"to":10000000.00},"planTotalUsd":{"from":35971.22,"to":35971.22},"projectedSchedulesTotalPkrAfterEdit":8900000.00,"planTotalAdjusted":false,"changeReason":null}');
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1773, 'USER00002 ', N'Installment updated (schedule 03916149E4)', N'PaymentSchedule', '03916149E4', '2026-05-04 20:52:54.0230000', N'{"eventType":"PaymentScheduleEdit","timestampUtc":"2026-05-05T03:52:54.0233084Z","actorUserId":"USER00002 ","actorUserName":"Abbas","planId":"97FD2C3291","planName":"2A/1865.94","scheduleId":"03916149E4","installmentNo":48,"customersAssignedCount":0,"scheduleAmountPkr":{"from":100000.00,"to":600000},"scheduleAmountUsd":{"from":359.71,"to":2158.27},"planTotalPkr":{"from":10000000.00,"to":10000000.00},"planTotalUsd":{"from":35971.22,"to":35971.22},"projectedSchedulesTotalPkrAfterEdit":9400000.00,"planTotalAdjusted":false,"changeReason":null}');
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1774, 'USER00002 ', N'Installment updated (schedule F6AF35A43C)', N'PaymentSchedule', 'F6AF35A43C', '2026-05-04 20:53:13.6600000', N'{"eventType":"PaymentScheduleEdit","timestampUtc":"2026-05-05T03:53:13.6598955Z","actorUserId":"USER00002 ","actorUserName":"Abbas","planId":"97FD2C3291","planName":"2A/1865.94","scheduleId":"F6AF35A43C","installmentNo":49,"customersAssignedCount":0,"scheduleAmountPkr":{"from":100000.00,"to":700000},"scheduleAmountUsd":{"from":359.71,"to":2517.99},"planTotalPkr":{"from":10000000.00,"to":10000000.00},"planTotalUsd":{"from":35971.22,"to":35971.22},"projectedSchedulesTotalPkrAfterEdit":10000000.00,"planTotalAdjusted":false,"changeReason":null}');
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1775, 'USER00002 ', N'Customer Creation - AD (CNIC: 42201-4377318-9)', N'Customer', 'ZKBS-A0001', '2026-05-04 20:55:46.0170000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1776, 'USER00002 ', N'Upload CustomerPicture', N'Attachment', '01FD21D47C', '2026-05-04 20:55:53.7800000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1777, 'USER00002 ', N'Upload IDCard', N'Attachment', 'B819D76D72', '2026-05-04 20:56:01.8670000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1778, 'USER00002 ', N'Customer Updation - AD (CNIC: 42201-4377318-9)', N'Customer', 'ZKBS-A0001', '2026-05-04 20:56:18.8030000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1779, 'USER00002 ', N'Bulk status update by Abbas: Pending -> Active at 2026-05-04 20:56:32. Comments: gghgh', N'Customer', 'ZKBS-A0001', '2026-05-04 20:56:32.6530000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1780, 'USER00002 ', N'Update Property', N'Property', '81E9097C66', '2026-05-04 20:58:14.5570000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1781, 'USER00002 ', N'Customer Updation - AD (CNIC: 42201-4377318-9)', N'Customer', 'ZKBS-A0001', '2026-05-04 20:58:37.1330000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1782, 'USER00002 ', N'Assigned property 81E9097C66 to customer ZKBS-A0001', N'Allotment', '81E9097C66', '2026-05-04 20:58:37.2070000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1783, 'USER00002 ', N'Record Payment - Abbas', N'Payment', 'B3320AA421', '2026-05-04 21:04:07.8400000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1784, 'USER00002 ', N'Record Multiple Payments (17 installments) - Abbas', N'Payment', '6D61AA80D9', '2026-05-04 21:06:22.2970000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1785, 'USER00002 ', N'Record Multiple Payments (17 installments) - Abbas', N'Payment', '722ACEB5CC', '2026-05-04 21:06:22.3030000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1786, 'USER00002 ', N'Record Multiple Payments (17 installments) - Abbas', N'Payment', '865E5E9566', '2026-05-04 21:06:22.3070000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1787, 'USER00002 ', N'Record Multiple Payments (17 installments) - Abbas', N'Payment', 'A6C6220997', '2026-05-04 21:06:22.3100000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1788, 'USER00002 ', N'Record Multiple Payments (17 installments) - Abbas', N'Payment', 'BF01A3814F', '2026-05-04 21:06:22.3130000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1789, 'USER00002 ', N'Record Multiple Payments (17 installments) - Abbas', N'Payment', '3643D1982A', '2026-05-04 21:06:22.3130000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1790, 'USER00002 ', N'Record Multiple Payments (17 installments) - Abbas', N'Payment', '5AD2DEFB09', '2026-05-04 21:06:22.3170000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1791, 'USER00002 ', N'Record Multiple Payments (17 installments) - Abbas', N'Payment', 'E8409F3818', '2026-05-04 21:06:22.3200000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1792, 'USER00002 ', N'Record Multiple Payments (17 installments) - Abbas', N'Payment', '6C6400AF9F', '2026-05-04 21:06:22.3230000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1793, 'USER00002 ', N'Record Multiple Payments (17 installments) - Abbas', N'Payment', 'A8779C2A39', '2026-05-04 21:06:22.3270000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1794, 'USER00002 ', N'Record Multiple Payments (17 installments) - Abbas', N'Payment', '5893BDF240', '2026-05-04 21:06:22.3270000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1795, 'USER00002 ', N'Record Multiple Payments (17 installments) - Abbas', N'Payment', 'D771046F41', '2026-05-04 21:06:22.3300000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1796, 'USER00002 ', N'Record Multiple Payments (17 installments) - Abbas', N'Payment', '08F3E3D816', '2026-05-04 21:06:22.3330000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1797, 'USER00002 ', N'Record Multiple Payments (17 installments) - Abbas', N'Payment', 'FBF5A3A0BB', '2026-05-04 21:06:22.3370000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1798, 'USER00002 ', N'Record Multiple Payments (17 installments) - Abbas', N'Payment', '90E6A2E9E5', '2026-05-04 21:06:22.3400000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1799, 'USER00002 ', N'Record Multiple Payments (17 installments) - Abbas', N'Payment', '1A1B71C094', '2026-05-04 21:06:22.3430000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1800, 'USER00002 ', N'Record Multiple Payments (17 installments) - Abbas', N'Payment', '08D8DB6D7D', '2026-05-04 21:06:22.3470000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1801, 'USER00002 ', N'Payment B3320AA421 audit: Approved. Remarks: fgf', N'Payment', 'B3320AA421', '2026-05-04 21:06:38.8500000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1802, 'USER00002 ', N'Payment E8409F3818 audit: Approved. Remarks: ggvgv', N'Payment', 'E8409F3818', '2026-05-04 21:06:48.7370000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1803, 'USER00002 ', N'Payment FBF5A3A0BB audit: Approved. Remarks: ffggg', N'Payment', 'FBF5A3A0BB', '2026-05-04 21:06:55.7200000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1804, 'USER00002 ', N'Payment 5893BDF240 audit: Approved. Remarks: fgfghg', N'Payment', '5893BDF240', '2026-05-04 21:07:03.9130000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1805, 'USER00002 ', N'Payment 5AD2DEFB09 audit: Approved. Remarks: ghgfh', N'Payment', '5AD2DEFB09', '2026-05-04 21:07:11.1970000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1806, 'USER00002 ', N'Payment 6C6400AF9F audit: Approved. Remarks: ghgf', N'Payment', '6C6400AF9F', '2026-05-04 21:07:18.6370000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1807, 'USER00002 ', N'Create Transfer Fee: B8A9BD6C14 / Apartment / Urgent Transfer / Urgent / 150.00', N'TransferFee', 'E82E63AB4C', '2026-05-04 21:09:07.7670000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1808, 'USER00002 ', N'Create Transfer Fee: B8A9BD6C14 / Apartment / Normal Transfer / Normal / 119.99', N'TransferFee', '9F6785804D', '2026-05-04 21:10:08.6830000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1809, 'USER00002 ', N'Create NDC', N'NDC', '362892F843', '2026-05-04 21:12:49.7070000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1810, 'USER00002 ', N'View NDC Details', N'NDC', '362892F843', '2026-05-04 21:12:49.8400000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1811, 'USER00002 ', N'Edit NDC', N'NDC', '362892F843', '2026-05-04 21:13:03.8300000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1812, 'USER00002 ', N'View NDC Details', N'NDC', '362892F843', '2026-05-04 21:13:03.9100000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1813, 'USER00002 ', N'Added transfer joint owner hghfghgfh', N'TransferJointOwner', '4A5E3DE5B2', '2026-05-04 21:20:22.0270000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1814, 'USER00002 ', N'Transfer approved: replaced customer joint owners (deleted 0, added 1).', N'Transfer', 'TRF-202605', '2026-05-04 21:30:08.4370000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1815, 'USER00002 ', N'Create Transfer Fee: B8A9BD6C14 / Apartment / Duplicate File Transfer / Normal / 50,000.00', N'TransferFee', 'EC7F115FB6', '2026-05-04 21:41:05.3670000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1816, 'USER00002 ', N'Duplicate file transfer DFT0000002 initiated for ZKBS-A0001.', N'DuplicateFileTransfer', 'DFT0000002', '2026-05-04 21:41:33.7470000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1817, 'USER00002 ', N'Refund RFD0000001 initiated for customer ZKBS-A0001. Amount: PKR 4,000,050', N'Refund', 'RFD0000001', '2026-05-04 21:43:15.3770000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1818, 'USER00002 ', N'Refund RFD0000001 status changed from Initiated to Accounts Desk.', N'Refund', 'RFD0000001', '2026-05-04 21:44:26.3070000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1819, 'USER00002 ', N'Refund RFD0000001 status changed from Accounts Desk to Approved.', N'Refund', 'RFD0000001', '2026-05-04 21:44:31.2370000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1820, 'USER00002 ', N'Create Project', N'Project', 'E2CEF5B2E6', '2026-05-05 09:56:55.2970000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1821, 'USER00002 ', N'Customer Creation - asdf (CNIC: 11111-1111111-1)', N'Customer', 'ZKBS-A0002', '2026-05-05 10:05:32.9770000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1822, 'USER00002 ', N'Upload CustomerPicture', N'Attachment', '1CFA525699', '2026-05-05 10:06:10.7570000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1823, 'USER00002 ', N'Upload IDCard', N'Attachment', 'D4C176B1EB', '2026-05-05 10:06:19.0130000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1824, 'USER00002 ', N'Customer Updation - asdf (CNIC: 11111-1111111-1)', N'Customer', 'ZKBS-A0002', '2026-05-05 10:06:25.6030000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1825, 'USER00002 ', N'Bulk status update by Abbas: Pending -> Active at 2026-05-05 10:06:41. Comments: gf', N'Customer', 'ZKBS-A0002', '2026-05-05 10:06:41.2530000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1826, 'USER00002 ', N'Create Dealer', N'Dealer', '1009      ', '2026-05-05 10:13:13.0930000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1827, 'USER00002 ', N'Customer Updation - asdf (CNIC: 11111-1111111-1)', N'Customer', 'ZKBS-A0002', '2026-05-05 10:15:12.5900000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1828, 'USER00002 ', N'Customer Deletion - asdf (CNIC: 11111-1111111-1)', N'Customer', 'ZKBS-A0002', '2026-05-05 10:16:02.2100000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1829, 'USER00002 ', N'Customer Deletion - Muhammad Abbas (CNIC: 11111-1111111-1)', N'Customer', 'ZKBQ-A0001', '2026-05-05 10:16:09.2530000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1830, 'USER00002 ', N'Customer Deletion - Muhammad Abbas (CNIC: 42201-4377318-1)', N'Customer', 'ZKBK-00001', '2026-05-05 10:16:16.3930000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1831, 'USER00002 ', N'Customer Deletion - Muhammad Abbas (CNIC: 11111-1111111-1)', N'Customer', 'ZKBK-A0001', '2026-05-05 10:16:56.4930000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1832, 'USER00002 ', N'Customer Deletion - Muhammad Abbas (CNIC: 84097-3548348-8)', N'Customer', 'ZKBK-00002', '2026-05-05 10:17:05.5670000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1833, 'USER00002 ', N'Customer Deletion - Muhammad Abbas (CNIC: 42201-4377318-1)', N'Customer', 'ZKBK-00003', '2026-05-05 10:17:12.9670000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1834, 'USER00002 ', N'Delete Property', N'Property', '8E4D62EB2F', '2026-05-05 10:18:03.9400000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1835, 'USER00002 ', N'Delete Property', N'Property', '115465D603', '2026-05-05 10:18:10.5470000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1836, 'USER00002 ', N'Delete Dealer', N'Dealer', '1009      ', '2026-05-05 10:18:35.2400000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1837, 'USER00002 ', N'Delete Dealer', N'Dealer', '1007      ', '2026-05-05 10:18:40.8100000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1838, 'USER00002 ', N'Delete Dealer', N'Dealer', '1008      ', '2026-05-05 10:18:46.6130000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1839, 'USER00002 ', N'Delete Payment. CustomerID: ZKBS-A0001', N'Payment', 'B3320AA421', '2026-05-05 10:18:55.3630000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1840, 'USER00002 ', N'Delete Payment. CustomerID: ZKBS-A0001', N'Payment', 'A8779C2A39', '2026-05-05 10:19:00.5270000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1841, 'USER00002 ', N'Delete Payment. CustomerID: ZKBS-A0001', N'Payment', 'E8409F3818', '2026-05-05 10:19:04.1270000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1842, 'USER00002 ', N'Delete Payment. CustomerID: ZKBK-00001', N'Payment', 'ABC299E11A', '2026-05-05 10:19:06.8830000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1843, 'USER00002 ', N'Delete Payment. CustomerID: ZKBK-00001', N'Payment', '39FF0D3FAF', '2026-05-05 10:19:09.3930000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1844, 'USER00002 ', N'Delete Payment. CustomerID: ZKBK-00001', N'Payment', '49AC0B0A73', '2026-05-05 10:19:12.4070000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1845, 'USER00002 ', N'Delete Payment. CustomerID: ZKBK-00001', N'Payment', 'E6C6570D36', '2026-05-05 10:19:14.9700000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1846, 'USER00002 ', N'Delete Payment. CustomerID: ZKBK-00001', N'Payment', '3AF027AA4A', '2026-05-05 10:19:17.4170000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1847, 'USER00002 ', N'Delete Payment. CustomerID: ZKBS-A0001', N'Payment', '08D8DB6D7D', '2026-05-05 10:19:22.8600000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1848, 'USER00002 ', N'Delete Payment. CustomerID: ZKBS-A0001', N'Payment', '1A1B71C094', '2026-05-05 10:19:25.7330000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1849, 'USER00002 ', N'Delete Payment. CustomerID: ZKBS-A0001', N'Payment', '08F3E3D816', '2026-05-05 10:19:28.3930000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1850, 'USER00002 ', N'Delete Payment. CustomerID: ZKBS-A0001', N'Payment', '3643D1982A', '2026-05-05 10:19:31.1100000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1851, 'USER00002 ', N'Delete Payment. CustomerID: ZKBS-A0001', N'Payment', '5893BDF240', '2026-05-05 10:19:34.1470000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1852, 'USER00002 ', N'Delete Payment. CustomerID: ZKBS-A0001', N'Payment', '5AD2DEFB09', '2026-05-05 10:19:36.7070000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1853, 'USER00002 ', N'Delete Payment. CustomerID: ZKBS-A0001', N'Payment', '6C6400AF9F', '2026-05-05 10:19:39.9700000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1854, 'USER00002 ', N'Delete Payment. CustomerID: ZKBS-A0001', N'Payment', 'FBF5A3A0BB', '2026-05-05 10:19:44.6230000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1855, 'USER00002 ', N'Delete Payment. CustomerID: ZKBS-A0001', N'Payment', '865E5E9566', '2026-05-05 10:19:47.1700000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1856, 'USER00002 ', N'Delete Payment. CustomerID: ZKBS-A0001', N'Payment', '722ACEB5CC', '2026-05-05 10:19:50.0370000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1857, 'USER00002 ', N'Delete Payment. CustomerID: ZKBS-A0001', N'Payment', '6D61AA80D9', '2026-05-05 10:19:52.8870000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1858, 'USER00002 ', N'Delete Payment. CustomerID: ZKBS-A0001', N'Payment', 'A6C6220997', '2026-05-05 10:19:55.6470000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1859, 'USER00002 ', N'Delete Payment. CustomerID: ZKBS-A0001', N'Payment', '90E6A2E9E5', '2026-05-05 10:19:59.2600000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1860, 'USER00002 ', N'Delete Payment. CustomerID: ZKBS-A0001', N'Payment', 'BF01A3814F', '2026-05-05 10:20:02.0800000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1861, 'USER00002 ', N'Delete Payment. CustomerID: ZKBS-A0001', N'Payment', 'D771046F41', '2026-05-05 10:20:05.2370000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1863, 'USER00002 ', N'Delete Registration', N'Registration', 'REG0000002', '2026-05-05 10:20:43.5300000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1864, 'USER00002 ', N'Delete Registration', N'Registration', 'REG0000001', '2026-05-05 10:20:47.2700000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1865, 'USER00002 ', N'Delete Transfer Fee: B8A9BD6C14 / Apartment / Urgent Transfer / Urgent / 150.00', N'TransferFee', 'E82E63AB4C', '2026-05-05 10:20:59.1370000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1866, 'USER00002 ', N'Delete Transfer Fee: 05AFED226D / Residential / Duplicate File Transfer / Normal / 5,000.00', N'TransferFee', '8664B009D5', '2026-05-05 10:21:02.0130000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1867, 'USER00002 ', N'Delete Transfer Fee: 05AFED226D / Residential / Normal Transfer / Normal / 500.00', N'TransferFee', 'CD3E3776AE', '2026-05-05 10:21:04.4130000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1868, 'USER00002 ', N'Delete Transfer Fee: 05AFED226D / Residential / Urgent Transfer / Urgent / 1,000.00', N'TransferFee', '524A15CD26', '2026-05-05 10:21:07.2700000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1869, 'USER00002 ', N'Delete Transfer Fee: 4B994075B2 / Residential / Urgent Transfer / Urgent / 149.98', N'TransferFee', '0C59798220', '2026-05-05 10:21:09.8070000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1870, 'USER00002 ', N'Delete Transfer Fee: B8A9BD6C14 / Apartment / Normal Transfer / Normal / 119.99', N'TransferFee', '9F6785804D', '2026-05-05 10:21:12.2800000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1871, 'USER00002 ', N'Delete Transfer Fee: B8A9BD6C14 / Apartment / Duplicate File Transfer / Normal / 50,000.00', N'TransferFee', 'EC7F115FB6', '2026-05-05 10:21:14.9330000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1872, 'USER00002 ', N'Duplicate file transfer DFT0000002 deleted.', N'DuplicateFileTransfer', 'DFT0000002', '2026-05-05 10:21:47.3700000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1873, 'USER00002 ', N'Delete Project', N'Project', 'E2CEF5B2E6', '2026-05-05 10:22:22.6830000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1874, 'USER00002 ', N'Delete Project', N'Project', '05AFED226D', '2026-05-05 10:22:25.6270000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1875, 'USER00002 ', N'Delete Project', N'Project', '1B09539CF3', '2026-05-05 10:22:28.8830000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1876, 'USER00002 ', N'Delete Project', N'Project', '4B994075B2', '2026-05-05 10:22:31.7030000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1877, 'USER00002 ', N'Delete Project', N'Project', '6291909CEB', '2026-05-05 10:22:34.7300000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1878, 'USER00002 ', N'Delete Project', N'Project', 'B8A9BD6C14', '2026-05-05 10:22:37.8470000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1879, 'USER00002 ', N'Allotment 5317E6F26F cancelled. Property BF1834157A un-allotted from Customer ZKBK-00002. Reason: rr', N'Allotment', '5317E6F26F', '2026-05-05 10:23:38.8530000', NULL);
INSERT INTO [dbo].[ActivityLog] ([LogID], [UserID], [Action], [RefType], [RefID], [CreatedAt], [Details]) VALUES (1880, 'USER00002 ', N'Delete Property', N'Property', '81E9097C66', '2026-05-05 10:24:21.3800000', NULL);
SET IDENTITY_INSERT [dbo].[ActivityLog] OFF;
GO

INSERT INTO [dbo].[Allotment] ([AllotmentID], [PropertyID], [CustomerID], [AllottedBy], [AllotmentDate], [ApprovedBy], [AllottmentType], [WorkFlowStatus], [Comments], [AdditionalInfo]) VALUES ('1         ', '115465D603', 'ZKBK-00003', 'USER00002 ', '2026-05-04 19:54:14.4100000', NULL, N'Regular', N'Pending', NULL, NULL);
INSERT INTO [dbo].[Allotment] ([AllotmentID], [PropertyID], [CustomerID], [AllottedBy], [AllotmentDate], [ApprovedBy], [AllottmentType], [WorkFlowStatus], [Comments], [AdditionalInfo]) VALUES ('2         ', '81E9097C66', 'ZKBS-A0001', 'USER00002 ', '2026-05-04 20:58:37.1970000', NULL, N'Regular', N'Pending', NULL, NULL);
INSERT INTO [dbo].[Allotment] ([AllotmentID], [PropertyID], [CustomerID], [AllottedBy], [AllotmentDate], [ApprovedBy], [AllottmentType], [WorkFlowStatus], [Comments], [AdditionalInfo]) VALUES ('EAAEF0CAC8', '8E4D62EB2F', 'ZKBK-00001', 'USER00002 ', '2026-05-04 13:11:26.4130000', NULL, N'Direct', N'Pending', N'test', NULL);
GO

INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('01FD21D47C', N'Customer', N'ZKBS-A0001', N'Abrar Avenue.png', N'/uploads/customers/ZKBS-A0001/5037e3b3-a5d3-4fff-92e1-e0ef65476704.png', N'image/png', 1057071, 'USER00002 ', '2026-05-04 20:55:53.7570000', NULL, N'CustomerPicture');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('03EF6C581D', N'Payment', N'865E5E9566', N'Abrar Avenue.png', N'/uploads/payments/865E5E9566/ba0dd02b1c7a4caeb5314903cf67de75.png', N'image/png', 1057071, 'USER00002 ', '2026-05-04 21:06:22.0970000', N'fhfghfghgf', N'Proof');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('0A70B936B2', N'Customer', N'ZKBQ-Q0001', N'WhatsApp Image 2026-04-27 at 09.24.20.jpeg', N'/uploads/customers/ZKBQ-Q0001/9543ebc6-faa9-4501-b65d-c8ba039e6b1d.jpeg', N'image/jpeg', 135659, 'USER00002 ', '2026-05-04 17:31:33.7100000', N'ookkoo', N'Other');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('0A91959474', N'Customer', N'ZKBK-00002', N'basketball.png', N'/uploads/customers/ZKBK-00002/efd58298-53ac-4c54-bead-175268629fe8.png', N'image/png', 56706, 'USER00002 ', '2026-05-04 13:49:42.9670000', NULL, N'CustomerPicture');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('109DE36B06', N'Payment', N'5893BDF240', N'Abrar Avenue.png', N'/uploads/payments/5893BDF240/e94e1fa792b349eca2395d2ea94356ce.png', N'image/png', 1057071, 'USER00002 ', '2026-05-04 21:06:22.2630000', N'fhfghfghgf', N'Proof');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('1CFA525699', N'Customer', N'ZKBS-A0002', N'Abrar Avenue.png', N'/uploads/customers/ZKBS-A0002/c4f36e5a-e3ac-4df0-8071-f251222a49b3.png', N'image/png', 1057071, 'USER00002 ', '2026-05-05 10:06:10.1430000', NULL, N'CustomerPicture');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('24E58C3294', N'Payment', N'FBF5A3A0BB', N'Abrar Avenue.png', N'/uploads/payments/FBF5A3A0BB/94d534b7ffd340cdac3141141419e88d.png', N'image/png', 1057071, 'USER00002 ', '2026-05-04 21:06:22.2800000', N'fhfghfghgf', N'Proof');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('2B8205966C', N'Payment', N'E8409F3818', N'Abrar Avenue.png', N'/uploads/payments/E8409F3818/9d3a6a56dbf7402d83a46393c77266f8.png', N'image/png', 1057071, 'USER00002 ', '2026-05-04 21:06:22.1400000', N'fhfghfghgf', N'Proof');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('3436329E20', N'Customer', N'ZKBQ-Q0001', N'WhatsApp Image 2026-04-27 at 09.24.20.jpeg', N'/uploads/customers/ZKBQ-Q0001/9281e90d-2715-4919-96a4-c71886aa130e.jpeg', N'image/jpeg', 135659, 'USER00002 ', '2026-05-04 17:31:08.0770000', NULL, N'CustomerPicture');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('369EF5DEFF', N'Customer', N'ZKBK-00001', N'basketball.png', N'/uploads/customers/ZKBK-00001/68a04f87-9777-439c-acb3-f4dc5bd992c8.png', N'image/png', 56706, 'USER00002 ', '2026-05-04 12:14:02.8400000', NULL, N'IDCard');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('44D8408C55', N'Customer', N'ZKBQ-A0001', N'appicon.jpg', N'/uploads/customers/ZKBQ-A0001/33610188-d0a0-43ac-a2e9-28e4a75b9790.jpg', N'image/jpeg', 28139, 'USER00002 ', '2026-05-04 19:40:47.5630000', NULL, N'IDCard');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('50C3D258BE', N'Customer', N'ZKBK-A0001', N'app icon.png', N'/uploads/customers/ZKBK-A0001/7de49506-e222-4a8a-88ed-e4af7697aba7.png', N'image/png', 24845, 'USER00002 ', '2026-05-04 19:33:22.5870000', NULL, N'CustomerPicture');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('55D95FC11F', N'Customer', N'ZKBK-00001', N'basketball.png', N'/uploads/customers/ZKBK-00001/51bf7239-36b5-415a-a2bf-c16b7ae5eb76.png', N'image/png', 56706, 'USER00002 ', '2026-05-04 12:13:58.0930000', NULL, N'CustomerPicture');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('5FA1FCA0FC', N'Payment', N'1A1B71C094', N'Abrar Avenue.png', N'/uploads/payments/1A1B71C094/528481355d4342fe91f648f501015507.png', N'image/png', 1057071, 'USER00002 ', '2026-05-04 21:06:22.2870000', N'fhfghfghgf', N'Proof');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('634A6B452E', N'Customer', N'ZKBQ-A0001', N'app icon 1.jpg', N'/uploads/customers/ZKBQ-A0001/ff17a24f-815c-48ed-89bc-61123f3832c0.jpg', N'image/jpeg', 52445, 'USER00002 ', '2026-05-04 19:40:39.0000000', NULL, N'CustomerPicture');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('7861EA5FCD', N'Payment', N'B3320AA421', N'Abrar Avenue.png', N'/uploads/payments/B3320AA421/2b49d79fd5fa4eb4b6aa7948f5baacbc.png', N'image/png', 1057071, 'USER00002 ', '2026-05-04 21:04:07.7900000', N'ghghghghg', N'Proof');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('7F1131DA8D', N'DuplicateFileTransfer', N'DFT0000001', N'WhatsApp Image 2026-04-28 at 10.49.29 (2).jpeg', N'/uploads/duplicatefiletransfers/DFT0000001/b78a0b9a-29b4-4b3b-8b00-85f19d648514.jpeg', N'image/jpeg', 98027, 'USER00002 ', '2026-05-04 17:43:57.4530000', N'dsss', N'Other');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('8B34A0FAB9', N'Payment', N'5AD2DEFB09', N'Abrar Avenue.png', N'/uploads/payments/5AD2DEFB09/908d12f37aed4f83925c091b16579160.png', N'image/png', 1057071, 'USER00002 ', '2026-05-04 21:06:22.1370000', N'fhfghfghgf', N'Proof');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('8DBF732190', N'Payment', N'3643D1982A', N'Abrar Avenue.png', N'/uploads/payments/3643D1982A/2e11df72a6054d7bba90d6d5114bb9e3.png', N'image/png', 1057071, 'USER00002 ', '2026-05-04 21:06:22.1330000', N'fhfghfghgf', N'Proof');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('9AFA8DE3B5', N'Payment', N'722ACEB5CC', N'Abrar Avenue.png', N'/uploads/payments/722ACEB5CC/e3a2ff1fd5b847f5af0922b79f2875bf.png', N'image/png', 1057071, 'USER00002 ', '2026-05-04 21:06:22.0930000', N'fhfghfghgf', N'Proof');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('9F18779AF3', N'Customer', N'ZKBK-A0001', N'ss3.jpeg', N'/uploads/customers/ZKBK-A0001/8bcdd306-c53b-4b46-9717-2a0f7e15be3a.jpeg', N'image/jpeg', 68379, 'USER00002 ', '2026-05-04 19:33:30.4200000', NULL, N'IDCard');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('A3BB7A8020', N'Payment', N'08D8DB6D7D', N'Abrar Avenue.png', N'/uploads/payments/08D8DB6D7D/5380055d9f7c4aa9836ef84d78bc2b42.png', N'image/png', 1057071, 'USER00002 ', '2026-05-04 21:06:22.2930000', N'fhfghfghgf', N'Proof');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('AD29DA3B3E', N'Customer', N'ZKBK-00003', N'basketball.png', N'/uploads/customers/ZKBK-00003/0795b6f9-4202-44db-88be-06c1da2d515d.png', N'image/png', 56706, 'USER00002 ', '2026-05-04 13:50:56.0070000', NULL, N'CustomerPicture');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('B819D76D72', N'Customer', N'ZKBS-A0001', N'Abrar Avenue.png', N'/uploads/customers/ZKBS-A0001/d07e5752-2846-4a46-b078-0b458ea98dca.png', N'image/png', 1057071, 'USER00002 ', '2026-05-04 20:56:01.8630000', NULL, N'IDCard');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('B833D0553A', N'Payment', N'A8779C2A39', N'Abrar Avenue.png', N'/uploads/payments/A8779C2A39/118a757ed3e648c6af76423fcfd1a444.png', N'image/png', 1057071, 'USER00002 ', '2026-05-04 21:06:22.2600000', N'fhfghfghgf', N'Proof');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('B972D8B6A6', N'Payment', N'6C6400AF9F', N'Abrar Avenue.png', N'/uploads/payments/6C6400AF9F/72a74c9fc3f24e8a92ad033f2c1ba8ec.png', N'image/png', 1057071, 'USER00002 ', '2026-05-04 21:06:22.2500000', N'fhfghfghgf', N'Proof');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('BA7C40C68A', N'Payment', N'D771046F41', N'Abrar Avenue.png', N'/uploads/payments/D771046F41/7aee72add5604aa7b87a2acfc320d2e4.png', N'image/png', 1057071, 'USER00002 ', '2026-05-04 21:06:22.2700000', N'fhfghfghgf', N'Proof');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('C29B7B61CA', N'Payment', N'6D61AA80D9', N'Abrar Avenue.png', N'/uploads/payments/6D61AA80D9/2022f38dc68b4ed79eb4951108a885ea.png', N'image/png', 1057071, 'USER00002 ', '2026-05-04 21:06:22.0870000', N'fhfghfghgf', N'Proof');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('D4C176B1EB', N'Customer', N'ZKBS-A0002', N'Abrar Avenue.png', N'/uploads/customers/ZKBS-A0002/98ff66ee-18ee-4fbb-8394-8a161524a22b.png', N'image/png', 1057071, 'USER00002 ', '2026-05-05 10:06:19.0130000', NULL, N'IDCard');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('E6679C3561', N'Customer', N'ZKBK-00003', N'basketball.png', N'/uploads/customers/ZKBK-00003/8c33eab2-0b7e-4cdd-b2c2-eb07273418c3.png', N'image/png', 56706, 'USER00002 ', '2026-05-04 13:51:00.9000000', NULL, N'IDCard');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('EF007D1BBD', N'Payment', N'A6C6220997', N'Abrar Avenue.png', N'/uploads/payments/A6C6220997/684b14d582334e868f9b7a5138e4f0ba.png', N'image/png', 1057071, 'USER00002 ', '2026-05-04 21:06:22.1200000', N'fhfghfghgf', N'Proof');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('F090C4CB19', N'Payment', N'BF01A3814F', N'Abrar Avenue.png', N'/uploads/payments/BF01A3814F/7aea4fbb459a4383af83c459169ae150.png', N'image/png', 1057071, 'USER00002 ', '2026-05-04 21:06:22.1270000', N'fhfghfghgf', N'Proof');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('F1355F4A37', N'Customer', N'ZKBK-00002', N'basketball.png', N'/uploads/customers/ZKBK-00002/bebbc39e-92b2-4d5f-8745-d4f1062995ed.png', N'image/png', 56706, 'USER00002 ', '2026-05-04 13:49:48.6300000', NULL, N'IDCard');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('F2B5865242', N'Payment', N'90E6A2E9E5', N'Abrar Avenue.png', N'/uploads/payments/90E6A2E9E5/b8d1fdc655574b9f930aa85020cdb246.png', N'image/png', 1057071, 'USER00002 ', '2026-05-04 21:06:22.2830000', N'fhfghfghgf', N'Proof');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('FAC42E4FF1', N'Payment', N'08F3E3D816', N'Abrar Avenue.png', N'/uploads/payments/08F3E3D816/efd2c488b6d040699f4e9828f56e01d3.png', N'image/png', 1057071, 'USER00002 ', '2026-05-04 21:06:22.2730000', N'fhfghfghgf', N'Proof');
INSERT INTO [dbo].[Attachments] ([AttachmentID], [RefType], [RefID], [FileName], [FilePath], [FileType], [FileSize], [UploadedBy], [UploadedAt], [Description], [AttachmentType]) VALUES ('FB9E4F6D09', N'Customer', N'ZKBQ-Q0001', N'WhatsApp Image 2026-04-27 at 09.24.20 (1).jpeg', N'/uploads/customers/ZKBQ-Q0001/66d74860-cc98-4fef-92ab-61d5d03c6a36.jpeg', N'image/jpeg', 177191, 'USER00002 ', '2026-05-04 17:31:19.8770000', NULL, N'IDCard');
GO

INSERT INTO [dbo].[Configuration] ([ConfigKey], [Category], [ConfigValue], [Description], [CreatedAt], [UpdatedAt], [UpdatedBy]) VALUES (N'allotmenttypes', N'AllotmentTypes', N'Regular,Transfer,Balloting,Special', N'Types of property allotments', '2026-01-23 23:07:02.5400000', NULL, NULL);
INSERT INTO [dbo].[Configuration] ([ConfigKey], [Category], [ConfigValue], [Description], [CreatedAt], [UpdatedAt], [UpdatedBy]) VALUES (N'banks', NULL, N'Cash,HBL,UBL,ABL,MCB,Meezan', N'Bank options for payment screens', '2026-04-25 04:57:36.0000000', '2026-05-04 21:06:19.8100000', 'USER00002 ');
INSERT INTO [dbo].[Configuration] ([ConfigKey], [Category], [ConfigValue], [Description], [CreatedAt], [UpdatedAt], [UpdatedBy]) VALUES (N'blocks', N'Blocks', N'Block A,Block B,Block C,Block D,Block E,Block F,Block G,Block H', N'Available block identifiers', '2026-01-23 23:07:03.0000000', '2026-01-29 01:15:51.4670000', 'USER00002 ');
INSERT INTO [dbo].[Configuration] ([ConfigKey], [Category], [ConfigValue], [Description], [CreatedAt], [UpdatedAt], [UpdatedBy]) VALUES (N'cities', N'Cities', N'Karachi,Lahore,Islamabad,Faisalabad,Rawalpindi,Peshawar,Hyderabad,Multan', N'List of cities', '2026-01-23 23:07:04.0000000', '2026-01-29 01:15:14.5830000', 'USER00002 ');
INSERT INTO [dbo].[Configuration] ([ConfigKey], [Category], [ConfigValue], [Description], [CreatedAt], [UpdatedAt], [UpdatedBy]) VALUES (N'countries', N'Countries', N'Pakistan,UAE,Saudi Arabia,USA,UK,Oman', N'Iran', '2026-01-23 23:07:04.0000000', '2026-04-29 10:32:58.0000000', 'USER00002 ');
INSERT INTO [dbo].[Configuration] ([ConfigKey], [Category], [ConfigValue], [Description], [CreatedAt], [UpdatedAt], [UpdatedBy]) VALUES (N'Currency:USDToPKR', N'Currency', N'278', N'Exchange rate: 1 USD = 278 PKR. Used for plan and installment amount conversion.', '2026-02-21 01:12:52.6670000', NULL, NULL);
INSERT INTO [dbo].[Configuration] ([ConfigKey], [Category], [ConfigValue], [Description], [CreatedAt], [UpdatedAt], [UpdatedBy]) VALUES (N'customerstatus', NULL, N'Active,Inactive,Suspended,Cancelled,Refunded', N'Customer account statuses', '2026-01-23 23:07:04.0000000', '2026-02-22 21:43:02.5870000', 'USER00002 ');
INSERT INTO [dbo].[Configuration] ([ConfigKey], [Category], [ConfigValue], [Description], [CreatedAt], [UpdatedAt], [UpdatedBy]) VALUES (N'departments', NULL, N'Admin,IT,Sales,Accounts,Transfer,HR,Operations', N'User departments (comma-separated)', '2026-02-22 01:06:58.0000000', '2026-02-22 01:10:20.1830000', 'USER00002 ');
INSERT INTO [dbo].[Configuration] ([ConfigKey], [Category], [ConfigValue], [Description], [CreatedAt], [UpdatedAt], [UpdatedBy]) VALUES (N'designations', N'Users', N'System Administrator,Manager,CRO,Sales Officer,Accountant,HR Officer,Executive', N'User designations (comma-separated)', '2026-02-22 01:06:58.2770000', NULL, NULL);
INSERT INTO [dbo].[Configuration] ([ConfigKey], [Category], [ConfigValue], [Description], [CreatedAt], [UpdatedAt], [UpdatedBy]) VALUES (N'NDCExpiry', NULL, N'15', N'NDC validity in days from creation', '2026-02-21 13:12:13.0000000', '2026-04-21 22:05:28.6800000', 'USER00002 ');
INSERT INTO [dbo].[Configuration] ([ConfigKey], [Category], [ConfigValue], [Description], [CreatedAt], [UpdatedAt], [UpdatedBy]) VALUES (N'NDCStartNormal', NULL, N'1', N'Issued date = creation date + this many days', '2026-02-21 13:56:57.0000000', '2026-05-03 16:18:43.5570000', 'USER00002 ');
INSERT INTO [dbo].[Configuration] ([ConfigKey], [Category], [ConfigValue], [Description], [CreatedAt], [UpdatedAt], [UpdatedBy]) VALUES (N'NDCStartUrgent', NULL, N'0', N'Issued date = creation date + this many days when NDC Type contains Urgent', '2026-02-21 13:59:31.0000000', '2026-05-04 13:58:12.9330000', 'USER00002 ');
INSERT INTO [dbo].[Configuration] ([ConfigKey], [Category], [ConfigValue], [Description], [CreatedAt], [UpdatedAt], [UpdatedBy]) VALUES (N'NDCType', N'NDC', N'Normal Transfer,Urgent Transfer,Family Transfer,Death Transfer', N'NDC types (comma-separated)', '2026-02-21 13:12:13.8300000', NULL, NULL);
INSERT INTO [dbo].[Configuration] ([ConfigKey], [Category], [ConfigValue], [Description], [CreatedAt], [UpdatedAt], [UpdatedBy]) VALUES (N'NDCWorkFlowStatus', NULL, N'Initiated,Audit Desk,Approved,Declined', N'NDC workflow statuses (comma-separated)', '2026-02-21 13:12:13.0000000', '2026-04-21 05:44:24.5870000', 'USER00002 ');
INSERT INTO [dbo].[Configuration] ([ConfigKey], [Category], [ConfigValue], [Description], [CreatedAt], [UpdatedAt], [UpdatedBy]) VALUES (N'nomineerelations', N'NomineeRelations', N'Father,Mother,Son,Daughter,Spouse,Brother,Sister,Other', N'Relationship types for nominees', '2026-01-23 23:07:04.1730000', NULL, NULL);
INSERT INTO [dbo].[Configuration] ([ConfigKey], [Category], [ConfigValue], [Description], [CreatedAt], [UpdatedAt], [UpdatedBy]) VALUES (N'paymentmethods', N'PaymentMethods', N'Cash,Bank Transfer,Cheque,Online Payment,Mobile Money', N'Available payment methods', '2026-01-23 23:07:04.1800000', NULL, NULL);
INSERT INTO [dbo].[Configuration] ([ConfigKey], [Category], [ConfigValue], [Description], [CreatedAt], [UpdatedAt], [UpdatedBy]) VALUES (N'paymentstatus', N'PaymentStatus', N'Pending,Completed,Failed,Refunded,Cancelled', N'Payment transaction statuses', '2026-01-23 23:07:04.1900000', NULL, NULL);
INSERT INTO [dbo].[Configuration] ([ConfigKey], [Category], [ConfigValue], [Description], [CreatedAt], [UpdatedAt], [UpdatedBy]) VALUES (N'plottypes', NULL, N'General,Corner,Park Facing,Boulevard', N'Types of plots', '2026-01-23 23:07:04.0000000', '2026-02-17 04:06:47.3300000', 'USER00002 ');
INSERT INTO [dbo].[Configuration] ([ConfigKey], [Category], [ConfigValue], [Description], [CreatedAt], [UpdatedAt], [UpdatedBy]) VALUES (N'projecttypes', N'ProjectTypes', N'Residential Plot, Villa, Apartment, Commercial, Farm House, Shop,Food Court', N'Types of projects', '2026-01-23 23:07:04.0000000', '2026-04-30 22:04:19.6130000', 'USER00002 ');
INSERT INTO [dbo].[Configuration] ([ConfigKey], [Category], [ConfigValue], [Description], [CreatedAt], [UpdatedAt], [UpdatedBy]) VALUES (N'propertystatus', NULL, N'Available,Allotted,Reserved,Blocked,Rented,Undeveloped', N'Property availability statuses', '2026-01-23 23:07:04.0000000', '2026-02-21 11:52:04.0100000', 'USER00002 ');
INSERT INTO [dbo].[Configuration] ([ConfigKey], [Category], [ConfigValue], [Description], [CreatedAt], [UpdatedAt], [UpdatedBy]) VALUES (N'PropertyType', N'Property', N'General,Park Facing,West Open', NULL, '2026-05-04 12:43:05.2830000', '2026-05-04 12:43:05.2830000', 'USER00002 ');
INSERT INTO [dbo].[Configuration] ([ConfigKey], [Category], [ConfigValue], [Description], [CreatedAt], [UpdatedAt], [UpdatedBy]) VALUES (N'propertytypes', N'PropertyTypes', N'Apartment,Commercial Plot,Residential Plot,Shop', N'Types of properties', '2026-01-23 23:07:04.0000000', '2026-02-22 22:26:57.8770000', 'USER00002 ');
INSERT INTO [dbo].[Configuration] ([ConfigKey], [Category], [ConfigValue], [Description], [CreatedAt], [UpdatedAt], [UpdatedBy]) VALUES (N'refundworkflow', NULL, N'Initiated,Accounts Desk,Approved,Declined', N'Workflow statuses for the Refund module', '2026-02-22 22:18:20.0000000', '2026-04-21 23:01:46.8500000', 'USER00002 ');
INSERT INTO [dbo].[Configuration] ([ConfigKey], [Category], [ConfigValue], [Description], [CreatedAt], [UpdatedAt], [UpdatedBy]) VALUES (N'sizes', N'Sizes', N'5 Marla,7 Marla,10 Marla,1 Kanal,2 Kanal,250 sq meters,500 sq meters,1000 sq meters,1500 sq meters,2000 sq meters', N'Available property sizes', '2026-01-23 23:07:04.2770000', NULL, NULL);
INSERT INTO [dbo].[Configuration] ([ConfigKey], [Category], [ConfigValue], [Description], [CreatedAt], [UpdatedAt], [UpdatedBy]) VALUES (N'subprojects', N'SubProjects', N'Phase 1,Phase 2,Phase 3,Block A Extension,Block B Extension,Commercial Zone,Residential Zone', N'Sub-project identifiers', '2026-01-23 23:07:04.2900000', NULL, NULL);
INSERT INTO [dbo].[Configuration] ([ConfigKey], [Category], [ConfigValue], [Description], [CreatedAt], [UpdatedAt], [UpdatedBy]) VALUES (N'USDToPKR', N'General', N'278', NULL, '2026-02-20 08:15:28.4570000', '2026-02-20 08:15:28.4570000', 'USER00002 ');
INSERT INTO [dbo].[Configuration] ([ConfigKey], [Category], [ConfigValue], [Description], [CreatedAt], [UpdatedAt], [UpdatedBy]) VALUES (N'WaiverWorkFlow', N'Waiver', N'Initialted,Approved,Declined', N'Waiver workflow statuses (comma-separated)', '2026-04-23 04:37:48.4170000', NULL, NULL);
INSERT INTO [dbo].[Configuration] ([ConfigKey], [Category], [ConfigValue], [Description], [CreatedAt], [UpdatedAt], [UpdatedBy]) VALUES (N'workflowstatus', N'WorkflowStatus', N'Pending,Pending Approval,Approved,Rejected,Completed,Cancelled', N'Workflow approval statuses', '2026-01-23 23:07:04.3130000', NULL, NULL);
INSERT INTO [dbo].[Configuration] ([ConfigKey], [Category], [ConfigValue], [Description], [CreatedAt], [UpdatedAt], [UpdatedBy]) VALUES (N'years', N'Years', N'2020,2021,2022,2023,2024,2025,2026,2027,2028,2029,2030,2031,2032,2033,2034,2035', N'Available years for selection', '2026-01-23 23:07:04.3300000', NULL, NULL);
GO

INSERT INTO [dbo].[Customers] ([CustomerID], [RegID], [PlanID], [FullName], [FatherName], [CNIC], [PassportNo], [DOB], [Gender], [Phone], [Email], [MailingAddress], [PermanentAddress], [City], [Country], [SubProject], [RegisteredSize], [CreatedAt], [Status], [NomineeName], [NomineeID], [NomineeRelation], [AdditionalInfo], [DealerID], [ProjectID], [Nationality], [NomineeNICDocumentPath], [NomineePicturePath], [DealerName], [isDealerRegistered], [MobileNo], [MobileNo2], [FormNo]) VALUES ('ZKBQ-Q0001', NULL, '3C1176238B', N'Imran Abbas', N'M Ramzan', N'11111-1234567-7', NULL, '1990-02-02 00:00:00.0000000', N'Male', N'03001512142', N'imran@gmail.com', N'xxxxxxxx', N'xxxxxxxx', N'Karachi', N'Pakistan', N'Residential', N'1 Bed (800 Sq)', '2026-05-04 17:30:51.0000000', N'Active', N'R Madam', N'11111-1234567-0', N'Spouse', N'[2026-05-04 17:32:07] Status changed by Abbas (USER00002 ): Pending -> Active. Comments: okay', 1007, N'4B994075B2', N'Iran', N'/uploads/customers/ZKBQ-Q0001/kin/kin-nic_cee56476-4eab-437b-aa76-698256d45474.jpeg', N'/uploads/customers/ZKBQ-Q0001/kin/kin-picture_04939035-bf81-45a5-9c66-1f72fbed38b7.jpeg', NULL, 1, N'03001512140', N'03001512141', NULL);
INSERT INTO [dbo].[Customers] ([CustomerID], [RegID], [PlanID], [FullName], [FatherName], [CNIC], [PassportNo], [DOB], [Gender], [Phone], [Email], [MailingAddress], [PermanentAddress], [City], [Country], [SubProject], [RegisteredSize], [CreatedAt], [Status], [NomineeName], [NomineeID], [NomineeRelation], [AdditionalInfo], [DealerID], [ProjectID], [Nationality], [NomineeNICDocumentPath], [NomineePicturePath], [DealerName], [isDealerRegistered], [MobileNo], [MobileNo2], [FormNo]) VALUES ('ZKBS-A0001', NULL, '97FD2C3291', N'gfgfdg', N'M Ramzan', N'36103-7994589-7', N'asds45', '1990-01-01 00:00:00.0000000', N'Male', N'03001512142', N'imtiazabbas512@gmail.com', N'jgtyutmut,67  yu u u6euu u u  u', N'xxxxxxxx  ututyutyu yu tyuutyj ytuyi yy', N'Karachi', N'Pakistan', N'Apartment', N'1865.94', '2026-05-04 20:55:45.0000000', N'Refunded', N'Mrs Noreen', N'11111-1234567-0', N'Spouse', N'[2026-05-04 20:56:32] Status changed by Abbas (USER00002 ): Pending -> Active. Comments: gghgh', 1007, N'B8A9BD6C14', N'dfdfddntr', N'/uploads/customers/ZKBS-A0001/kin/kin-nic_543f13f3-7818-45ce-ad99-55eaeff8cd54.png', N'/uploads/customers/ZKBS-A0001/kin/kin-picture_37f86d2e-99ff-4922-822b-a81cca6c9514.png', NULL, 1, N'03001512140', N'03001512141', NULL);
GO

INSERT INTO [dbo].[DuplicateFileTransfer] ([Id], [CustomerID], [CustomerName], [CustomerCNIC], [Created_at], [Created_by], [Modified_by], [Status], [Comments], [FeeDue], [FeePaid], [ChallanID], [BankName], [InstrumentNo], [DepositDate], [PaymentMethod]) VALUES ('DFT0000001', 'ZKBQ-Q0001', N'Imran Abbas', N'11111-1234567-7', '2026-05-04 17:43:57.4070000', 'USER00002 ', 'USER00002 ', N'Approved', N'iuj
[Approve] sdsds', NULL, NULL, NULL, NULL, NULL, NULL, NULL);
GO

INSERT INTO [dbo].[JointOwner] ([Id], [CustomerID], [JointOwnerName], [CNIC], [Contact], [Address], [Percentage], [Created_at], [Created_by], [Modified_by], [Details], [FatherName]) VALUES (N'252E131F3C', 'ZKBS-A0001', N'hghfghgfh', N'11111-1111111-1', N'03001512142', N'trrtryry try', 50.00, '2026-05-04 21:30:08.4370000', 'USER00002 ', 'USER00002 ', N'rtrrbdyrtd', N'gh ghfchfg');
GO

INSERT INTO [dbo].[NDC] ([NDCID], [CustomerID], [NDCType], [Title], [WorkFlowStatus], [Comments], [IssuedDate], [Remarks], [CreatedAt], [CreatedBy], [NDCExpiryDate], [TotalDueAmount], [TotalDueInstallments], [AllPaymentClear], [AmountPerUnit], [PropertySize], [TransferFeeAmount], [RemainingDues]) VALUES ('362892F843', 'ZKBS-A0001', N'Urgent Transfer', N'Urgent Transfer', N'Approved', NULL, '2026-05-04 21:12:49.4909557', NULL, '2026-05-04 21:12:49.4909557', N'Abbas', '2026-05-19 00:00:00.0000000', 2100000.00, 4000050.00, NULL, 150.00, 1865.94, 279891.00, 0.00);
INSERT INTO [dbo].[NDC] ([NDCID], [CustomerID], [NDCType], [Title], [WorkFlowStatus], [Comments], [IssuedDate], [Remarks], [CreatedAt], [CreatedBy], [NDCExpiryDate], [TotalDueAmount], [TotalDueInstallments], [AllPaymentClear], [AmountPerUnit], [PropertySize], [TransferFeeAmount], [RemainingDues]) VALUES ('B17D57795E', 'ZKBQ-Q0001', N'Urgent Transfer', N'Urgent Transfer', N'Approved', N'ok', '2026-05-04 17:41:19.0694329', NULL, '2026-05-04 17:41:19.0694329', N'Abbas', '2026-05-19 00:00:00.0000000', 2000000.00, 0.00, 0, 149.98, 1.00, 149.98, 2000000.00);
GO

INSERT INTO [dbo].[PaymentPlan] ([PlanID], [ProjectID], [PlanName], [TotalAmount], [DurationMonths], [Frequency], [Description], [CreatedAt], [Currency], [ExchangeRate], [TotalAmountUSD], [RegisteredSize], [SubProject]) VALUES ('3C1176238B', '4B994075B2', N'1Bed(800)', 10000000.00, 49, N'Monthly', NULL, '2026-05-04 16:29:49.3370000', N'PKR', 278.0000, 35971.22, N'1 Bed (800 Sq)', N'Residential');
INSERT INTO [dbo].[PaymentPlan] ([PlanID], [ProjectID], [PlanName], [TotalAmount], [DurationMonths], [Frequency], [Description], [CreatedAt], [Currency], [ExchangeRate], [TotalAmountUSD], [RegisteredSize], [SubProject]) VALUES ('8BD2A7A52E', '05AFED226D', N'ZKBK - 1 Year Plan', 4999.00, 11, N'Monthly', NULL, '2026-05-04 12:12:18.4170000', N'PKR', 278.0000, 17.98, N'125 sq yd', N'Residential');
INSERT INTO [dbo].[PaymentPlan] ([PlanID], [ProjectID], [PlanName], [TotalAmount], [DurationMonths], [Frequency], [Description], [CreatedAt], [Currency], [ExchangeRate], [TotalAmountUSD], [RegisteredSize], [SubProject]) VALUES ('97FD2C3291', 'B8A9BD6C14', N'2A/1865.94', 10000000.00, 49, N'Monthly', NULL, '2026-05-04 20:29:58.0000000', N'PKR', 278.0000, 35971.22, N'1865.94', N'Apartment');
INSERT INTO [dbo].[PaymentPlan] ([PlanID], [ProjectID], [PlanName], [TotalAmount], [DurationMonths], [Frequency], [Description], [CreatedAt], [Currency], [ExchangeRate], [TotalAmountUSD], [RegisteredSize], [SubProject]) VALUES ('B192D88D71', 'B8A9BD6C14', N'2A/1865.94', 10000000.00, 49, N'Monthly', NULL, '2026-05-04 20:19:47.1730000', N'PKR', 278.0000, 35971.22, N'1865.94', N'Apartment');
GO

INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('018A3DE62A', '97FD2C3291', N'Monthly Installment', 34, '2029-02-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('03916149E4', '97FD2C3291', N'Yearly Bulk Installment', 48, '2030-04-04 00:00:00.0000000', 600000.00, NULL, 0.006700, NULL, 2158.27);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('0419B24BD3', '3C1176238B', N'Monthly Installment', 41, '2033-05-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('04926040BF', '97FD2C3291', N'Monthly Installment', 33, '2029-01-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('05241AF000', 'B192D88D71', N'Monthly Installment', 22, '2027-11-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('05A9A07269', '97FD2C3291', N'Monthly Installment', 15, '2027-07-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('07FAC7CAF4', 'B192D88D71', N'Monthly Installment', 11, '2026-12-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('0977B54067', 'B192D88D71', N'Monthly Installment', 24, '2028-01-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('0D6DF629BC', '97FD2C3291', N'Monthly Installment', 1, '2026-05-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('0DC9D5F6B6', '3C1176238B', N'Monthly Installment', 39, '2033-03-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('0EB614CE14', '97FD2C3291', N'Yearly Bulk Installment', 12, '2027-04-04 00:00:00.0000000', 600000.00, NULL, 0.006700, NULL, 2158.27);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('118F504DA2', '3C1176238B', N'Monthly Installment', 29, '2032-05-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('139514E0FC', '3C1176238B', N'Monthly Installment', 27, '2032-03-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('1440B7D0D2', '97FD2C3291', N'Monthly Installment', 25, '2028-05-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('15A60EFC77', '8BD2A7A52E', N'Land and Dev Charges', 11, '2027-03-03 00:00:00.0000000', 273.00, NULL, 0.050000, NULL, 0.98);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('178EA63008', 'B192D88D71', N'Monthly Installment', 7, '2026-08-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('18B88FD2C6', 'B192D88D71', N'Monthly Installment', 9, '2026-10-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('192D350BE3', '97FD2C3291', N'Monthly Installment', 39, '2029-07-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('1A393FAA22', '97FD2C3291', N'Monthly Installment', 4, '2026-08-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('1D0773D7DC', 'B192D88D71', N'Down Payment', 0, '2026-01-01 00:00:00.0000000', 2000000.00, 0, 0.000000, NULL, 7194.24);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('1D2BA5B595', 'B192D88D71', N'Monthly Installment', 47, '2029-12-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('1D59D7D8AE', '97FD2C3291', N'Monthly Installment', 35, '2029-03-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('2045E0AB21', 'B192D88D71', N'Monthly Installment', 13, '2027-02-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('207CF8E3FF', '8BD2A7A52E', N'Land and Dev Charges', 5, '2026-09-03 00:00:00.0000000', 273.00, NULL, 0.050000, NULL, 0.98);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('21545BE516', '3C1176238B', N'Monthly Installment', 25, '2032-01-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('2B34822FC4', '97FD2C3291', N'Monthly Installment', 7, '2026-11-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('2E3CFCAC8B', '97FD2C3291', N'Monthly Installment', 6, '2026-10-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('2E8C3DE5DC', 'B192D88D71', N'Monthly Installment', 33, '2028-10-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('2F16C39C8B', '3C1176238B', N'Monthly Installment', 14, '2031-02-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('3105E5042C', '97FD2C3291', N'Yearly Bulk  Installment', 36, '2029-04-04 00:00:00.0000000', 600000.00, NULL, 0.006700, NULL, 2158.27);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('3560964B51', 'B192D88D71', N'Monthly Installment', 20, '2027-09-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('3562179FA2', '3C1176238B', N'Monthly Installment', 2, '2030-02-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('3954752717', '3C1176238B', N'Monthly Installment', 13, '2031-01-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('39E13707A5', '3C1176238B', N'Monthly Installment', 16, '2031-04-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('3AE8DA03C9', 'B192D88D71', N'Monthly Installment', 45, '2029-10-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('3BFDF53420', '3C1176238B', N'Monthly Installment', 17, '2031-05-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('419326FB63', '3C1176238B', N'Monthly Installment', 19, '2031-07-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('4908FED4D3', 'B192D88D71', N'Monthly Installment', 43, '2029-08-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('493B12631B', '97FD2C3291', N'Monthly Installment', 13, '2027-05-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('4AF830A3E6', '8BD2A7A52E', N'Token', 0, '2026-05-01 00:00:00.0000000', 999.00, 0, 0.000000, NULL, 3.59);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('4C6D65968F', 'B192D88D71', N'Monthly Installment', 15, '2027-04-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('4C7CAAF0A0', 'B192D88D71', N'Monthly Installment', 4, '2026-05-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('4CF77FD5A0', 'B192D88D71', N'Monthly Installment', 36, '2029-01-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('4DC78013FB', '8BD2A7A52E', N'Land and Dev Charges', 6, '2026-10-03 00:00:00.0000000', 273.00, NULL, 0.050000, NULL, 0.98);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('4F2055025C', '97FD2C3291', N'Monthly Installment', 44, '2029-12-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('516BD93FF0', '3C1176238B', N'Monthly Installment', 28, '2032-04-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('5336C8F403', '3C1176238B', N'Down Payment', 0, '2026-01-01 00:00:00.0000000', 2000000.00, 0, 0.000000, NULL, 7194.24);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('53D8C84BC9', 'B192D88D71', N'Monthly Installment', 14, '2027-03-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('589315A426', 'B192D88D71', N'Monthly Installment', 17, '2027-06-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('5A5CFE0431', '97FD2C3291', N'Monthly Installment', 9, '2027-01-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('5BA23E99EE', '3C1176238B', N'Monthly Installment', 38, '2033-02-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('5F231A3AD9', '8BD2A7A52E', N'Land and Dev Charges', 10, '2027-02-03 00:00:00.0000000', 273.00, NULL, 0.050000, NULL, 0.98);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('602951DA1D', '3C1176238B', N'Monthly Installment', 4, '2030-04-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('612D738E52', '97FD2C3291', N'Yearly Bulk Installment', 24, '2028-04-04 00:00:00.0000000', 600000.00, NULL, 0.006700, NULL, 2158.27);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('6A95CBFA00', '97FD2C3291', N'Monthly Installment', 27, '2028-07-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('6AB9196DD9', '97FD2C3291', N'Down Payment', 0, '2026-05-04 00:00:00.0000000', 2000000.00, 0, 0.000000, NULL, 7194.24);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('6D504FC74B', '3C1176238B', N'Monthly Installment', 46, '2033-10-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('6FA73D2F43', '8BD2A7A52E', N'Land and Dev Charges', 3, '2026-07-03 00:00:00.0000000', 273.00, NULL, 0.050000, NULL, 0.98);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('6FDE244200', '3C1176238B', N'Monthly Installment', 22, '2031-10-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('733357AB62', '97FD2C3291', N'Monthly Installment', 40, '2029-08-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('73ACB70DE4', 'B192D88D71', N'Monthly Installment', 42, '2029-07-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('74CACF2A15', 'B192D88D71', N'Monthly Installment', 40, '2029-05-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('74D309FA61', 'B192D88D71', N'Monthly Installment', 2, '2026-03-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('7690F25252', '3C1176238B', N'Monthly Installment', 20, '2031-08-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('77CA558D64', '3C1176238B', N'Monthly Installment', 26, '2032-02-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('77F52F4D57', '3C1176238B', N'Yearly Bulk Installment', 24, '2031-12-01 00:00:00.0000000', 585500.00, NULL, 0.006700, NULL, 2106.12);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('7833244392', 'B192D88D71', N'Monthly Installment', 19, '2027-08-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('792A3FECA4', 'B192D88D71', N'Monthly Installment', 46, '2029-11-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('79CFEDA7B9', '3C1176238B', N'Monthly Installment', 32, '2032-08-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('7AD4222100', 'B192D88D71', N'Monthly Installment', 35, '2028-12-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('7B30CE46DC', '97FD2C3291', N'Monthly Installment', 10, '2027-02-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('7BDE8F9D25', 'B192D88D71', N'Monthly Installment', 5, '2026-06-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('7C14D02497', 'B192D88D71', N'Monthly Installment', 16, '2027-05-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('7C5474D026', '97FD2C3291', N'Monthly Installment', 42, '2029-10-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('7C9B173C10', '97FD2C3291', N'Monthly Installment', 17, '2027-09-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('7CDDEF0F21', '97FD2C3291', N'Monthly Installment', 26, '2028-06-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('801EDAD59C', 'B192D88D71', N'Monthly Installment', 10, '2026-11-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('8199478CCF', '97FD2C3291', N'Monthly Installment', 20, '2027-12-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('81E4E5735B', '97FD2C3291', N'Monthly Installment', 28, '2028-08-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('82B5F1C8B1', '97FD2C3291', N'Monthly Installment', 19, '2027-11-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('85E4292A01', '3C1176238B', N'Monthly Installment', 1, '2030-01-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('860BB389A4', '8BD2A7A52E', N'Land and Dev Charges', 1, '2026-05-03 00:00:00.0000000', 273.00, NULL, 0.050000, NULL, 0.98);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('878B98B7F7', 'B192D88D71', N'Monthly Installment', 25, '2028-02-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('8A725071BE', 'B192D88D71', N'Monthly Installment', 8, '2026-09-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('8D0D0449BE', 'B192D88D71', N'Monthly Installment', 3, '2026-04-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('8E0D4710A2', '3C1176238B', N'Monthly Installment', 18, '2031-06-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('8F2E114661', '3C1176238B', N'Monthly Installment', 37, '2033-01-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('8F690C9116', '97FD2C3291', N'Monthly Installment', 37, '2029-05-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('901DB7A97E', '3C1176238B', N'Yearly Bulk Installment', 12, '2030-12-01 00:00:00.0000000', 585500.00, NULL, 0.006700, NULL, 2106.12);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('905B69FCCB', '3C1176238B', N'Monthly Installment', 23, '2031-11-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('9114B70ADF', 'B192D88D71', N'Monthly Installment', 23, '2027-12-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('927C2CDDFC', '97FD2C3291', N'Monthly Installment', 29, '2028-09-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('990A0038F0', 'B192D88D71', N'Monthly Installment', 1, '2026-02-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('9912275242', '97FD2C3291', N'Monthly Installment', 32, '2028-12-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('999F177C81', '97FD2C3291', N'Monthly Installment', 5, '2026-09-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('9C258CC48A', '8BD2A7A52E', N'Land and Dev Charges', 4, '2026-08-03 00:00:00.0000000', 273.00, NULL, 0.050000, NULL, 0.98);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('9E19825F9C', '3C1176238B', N'Monthly Installment', 44, '2033-08-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('9E5CDD1866', '3C1176238B', N'Monthly Installment', 6, '2030-06-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('9EF317B1EA', '3C1176238B', N'Monthly Installment', 21, '2031-09-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('9F2C433F89', '97FD2C3291', N'Monthly Installment', 11, '2027-03-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('9F41ECEACA', '3C1176238B', N'On Possession', 49, '2034-01-01 00:00:00.0000000', 1585500.00, NULL, 0.006700, NULL, 5703.24);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('9FE46C79A2', '3C1176238B', N'Yearly Bulk Installment', 36, '2032-12-01 00:00:00.0000000', 585500.00, NULL, 0.006700, NULL, 2106.12);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('A039F7774F', 'B192D88D71', N'Monthly Installment', 12, '2027-01-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('A0618DA118', '97FD2C3291', N'Monthly Installment', 31, '2028-11-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('A0F663E171', '8BD2A7A52E', N'Land and Dev Charges', 9, '2027-01-03 00:00:00.0000000', 273.00, NULL, 0.050000, NULL, 0.98);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('A319923F68', '3C1176238B', N'Monthly Installment', 34, '2032-10-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('A591C6CE70', '97FD2C3291', N'Monthly Installment', 38, '2029-06-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('A65D3E0976', '3C1176238B', N'Monthly Installment', 8, '2030-08-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('AABAC2B94C', '3C1176238B', N'Monthly Installment', 45, '2033-09-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('AB07BAA824', '3C1176238B', N'Monthly Installment', 31, '2032-07-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('ABCF226FCE', '3C1176238B', N'Monthly Installment', 10, '2030-10-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('AE44E06442', '8BD2A7A52E', N'Possession Charges', 12, '2027-10-14 00:00:00.0000000', 997.00, 0, 0.000000, NULL, 3.59);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('AE46609214', 'B192D88D71', N'Monthly Installment', 18, '2027-07-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('AF14ED6EE9', '97FD2C3291', N'Monthly Installment', 41, '2029-09-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('B149D9A6C0', 'B192D88D71', N'Monthly Installment', 30, '2028-07-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('B1E6D8F83B', 'B192D88D71', N'Monthly Installment', 28, '2028-05-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('B25C6787A2', 'B192D88D71', N'Monthly Installment', 27, '2028-04-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('B26B8DF717', '97FD2C3291', N'Monthly Installment', 14, '2027-06-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('B53791BE75', '97FD2C3291', N'Monthly Installment', 46, '2030-02-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('B97A7F231C', 'B192D88D71', N'Monthly Installment', 31, '2028-08-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('BA5A66D6CD', '97FD2C3291', N'Monthly Installment', 21, '2028-01-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('BD7188515A', '97FD2C3291', N'Monthly Installment', 8, '2026-12-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('BDAC3D33A8', '3C1176238B', N'Monthly Installment', 7, '2030-07-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('C262E2CF2D', '97FD2C3291', N'Monthly Installment', 23, '2028-03-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('C2BDFC1EAA', 'B192D88D71', N'Monthly Installment', 41, '2029-06-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('C31DB33EBB', 'B192D88D71', N'Monthly Installment', 34, '2028-11-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('C34B555D94', 'B192D88D71', N'Monthly Installment', 48, '2030-01-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('C37BBF9D6F', '3C1176238B', N'Yearly Bulk Installment', 48, '2033-12-01 00:00:00.0000000', 585500.00, NULL, 0.006700, NULL, 2106.12);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('C84E8D24B1', '97FD2C3291', N'Monthly Installment', 16, '2027-08-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('CBB31B9F5F', '3C1176238B', N'Monthly Installment', 15, '2031-03-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('CBF6469BD3', 'B192D88D71', N'Monthly Installment', 26, '2028-03-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('CDCBBF28ED', '3C1176238B', N'On Structure Completion Installment', 30, '2032-06-01 00:00:00.0000000', 396000.00, NULL, 0.006700, NULL, 1424.46);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('D227037941', 'B192D88D71', N'Monthly Installment', 44, '2029-09-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('D30790A2CD', '3C1176238B', N'Monthly Installment', 11, '2030-11-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('D363AAF798', 'B192D88D71', N'Monthly Installment', 21, '2027-10-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('D3A523260C', '3C1176238B', N'Monthly Installment', 35, '2032-11-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('D4E4A6DBC6', '3C1176238B', N'Monthly Installment', 33, '2032-09-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('D7F55B4E1E', '8BD2A7A52E', N'Land and Dev Charges', 7, '2026-11-03 00:00:00.0000000', 273.00, NULL, 0.050000, NULL, 0.98);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('D8D0FFEE8B', '97FD2C3291', N'Monthly Installment', 18, '2027-10-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('DDD895079A', '3C1176238B', N'Monthly Installment', 5, '2030-05-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('DFD964A77F', 'B192D88D71', N'Monthly Installment', 49, '2030-02-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('E31761BB3A', 'B192D88D71', N'Monthly Installment', 38, '2029-03-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('E33EDC0666', '97FD2C3291', N'Monthly Installment', 43, '2029-11-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('E3631B9A89', '97FD2C3291', N'Monthly Installment', 22, '2028-02-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('E36A32566A', '97FD2C3291', N'Structure Completion Installment', 30, '2028-10-04 00:00:00.0000000', 600000.00, NULL, 0.006700, NULL, 2158.27);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('E57222B212', '97FD2C3291', N'Monthly Installment', 45, '2030-01-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('E70C5C721E', '97FD2C3291', N'Monthly Installment', 47, '2030-03-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('EA1B9E070D', '8BD2A7A52E', N'Land and Dev Charges', 2, '2026-06-03 00:00:00.0000000', 273.00, NULL, 0.050000, NULL, 0.98);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('EC39180A57', '3C1176238B', N'Monthly Installment', 3, '2030-03-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('ED682D8D93', '97FD2C3291', N'Monthly Installment', 2, '2026-06-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('EED5BADE0D', '3C1176238B', N'Monthly Installment', 47, '2033-11-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('EFBDF4CAD9', '3C1176238B', N'Monthly Installment', 43, '2033-07-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('EFC25A21F1', 'B192D88D71', N'Monthly Installment', 6, '2026-07-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('F00BBC872B', 'B192D88D71', N'Monthly Installment', 32, '2028-09-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('F1834B3076', 'B192D88D71', N'Monthly Installment', 29, '2028-06-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('F50C8A78C5', '97FD2C3291', N'Monthly Installment', 3, '2026-07-04 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('F6AF35A43C', '97FD2C3291', N'On Possession', 49, '2030-05-04 00:00:00.0000000', 700000.00, NULL, 0.006700, NULL, 2517.99);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('F78E622299', 'B192D88D71', N'Monthly Installment', 39, '2029-04-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('F951F814ED', '3C1176238B', N'Monthly Installment', 40, '2033-04-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('F9C2390AB8', '3C1176238B', N'Monthly Installment', 9, '2030-09-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('FB061AFDA5', 'B192D88D71', N'Monthly Installment', 37, '2029-02-01 00:00:00.0000000', 100000.00, NULL, 0.006700, NULL, 359.71);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('FC2B544F2A', '3C1176238B', N'Monthly Installment', 42, '2033-06-01 00:00:00.0000000', 85500.00, NULL, 0.006700, NULL, 307.55);
INSERT INTO [dbo].[PaymentSchedule] ([ScheduleID], [PlanID], [PaymentDescription], [InstallmentNo], [DueDate], [Amount], [SurchargeApplied], [SurchargeRate], [Description], [AmountUSD]) VALUES ('FE46B83FBD', '8BD2A7A52E', N'Land and Dev Charges', 8, '2026-12-03 00:00:00.0000000', 273.00, NULL, 0.050000, NULL, 0.98);
GO

INSERT INTO [dbo].[Property] ([PropertyID], [ProjectID], [PlotNo], [Street], [PlotType], [Block], [PropertyType], [Size], [Status], [CreatedAt], [AdditionalInfo], [DealerID], [Floor], [SubProject]) VALUES ('076697A5F1', '05AFED226D', N'1303', NULL, N'3B', N'Tower-01', NULL, N'2609.39', N'Available', '2026-05-04 19:09:12.0000000', NULL, 1007, N'13th', N'Apartment');
INSERT INTO [dbo].[Property] ([PropertyID], [ProjectID], [PlotNo], [Street], [PlotType], [Block], [PropertyType], [Size], [Status], [CreatedAt], [AdditionalInfo], [DealerID], [Floor], [SubProject]) VALUES ('14B7D5F0A7', '05AFED226D', N'1304', NULL, N'2A', N'Tower-01', NULL, N'1748.56', N'Available', '2026-05-04 19:09:12.0000000', NULL, 1007, N'13th', N'Apartment');
INSERT INTO [dbo].[Property] ([PropertyID], [ProjectID], [PlotNo], [Street], [PlotType], [Block], [PropertyType], [Size], [Status], [CreatedAt], [AdditionalInfo], [DealerID], [Floor], [SubProject]) VALUES ('327C8EC462', '05AFED226D', N'1314', NULL, N'2B', N'Tower-02', NULL, N'1554.5', N'Available', '2026-05-04 19:09:12.0070000', NULL, 1007, N'13th', N'Apartment');
INSERT INTO [dbo].[Property] ([PropertyID], [ProjectID], [PlotNo], [Street], [PlotType], [Block], [PropertyType], [Size], [Status], [CreatedAt], [AdditionalInfo], [DealerID], [Floor], [SubProject]) VALUES ('385338503B', '05AFED226D', N'1318', NULL, N'3D', N'Tower-02', NULL, N'4324.45', N'Available', '2026-05-04 19:09:12.0100000', NULL, 1007, N'13th', N'Apartment');
INSERT INTO [dbo].[Property] ([PropertyID], [ProjectID], [PlotNo], [Street], [PlotType], [Block], [PropertyType], [Size], [Status], [CreatedAt], [AdditionalInfo], [DealerID], [Floor], [SubProject]) VALUES ('49195D263A', '05AFED226D', N'1306', NULL, N'2A', N'Tower-01', NULL, N'1865.98', N'Available', '2026-05-04 19:09:12.0030000', NULL, 1007, N'13th', N'Apartment');
INSERT INTO [dbo].[Property] ([PropertyID], [ProjectID], [PlotNo], [Street], [PlotType], [Block], [PropertyType], [Size], [Status], [CreatedAt], [AdditionalInfo], [DealerID], [Floor], [SubProject]) VALUES ('506429C5F1', '05AFED226D', N'1307', NULL, N'2A', N'Tower-01', NULL, N'1747.63', N'Available', '2026-05-04 19:09:12.0030000', NULL, 1007, N'13th', N'Apartment');
INSERT INTO [dbo].[Property] ([PropertyID], [ProjectID], [PlotNo], [Street], [PlotType], [Block], [PropertyType], [Size], [Status], [CreatedAt], [AdditionalInfo], [DealerID], [Floor], [SubProject]) VALUES ('58163354E6', '05AFED226D', N'1309', NULL, N'3B', N'Tower-01', NULL, N'2581.7', N'Available', '2026-05-04 19:09:12.0030000', NULL, 1007, N'13th', N'Apartment');
INSERT INTO [dbo].[Property] ([PropertyID], [ProjectID], [PlotNo], [Street], [PlotType], [Block], [PropertyType], [Size], [Status], [CreatedAt], [AdditionalInfo], [DealerID], [Floor], [SubProject]) VALUES ('5AD148B11B', '05AFED226D', N'1302', NULL, N'3B', N'Tower-01', NULL, N'2609.47', N'Available', '2026-05-04 19:09:12.0000000', NULL, 1007, N'13th', N'Apartment');
INSERT INTO [dbo].[Property] ([PropertyID], [ProjectID], [PlotNo], [Street], [PlotType], [Block], [PropertyType], [Size], [Status], [CreatedAt], [AdditionalInfo], [DealerID], [Floor], [SubProject]) VALUES ('84D7DBA2FD', '05AFED226D', N'1311', NULL, N'2A', N'Tower-02', NULL, N'1922.15', N'Available', '2026-05-04 19:09:12.0070000', NULL, 1007, N'13th', N'Apartment');
INSERT INTO [dbo].[Property] ([PropertyID], [ProjectID], [PlotNo], [Street], [PlotType], [Block], [PropertyType], [Size], [Status], [CreatedAt], [AdditionalInfo], [DealerID], [Floor], [SubProject]) VALUES ('869848A502', '05AFED226D', N'1312', NULL, N'4A', N'Tower-02', NULL, N'3593.19', N'Available', '2026-05-04 19:09:12.0070000', NULL, 1007, N'13th', N'Apartment');
INSERT INTO [dbo].[Property] ([PropertyID], [ProjectID], [PlotNo], [Street], [PlotType], [Block], [PropertyType], [Size], [Status], [CreatedAt], [AdditionalInfo], [DealerID], [Floor], [SubProject]) VALUES ('98772A7AF7', '05AFED226D', N'1308', NULL, N'3B', N'Tower-01', NULL, N'2631.65', N'Available', '2026-05-04 19:09:12.0030000', NULL, 1007, N'13th', N'Apartment');
INSERT INTO [dbo].[Property] ([PropertyID], [ProjectID], [PlotNo], [Street], [PlotType], [Block], [PropertyType], [Size], [Status], [CreatedAt], [AdditionalInfo], [DealerID], [Floor], [SubProject]) VALUES ('ABA7047BCA', '05AFED226D', N'1316', NULL, N'2A', N'Tower-02', NULL, N'1985.15', N'Available', '2026-05-04 19:09:12.0100000', NULL, 1007, N'13th', N'Apartment');
INSERT INTO [dbo].[Property] ([PropertyID], [ProjectID], [PlotNo], [Street], [PlotType], [Block], [PropertyType], [Size], [Status], [CreatedAt], [AdditionalInfo], [DealerID], [Floor], [SubProject]) VALUES ('BF1834157A', '05AFED226D', N'1315', NULL, N'2C', N'Tower-02', NULL, N'2217.74', N'Available', '2026-05-04 19:09:12.0100000', NULL, 1007, N'13th', N'Apartment');
INSERT INTO [dbo].[Property] ([PropertyID], [ProjectID], [PlotNo], [Street], [PlotType], [Block], [PropertyType], [Size], [Status], [CreatedAt], [AdditionalInfo], [DealerID], [Floor], [SubProject]) VALUES ('C251A3A64F', '05AFED226D', N'1317', NULL, N'2A', N'Tower-02', NULL, N'2021.34', N'Available', '2026-05-04 19:09:12.0100000', NULL, 1007, N'13th', N'Apartment');
INSERT INTO [dbo].[Property] ([PropertyID], [ProjectID], [PlotNo], [Street], [PlotType], [Block], [PropertyType], [Size], [Status], [CreatedAt], [AdditionalInfo], [DealerID], [Floor], [SubProject]) VALUES ('C599ADC464', '05AFED226D', N'1310', NULL, N'2E', N'Tower-02', NULL, N'2676.8', N'Available', '2026-05-04 19:09:12.0070000', NULL, 1007, N'13th', N'Apartment');
INSERT INTO [dbo].[Property] ([PropertyID], [ProjectID], [PlotNo], [Street], [PlotType], [Block], [PropertyType], [Size], [Status], [CreatedAt], [AdditionalInfo], [DealerID], [Floor], [SubProject]) VALUES ('CA05D21291', '05AFED226D', N'1319', NULL, N'2E', N'Tower-02', NULL, N'2538.86', N'Available', '2026-05-04 19:09:12.0100000', NULL, 1007, N'13th', N'Apartment');
INSERT INTO [dbo].[Property] ([PropertyID], [ProjectID], [PlotNo], [Street], [PlotType], [Block], [PropertyType], [Size], [Status], [CreatedAt], [AdditionalInfo], [DealerID], [Floor], [SubProject]) VALUES ('F32749C90E', '05AFED226D', N'1313', NULL, N'2B', N'Tower-02', NULL, N'1483.85', N'Available', '2026-05-04 19:09:12.0070000', NULL, 1007, N'13th', N'Apartment');
GO

INSERT INTO [dbo].[Refund] ([RefundID], [CustomerID], [RefundType], [PaidAmount], [DeductionAmount], [RefundedAmount], [Reason], [WorkflowStatus], [SelectedPaymentIDs], [CreatedBy], [CreatedAt], [ApprovedBy], [ApprovedAt], [Notes]) VALUES ('RFD0000001', 'ZKBS-A0001', N'Partial', 4000050.00, 1000000.00, 3000050.00, N'ghghgf', N'Approved', N'["08D8DB6D7D","08F3E3D816","1A1B71C094","3643D1982A","5893BDF240","5AD2DEFB09","6C6400AF9F","6D61AA80D9","722ACEB5CC","865E5E9566","90E6A2E9E5","A6C6220997","A8779C2A39","BF01A3814F","D771046F41","E8409F3818","FBF5A3A0BB","B3320AA421"]', 'USER00002 ', '2026-05-04 21:43:15.3544071', 'USER00002 ', '2026-05-04 21:44:31.2374332', N'gfgdfgf');
GO

SET IDENTITY_INSERT [dbo].[RefundCheques] ON;
INSERT INTO [dbo].[RefundCheques] ([Id], [RefundID], [ChequeNo], [ChequeDate], [Amount], [Bank], [Details], [created_at], [created_by], [modified_by]) VALUES (1, 'RFD0000001', N'45354345', '2022-02-12 00:00:00.0000000', 545545.00, N'rnrtytryrtnytry', NULL, '2026-05-04 21:43:52.4310000', 'USER00002 ', NULL);
SET IDENTITY_INSERT [dbo].[RefundCheques] OFF;
GO

INSERT INTO [dbo].[Transfer] ([TransferID], [CustomerID], [WorkFlowStatus], [CreatedAt], [SellerName], [SellerFatherName], [SellerCNIC], [SellerContact], [SellerAddress], [BuyerName], [BuyerFatherName], [BuyerCNIC], [BuyerContact], [BuyerAddress], [BuyerCity], [BuyerCountry], [BuyerAttachments], [SellerAttachments], [TransferFeeDue], [TransferFeePaid], [PaymentDate], [PaymentMode], [PaymentChallanNo], [Details], [CROComments], [AccountsComments], [TransferComments], [BuyerBiometric], [SellerBiometric], [BuyerPassportNo], [BuyerDOB], [BuyerEmail], [BuyerGender], [BuyerMailingAddress], [BuyerMobile], [BuyerMobile2], [BuyerNationality], [BuyerPermanentAddress], [BuyerPhone], [PaymentMethod], [BankName], [PaymentDetails]) VALUES (N'TRF-20260504-0001', 'ZKBS-A0001', N'Initiated', '2026-05-04 21:19:12.6872792', N'AD', N'DA', N'42201-4377318-9', N'03001512142', N'Bahria Town Karachi rtrt ert r  rrr rtrt er', N'gfgfdg', N'M Ramzan', N'36103-7994589-7', N'03001512142', N'jgtyutmut,67  yu u u6euu u u  u', N'Karachi', N'Pakistan', N'[]', N'[]', 279891, 279891, '2026-05-04 00:00:00.0000000', N'Cash', N'fdfdfdfd', NULL, NULL, NULL, NULL, NULL, NULL, N'asds45', '1990-01-01 00:00:00.0000000', N'imtiazabbas512@gmail.com', N'Male', N'jgtyutmut,67  yu u u6euu u u  u', N'03001512140', N'03001512141', N'dfdfddntr', N'xxxxxxxx  ututyutyu yu tyuutyj ytuyi yy', N'03001512142', NULL, N'Cash', N'gfgfgf');
GO

INSERT INTO [dbo].[TransferJointOwners] ([Id], [TransferID], [CustomerID], [JointOwnerName], [CNIC], [Contact], [Address], [Percentage], [Created_at], [Created_by], [Modified_by], [Details], [FatherName]) VALUES (N'4A5E3DE5B2', N'TRF-20260504-0001', 'ZKBS-A0001', N'hghfghgfh', N'11111-1111111-1', N'03001512142', N'trrtryry try', 50.00, '2026-05-04 21:20:22.0000000', 'USER00002 ', 'USER00002 ', N'rtrrbdyrtd', N'gh ghfchfg');
GO

INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('17BDD8B96F', N'Account', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('17BDD8B96F', N'AccountsManagement', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('17BDD8B96F', N'ActivityLog', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('17BDD8B96F', N'Allotment', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('17BDD8B96F', N'Customer', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('17BDD8B96F', N'Dealer', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('17BDD8B96F', N'DuplicateFileTransfer', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('17BDD8B96F', N'Home', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('17BDD8B96F', N'InquiryApi', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('17BDD8B96F', N'NDC', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('17BDD8B96F', N'Payment', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('17BDD8B96F', N'PaymentAudit', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('17BDD8B96F', N'Project', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('17BDD8B96F', N'Property', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('17BDD8B96F', N'Refund', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('17BDD8B96F', N'Registration', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('17BDD8B96F', N'Rental', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('17BDD8B96F', N'Reports', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('17BDD8B96F', N'SalesInquiry', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('17BDD8B96F', N'Settings', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('17BDD8B96F', N'TesSQL', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('17BDD8B96F', N'Ticket', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('17BDD8B96F', N'Transfer', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('17BDD8B96F', N'TransferFee', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('17BDD8B96F', N'Waiver', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('873CC65AE1', N'Account', N'NoAccess');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('873CC65AE1', N'AccountsManagement', N'NoAccess');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('873CC65AE1', N'ActivityLog', N'NoAccess');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('873CC65AE1', N'Allotment', N'NoAccess');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('873CC65AE1', N'Customer', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('873CC65AE1', N'Dealer', N'NoAccess');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('873CC65AE1', N'Home', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('873CC65AE1', N'InquiryApi', N'NoAccess');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('873CC65AE1', N'NDC', N'NoAccess');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('873CC65AE1', N'Payment', N'NoAccess');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('873CC65AE1', N'Project', N'NoAccess');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('873CC65AE1', N'Property', N'NoAccess');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('873CC65AE1', N'Registration', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('873CC65AE1', N'Rental', N'NoAccess');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('873CC65AE1', N'Reports', N'NoAccess');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('873CC65AE1', N'SalesInquiry', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('873CC65AE1', N'Settings', N'NoAccess');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('873CC65AE1', N'TesSQL', N'NoAccess');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('873CC65AE1', N'Ticket', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('873CC65AE1', N'Transfer', N'NoAccess');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('8E96A44242', N'Account', N'NoAccess');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('8E96A44242', N'AccountsManagement', N'NoAccess');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('8E96A44242', N'ActivityLog', N'Read');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('8E96A44242', N'Allotment', N'Edit');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('8E96A44242', N'Customer', N'Edit');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('8E96A44242', N'Dealer', N'Edit');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('8E96A44242', N'Home', N'Read');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('8E96A44242', N'InquiryApi', N'NoAccess');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('8E96A44242', N'NDC', N'Read');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('8E96A44242', N'Payment', N'Edit');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('8E96A44242', N'Project', N'Read');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('8E96A44242', N'Property', N'Read');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('8E96A44242', N'Registration', N'Edit');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('8E96A44242', N'Rental', N'NoAccess');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('8E96A44242', N'Reports', N'Read');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('8E96A44242', N'SalesInquiry', N'Edit');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('8E96A44242', N'Settings', N'NoAccess');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('8E96A44242', N'TesSQL', N'NoAccess');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('8E96A44242', N'Ticket', N'Edit');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('8E96A44242', N'Transfer', N'Read');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('C6AA920B9D', N'Account', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('C6AA920B9D', N'AccountsManagement', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('C6AA920B9D', N'ActivityLog', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('C6AA920B9D', N'Allotment', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('C6AA920B9D', N'Customer', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('C6AA920B9D', N'Dealer', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('C6AA920B9D', N'DuplicateFileTransfer', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('C6AA920B9D', N'Home', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('C6AA920B9D', N'InquiryApi', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('C6AA920B9D', N'NDC', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('C6AA920B9D', N'Payment', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('C6AA920B9D', N'PaymentAudit', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('C6AA920B9D', N'Project', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('C6AA920B9D', N'Property', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('C6AA920B9D', N'Refund', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('C6AA920B9D', N'Registration', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('C6AA920B9D', N'Rental', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('C6AA920B9D', N'Reports', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('C6AA920B9D', N'SalesInquiry', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('C6AA920B9D', N'Settings', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('C6AA920B9D', N'TesSQL', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('C6AA920B9D', N'Ticket', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('C6AA920B9D', N'Transfer', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('C6AA920B9D', N'TransferFee', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('C6AA920B9D', N'Waiver', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00001 ', N'Account', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00001 ', N'AccountsManagement', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00001 ', N'ActivityLog', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00001 ', N'Allotment', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00001 ', N'Customer', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00001 ', N'Dealer', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00001 ', N'DuplicateFileTransfer', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00001 ', N'Home', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00001 ', N'InquiryApi', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00001 ', N'NDC', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00001 ', N'Payment', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00001 ', N'PaymentAudit', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00001 ', N'Project', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00001 ', N'Property', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00001 ', N'Refund', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00001 ', N'Registration', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00001 ', N'Rental', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00001 ', N'Reports', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00001 ', N'SalesInquiry', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00001 ', N'Settings', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00001 ', N'TesSQL', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00001 ', N'Ticket', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00001 ', N'Transfer', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00001 ', N'TransferFee', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00001 ', N'Waiver', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00002 ', N'Account', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00002 ', N'AccountsManagement', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00002 ', N'ActivityLog', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00002 ', N'Allotment', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00002 ', N'Customer', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00002 ', N'Dealer', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00002 ', N'DuplicateFileTransfer', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00002 ', N'Home', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00002 ', N'InquiryApi', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00002 ', N'NDC', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00002 ', N'Payment', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00002 ', N'PaymentAudit', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00002 ', N'Project', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00002 ', N'Property', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00002 ', N'Refund', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00002 ', N'Registration', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00002 ', N'Rental', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00002 ', N'Reports', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00002 ', N'SalesInquiry', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00002 ', N'Settings', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00002 ', N'TesSQL', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00002 ', N'Ticket', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00002 ', N'Transfer', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00002 ', N'TransferFee', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER00002 ', N'Waiver', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER002   ', N'Account', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER002   ', N'AccountsManagement', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER002   ', N'ActivityLog', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER002   ', N'Allotment', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER002   ', N'Customer', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER002   ', N'Dealer', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER002   ', N'DuplicateFileTransfer', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER002   ', N'Home', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER002   ', N'InquiryApi', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER002   ', N'NDC', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER002   ', N'Payment', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER002   ', N'PaymentAudit', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER002   ', N'Project', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER002   ', N'Property', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER002   ', N'Refund', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER002   ', N'Registration', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER002   ', N'Rental', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER002   ', N'Reports', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER002   ', N'SalesInquiry', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER002   ', N'Settings', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER002   ', N'TesSQL', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER002   ', N'Ticket', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER002   ', N'Transfer', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER002   ', N'TransferFee', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER002   ', N'Waiver', N'Admin');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER003   ', N'Account', N'NoAccess');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER003   ', N'AccountsManagement', N'NoAccess');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER003   ', N'ActivityLog', N'NoAccess');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER003   ', N'Allotment', N'Edit');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER003   ', N'Customer', N'Edit');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER003   ', N'Dealer', N'Edit');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER003   ', N'Home', N'NoAccess');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER003   ', N'InquiryApi', N'NoAccess');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER003   ', N'NDC', N'Edit');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER003   ', N'Payment', N'Edit');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER003   ', N'Project', N'Edit');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER003   ', N'Property', N'Edit');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER003   ', N'Registration', N'Edit');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER003   ', N'Rental', N'Edit');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER003   ', N'Reports', N'NoAccess');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER003   ', N'SalesInquiry', N'NoAccess');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER003   ', N'Settings', N'NoAccess');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER003   ', N'TesSQL', N'NoAccess');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER003   ', N'Ticket', N'NoAccess');
INSERT INTO [dbo].[UserModulePermission] ([UserID], [ModuleKey], [Permission]) VALUES ('USER003   ', N'Transfer', N'Edit');
GO

INSERT INTO [dbo].[Users] ([UserID], [FullName], [Email], [PasswordHash], [RoleID], [IsActive], [CreatedAt], [Department], [Designation], [UserType]) VALUES ('17BDD8B96F', N'Abbas', N'abbas@jubasmartcity.com', N'$2a$11$LJGGPob41w3ji7nuLVLH5.TsBmSQxafebk95sJVFJY306mDO0C31G', 'ADMIN001  ', NULL, '2025-10-11 04:32:42.0000000', N'IT', N'System Administrator', N'Admin');
INSERT INTO [dbo].[Users] ([UserID], [FullName], [Email], [PasswordHash], [RoleID], [IsActive], [CreatedAt], [Department], [Designation], [UserType]) VALUES ('873CC65AE1', N'Shahid', N'ghauri@pms.com', N'$2a$11$2FExdbwRCQBWGjqBiy.6/.xBuWvxPqdHXP8mOrEixz.onmfYc5MJS', 'ROLE003   ', NULL, '2026-02-17 03:14:49.0000000', N'Sales', N'Manager', N'CRO');
INSERT INTO [dbo].[Users] ([UserID], [FullName], [Email], [PasswordHash], [RoleID], [IsActive], [CreatedAt], [Department], [Designation], [UserType]) VALUES ('8E96A44242', N'Mr Amir Khan Niazi', N'makz@eclipse.com', N'$2a$11$hr23CiD/BMZFg2hx1HeSD.3dD7OlY2D/ZxEjLRDM3nTs5bcirKaDm', 'ROLE003   ', NULL, '2026-02-23 00:05:29.1670000', N'Sales', N'Sales Officer', N'CRO');
INSERT INTO [dbo].[Users] ([UserID], [FullName], [Email], [PasswordHash], [RoleID], [IsActive], [CreatedAt], [Department], [Designation], [UserType]) VALUES ('ADMIN     ', N'System Admin', N'admin@example.com', N'NA', NULL, NULL, '2025-11-06 02:19:43.0000000', NULL, NULL, NULL);
INSERT INTO [dbo].[Users] ([UserID], [FullName], [Email], [PasswordHash], [RoleID], [IsActive], [CreatedAt], [Department], [Designation], [UserType]) VALUES ('C6AA920B9D', N'Shahid', N'shahid@pms.com', N'$2a$11$.fKEVYenxfYQBb.x8I1c4es2QXRXa8WRO1v4NQN5sDltSi8i.ZOtq', 'ADMIN001  ', NULL, '2026-02-02 21:22:36.7370000', NULL, NULL, NULL);
INSERT INTO [dbo].[Users] ([UserID], [FullName], [Email], [PasswordHash], [RoleID], [IsActive], [CreatedAt], [Department], [Designation], [UserType]) VALUES ('USER00001 ', N'System Administrator', N'admin@jubasmartcity.com', N'$2a$11$e0SrYgQeqxiRSLbEcgHo1uzv7mCfY9hwTqCj7.QYgEYUfZuzOv8Aq', 'ADMIN001  ', NULL, '2025-10-11 02:52:46.0000000', NULL, NULL, NULL);
INSERT INTO [dbo].[Users] ([UserID], [FullName], [Email], [PasswordHash], [RoleID], [IsActive], [CreatedAt], [Department], [Designation], [UserType]) VALUES ('USER00002 ', N'Abbas', N'abbas@pms.com', N'$2a$11$b5WddlcvMw.waQ7Lrtx6/urxSweMhtfH9bzVNlJQuVszBmGPGkyLe', 'ADMIN001  ', NULL, '2026-01-23 02:37:32.0000000', N'IT', N'System Administrator', N'Admin');
INSERT INTO [dbo].[Users] ([UserID], [FullName], [Email], [PasswordHash], [RoleID], [IsActive], [CreatedAt], [Department], [Designation], [UserType]) VALUES ('USER001   ', N'Admin User', N'admin@juba.com', N'$2a$11$xQqKvZ9Z9YqYZQYZqYZqY.YqYZqYZqYZqYZqYZqYZqYZqYZqYZqY', 'ROLE001   ', NULL, '2025-10-11 07:13:24.0000000', NULL, NULL, NULL);
INSERT INTO [dbo].[Users] ([UserID], [FullName], [Email], [PasswordHash], [RoleID], [IsActive], [CreatedAt], [Department], [Designation], [UserType]) VALUES ('USER002   ', N'user', N'user@pms.com', N'$2a$11$jdYiICrZVzGo477VoZeHEeKDuWSachGRy6mLuL/knLPTYtJGeZ3ZS', 'ADMIN001  ', NULL, '2025-10-11 07:13:24.0000000', N'IT', N'System Administrator', N'Admin');
INSERT INTO [dbo].[Users] ([UserID], [FullName], [Email], [PasswordHash], [RoleID], [IsActive], [CreatedAt], [Department], [Designation], [UserType]) VALUES ('USER003   ', N'Yasir', N'operations@pms.com', N'$2a$11$xQqKvZ9Z9YqYZQYZqYZqY.YqYZqYZqYZqYZqYZqYZqYZqYZqYZqY', 'ROLE003   ', NULL, '2025-10-11 07:13:24.0000000', N'Sales', N'CRO', N'CRO');
GO

INSERT INTO [dbo].[UserSessions] ([SessionID], [UserID], [LoginTime], [LogoutTime], [IPAddress], [DeviceInfo]) VALUES ('104A856972', 'USER00002 ', '2026-05-04 20:22:16.8700000', NULL, N'205.164.151.8', N'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36');
INSERT INTO [dbo].[UserSessions] ([SessionID], [UserID], [LoginTime], [LogoutTime], [IPAddress], [DeviceInfo]) VALUES ('5A706688DB', 'USER00002 ', '2026-05-04 20:24:16.5300000', NULL, N'154.198.115.236', N'Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Mobile Safari/537.36');
INSERT INTO [dbo].[UserSessions] ([SessionID], [UserID], [LoginTime], [LogoutTime], [IPAddress], [DeviceInfo]) VALUES ('5C1C7E706C', 'USER00002 ', '2026-05-04 20:24:16.7870000', NULL, N'154.198.115.236', N'Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Mobile Safari/537.36');
INSERT INTO [dbo].[UserSessions] ([SessionID], [UserID], [LoginTime], [LogoutTime], [IPAddress], [DeviceInfo]) VALUES ('F5DC47A331', 'USER00002 ', '2026-05-04 20:21:33.7000000', NULL, N'205.164.151.8', N'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36');
GO

EXEC sp_msforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';
GO

-- End of file
