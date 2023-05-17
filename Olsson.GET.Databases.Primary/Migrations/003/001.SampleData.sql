INSERT INTO [dbo].[Scenarios]
           ([Id]
           ,[Name])
     VALUES
           (1
           ,'Add a Well')
GO

INSERT INTO [dbo].[Scenarios]
           ([Id]
           ,[Name])
     VALUES
           (2
           ,'Remove a Well')
GO

INSERT INTO [dbo].[Scenarios]
           ([Id]
           ,[Name])
     VALUES
           (3
           ,'Move a Well')
GO

INSERT INTO [dbo].[InputControls]
           ([Id]
           ,[Name])
     VALUES
           (1
           ,'CSVUploader')
GO

INSERT INTO [dbo].[Images]
           ([Id]
           ,[Name]
           ,[Server])
     VALUES
           (1
           ,'Image1'
           ,'localhost')
GO



INSERT INTO [dbo].[Models]
           ([Id]
           ,[Name]
           ,[ImageId])
     VALUES
           (1
           ,'Demo Model'
           ,1)
GO


INSERT INTO [dbo].[ModelScenarios]
           ([ModelId]
           ,[ScenarioId])
     VALUES
           (1
           ,1)
GO

INSERT INTO [dbo].[ModelScenarios]
           ([ModelId]
           ,[ScenarioId])
     VALUES
           (1
           ,2)
GO

INSERT INTO [dbo].[ModelScenarios]
           ([ModelId]
           ,[ScenarioId])
     VALUES
           (1
           ,3)
GO