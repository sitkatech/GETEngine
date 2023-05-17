CREATE TABLE [dbo].[RunBuckets](
    [Id] INT IDENTITY(1,1),
    [Name] [nvarchar](256) NOT NULL,
	[CreatedDate] datetime not null,
	[UserId]  int NOT NULL,
	[CustomerId]  int NOT NULL,
	[FileStorageLocator] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_dbo.RunBuckets] PRIMARY KEY CLUSTERED 
(
    [Id] ASC
)
) ON [PRIMARY]
GO

CREATE INDEX IX_RunBuckets_CID ON dbo.RunBuckets (CustomerId)
GO

Alter TABLE [dbo].[RunBuckets]
	add	CONSTRAINT FK_RunBuckets_Users FOREIGN KEY (UserId) REFERENCES [dbo].[Users] (Id);
GO

Alter TABLE [dbo].[RunBuckets]
	add	CONSTRAINT FK_RunBuckets_Customers FOREIGN KEY (CustomerId) REFERENCES [dbo].[Customers] (Id);
GO

CREATE TABLE [dbo].[RunBucketRuns](
    [Id] INT IDENTITY(1,1),
    [RunBucketId] INT NOT NULL,
    [RunId] INT NOT NULL,
 CONSTRAINT [PK_dbo.RunBucketRuns] PRIMARY KEY CLUSTERED 
(
    [Id] ASC
)
) ON [PRIMARY]
GO

CREATE INDEX IX_RunBucketRuns_RID ON dbo.RunBucketRuns (RunId)
GO

CREATE INDEX IX_RunBucketRuns_RBID ON dbo.RunBucketRuns (RunBucketId)
GO

Alter TABLE [dbo].[RunBucketRuns]
	add	CONSTRAINT FK_RunBuckets_RunBucketRuns FOREIGN KEY (RunBucketId) REFERENCES [dbo].[RunBuckets] (Id);
GO

Alter TABLE [dbo].[RunBucketRuns]
	add	CONSTRAINT FK_Runs_RunBucketRuns FOREIGN KEY (RunId) REFERENCES [dbo].[Runs] (Id);
GO

