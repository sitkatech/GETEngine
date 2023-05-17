alter table [dbo].[Models]
	drop column BaselineFileName;
go

alter table [dbo].[Models]
	drop column ZonesFileName;
go

alter table [dbo].[Models]
	drop column NodeFlowProportionsFileName;
go

alter table [dbo].[Models]
	drop column FriendlyZoneNamesFileName;
go

alter table [dbo].[Models]
	drop column LocationMapFileName;
go

alter table [dbo].[Models]
	drop column MapBaselineFileName;
go

ALTER PROCEDURE dbo.UpsertModel
	@imageName NVARCHAR(256),
	@modelName NVARCHAR(256),
	@startDateTime DATETIME,
	@modflowExeName VARCHAR(50),
	@namFileName VARCHAR(50),
	@runFileName VARCHAR(50),
	@mapRunFileName VARCHAR(50),
	@mapSettings VARCHAR(1024),
	@mapModelArea VARCHAR(MAX),
	@isDoubleSizeHeatMapOutput BIT,
	@allowablePercentDiscrepancy FLOAT,
	@scenarios dbo.ScenariosList READONLY
AS
BEGIN

DECLARE @imageId INT;
DECLARE @modelId INT;

	SET NOCOUNT ON;

    SELECT @imageId = Id FROM Images WHERE [Name] = @imageName
   IF @imageId is null
	BEGIN
		declare @ID table (ID int);
		insert Images(Id, [Name], [Server])
		output inserted.Id into @ID
		values ((select (max(id) + 1) from Images),@imageName,(select top 1 [server] from Images));
		SELECT @imageId = ID FROM @ID;
	END

	MERGE Models as Target
		USING (select @modelName as [Name]) as Source
		ON Target.Name = Source.Name
		WHEN MATCHED THEN
			UPDATE SET StartDateTime = @startDateTime,
			           ModflowExeName = @modflowExeName,
					   NamFileName = @namFileName,
					   RunFileName = @runFileName,
					   AllowablePercentDiscrepancy = @allowablePercentDiscrepancy,
					   MapRunFileName = @mapRunFileName,
					   IsDoubleSizeHeatMapOutput = @isDoubleSizeHeatMapOutput,
					   MapSettings = @mapSettings,
					   MapModelArea = @mapModelArea,
					   ImageId = @imageId
		WHEN NOT MATCHED THEN
			INSERT (Id, Name, StartDateTime, ModflowExeName, NamFileName, RunFileName, AllowablePercentDiscrepancy, ImageId) 
			VALUES ((select (max(id) + 1) from Models), @modelName, @startDateTime, @modflowExeName, @namFileName, @runFileName, 
			@allowablePercentDiscrepancy, @imageId);

	SELECT @modelId = Id FROM Models WHERE [Name] = @modelName;

	MERGE ModelScenarios as Target
		USING @scenarios as Source
		ON Target.ModelId = @ModelId and Target.ScenarioId = Source.id
		WHEN NOT MATCHED THEN
			INSERT (ModelId, ScenarioId) VALUES (@modelId, Source.Id)
		WHEN NOT MATCHED BY SOURCE AND Target.ModelId = @modelId THEN
			DELETE;
END
GO

ALTER PROCEDURE dbo.RetrieveModel
	@modelName NVARCHAR(256),
    @imageName NVARCHAR(256) OUTPUT,
	@startDateTime DATETIME OUTPUT,
	@modflowExeName VARCHAR(50) OUTPUT,
	@namFileName VARCHAR(50) OUTPUT,
	@runFileName VARCHAR(50) OUTPUT,
	@mapRunFileName VARCHAR(50) OUTPUT,
	@mapSettings VARCHAR(1024) OUTPUT,
	@mapModelArea VARCHAR(MAX) OUTPUT,
	@isDoubleSizeHeatMapOutput BIT OUTPUT,
	@allowablePercentDiscrepancy FLOAT OUTPUT
AS
BEGIN

	SET NOCOUNT ON;
SELECT @imageName = i.[Name],
       @startDateTime = m.StartDateTime,
	   @modflowExeName = m.ModflowExeName,
	   @namFileName = m.NamFileName,
	   @runFileName = m.RunFileName,
	   @mapRunFileName = m.MapRunFileName,
	   @mapSettings = m.MapSettings,
	   @mapModelArea = m.MapModelArea,
	   @isDoubleSizeHeatMapOutput = m.IsDoubleSizeHeatMapOutput,
	   @allowablePercentDiscrepancy = m.AllowablePercentDiscrepancy
FROM Images i INNER JOIN 
     Models m ON i.Id = m.ImageId
WHERE @modelName = m.[Name]
END
GO