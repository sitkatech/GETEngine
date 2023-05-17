CREATE TABLE [dbo].[Roles](
    [Id] INT NOT NULL,
    [Name] [nvarchar](256) NOT NULL,
	[Category] int NOT NULL DEFAULT 1,
	[Description] [nvarchar](512) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetRoles] PRIMARY KEY CLUSTERED 
(
    [Id] ASC
)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Users](
    [Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[UserName] [nvarchar](256) NOT NULL,
	[Password] [nvarchar](max) NULL,
    [IsLockedOut] [bit] NOT NULL,
	[LockoutExpiration] [datetimeoffset] NULL,
	[FailedAttemptCount] [int] NOT NULL,
    [SecurityStamp] [nvarchar](max) NULL,
	[Email] [nvarchar](256) NULL,
    [EmailConfirmed] [bit] NOT NULL DEFAULT 1,
	[CustomerId] [int] NOT NULL DEFAULT 1,
	CONSTRAINT [PK_dbo.AspNetUsers] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
)
GO

CREATE UNIQUE INDEX IX_Users_UserName ON dbo.Users (UserName)
GO

CREATE UNIQUE INDEX IX_Users_Email ON dbo.Users (Email)
GO

CREATE TABLE [dbo].[UserRoles](
    [UserId] [int] NOT NULL,
    [RoleId] [int] NOT NULL,
	CONSTRAINT [PK_dbo.UserRoles] PRIMARY KEY CLUSTERED 
	(
		[UserId] ASC,
		[RoleId] ASC
	),
	CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles (Id),
	CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE CASCADE
) 
GO

CREATE INDEX IX_UserRoles_UID ON dbo.UserRoles (UserId)
GO

INSERT INTO dbo.Roles (Id, Name, [Description], Category) VALUES (1, 'Admin', 'Admin', 1)
GO