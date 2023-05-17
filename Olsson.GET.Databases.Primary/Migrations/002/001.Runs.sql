CREATE TABLE [dbo].[Images](
    [Id] [int] NOT NULL,	
	[Name] [nvarchar](256) NOT NULL,		
	[Server] [nvarchar](256) NOT NULL,		
    CONSTRAINT [PK_dbo.Images] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
)
GO

CREATE TABLE [dbo].[Models](
    [Id] [int] NOT NULL,	
	[Name] [nvarchar](256) NOT NULL,	
	[ImageId]  int NOT NULL  FOREIGN KEY REFERENCES Images(Id),
    CONSTRAINT [PK_dbo.Models] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
)
GO

CREATE TABLE [dbo].[Scenarios](
    [Id] [int] NOT NULL,	
	[Name] [nvarchar](256) NOT NULL,		
    CONSTRAINT [PK_dbo.Scenarios] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
)
GO


CREATE TABLE [dbo].[InputControls](
    [Id] [int] NOT NULL,	
	[Name] [nvarchar](256) NOT NULL,		
    CONSTRAINT [PK_dbo.InputControls] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
)
GO

CREATE TABLE [dbo].[Runs](
    [Id] [int] IDENTITY(1,1) NOT NULL,	
	[Name] [nvarchar](256) NOT NULL,
	[FileStorageLocator] [nvarchar](50) NOT NULL,
	[ImageId]  int NOT NULL  FOREIGN KEY REFERENCES Images(Id),
	[ModelId]  int NOT NULL  FOREIGN KEY REFERENCES Models(Id),
	[ScenarioId]  int NOT NULL  FOREIGN KEY REFERENCES Scenarios(Id),
	[InputControlId]  int NOT NULL  FOREIGN KEY REFERENCES InputControls(Id),
	[UserId]  int NOT NULL  FOREIGN KEY REFERENCES Users(Id),
	[CustomerId]  int NOT NULL FOREIGN KEY REFERENCES Customers(Id),
	[Status] int NOT NULL,
	[CreatedDate] datetime not null,
    CONSTRAINT [PK_dbo.Runs] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
)
GO

CREATE TABLE [dbo].[ModelScenarios](
    [ModelId] [int] NOT NULL,
    [ScenarioId] [int] NOT NULL,
	CONSTRAINT [PK_dbo.ModelScenarios] PRIMARY KEY CLUSTERED 
	(
		[ModelId] ASC,
		[ScenarioId] ASC
	),
	CONSTRAINT FK_ModelScenarios_Models FOREIGN KEY (ModelId) REFERENCES dbo.Models (Id),
	CONSTRAINT FK_ModelScenarios_Scenarios FOREIGN KEY (ScenarioId) REFERENCES dbo.Scenarios (Id) 
) 
GO

CREATE TABLE [dbo].[CustomerModelScenarios](
    [CustomerId] [int] NOT NULL,
	[ModelId] [int] NOT NULL,
    [ScenarioId] [int] NOT NULL,
	CONSTRAINT [PK_dbo.CustomerModelScenarios] PRIMARY KEY CLUSTERED 
	(
		[CustomerId] ASC,
		[ModelId] ASC,
		[ScenarioId] ASC
	),
	CONSTRAINT FK_CustomerModelScenarios_Cutomers FOREIGN KEY (CustomerId) REFERENCES dbo.Customers (Id),
	CONSTRAINT FK_CustomerModelScenarios_Models FOREIGN KEY (ModelId) REFERENCES dbo.Models (Id),
	CONSTRAINT FK_CustomerModelScenarios_Scenarios FOREIGN KEY (ScenarioId) REFERENCES dbo.Scenarios (Id) 
) 
GO