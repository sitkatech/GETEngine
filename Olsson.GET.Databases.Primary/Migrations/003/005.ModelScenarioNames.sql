update dbo.Models set name = 'Central Platte NRD' 
	where Id = 1;
go

INSERT INTO [dbo].[Scenarios]
           ([Id]
           ,[Name])
     VALUES
           (4
           ,'Canal Recharge')
GO

INSERT INTO [dbo].[ModelScenarios]
           ([ModelId]
           ,[ScenarioId])
     VALUES
           (1
           ,4)
GO

delete from dbo.ModelScenarios
	where modelid = 1 and scenarioid = 1;
go

delete from dbo.ModelScenarios
	where modelid = 1 and scenarioid = 2;
go

delete from dbo.ModelScenarios
	where modelid = 1 and scenarioid = 3;
go

delete from dbo.CustomerModelScenarios
	where modelid = 1 and scenarioid = 1;
go

delete from dbo.CustomerModelScenarios
	where modelid = 1 and scenarioid = 2;
go

delete from dbo.CustomerModelScenarios
	where modelid = 1 and scenarioid = 3;
go

update dbo.Images set name = 'cpnrd', server ='$ImageServerUri$' where id = 1;
go