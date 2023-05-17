CREATE TABLE [dbo].[ScenarioFiles](
    [Id] INT NOT NULL,
	[ScenarioId] INT NOT NULL,
    [Name] [nvarchar](256) NOT NULL,
	[Description] [nvarchar](512),
	[Required] BIT NOT NULL
 CONSTRAINT [PK_dbo.ScenarioFiles] PRIMARY KEY CLUSTERED 
(
    [Id] ASC
)
) ON [PRIMARY]
GO


ALTER TABLE [dbo].[ScenarioFiles]
ADD FOREIGN KEY (ScenarioId) REFERENCES Scenarios(Id);