alter table [dbo].[Models]
	add MapBaselineFileName varchar(50) null;
go

alter table [dbo].[Models]
	add MapRunFileName varchar(50) null;
go

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE dbo.UpsertModel
	@imageName NVARCHAR(256),
	@modelName NVARCHAR(256),
	@startDateTime DATETIME,
	@modflowExeName VARCHAR(50),
	@namFileName VARCHAR(50),
	@baselineFileName VARCHAR(50),
	@runFileName VARCHAR(50),
	@zonesFileName VARCHAR(50),
	@nodeFlowProportionsFileName VARCHAR(50),
	@friendlyZoneNamesFileName VARCHAR(50),
	@locationMapFileName VARCHAR(50),
	@mapBaselineFileName VARCHAR(50),
	@mapRunFileName VARCHAR(50),
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
					   BaselineFileName = @baselineFileName,
					   RunFileName = @runFileName,
					   ZonesFileName = @zonesFileName,
					   NodeFlowProportionsFileName = @nodeFlowProportionsFileName,
					   FriendlyZoneNamesFileName = @friendlyZoneNamesFileName,
					   AllowablePercentDiscrepancy = @allowablePercentDiscrepancy,
					   LocationMapFileName = @locationMapFileName,
					   MapBaselineFileName = @mapBaselineFileName,
					   MapRunFileName = @mapRunFileName,
					   ImageId = @imageId
		WHEN NOT MATCHED THEN
			INSERT (Id, Name, StartDateTime, ModflowExeName, NamFileName, BaselineFileName, RunFileName, ZonesFileName, NodeFlowProportionsFileName, FriendlyZoneNamesFileName, AllowablePercentDiscrepancy, ImageId) 
			VALUES ((select (max(id) + 1) from Models), @modelName, @startDateTime, @modflowExeName, @namFileName, @baselineFileName, @runFileName, @zonesFileName, @nodeFlowProportionsFileName, @friendlyZoneNamesFileName, @allowablePercentDiscrepancy, @imageId);

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

CREATE PROCEDURE dbo.RetrieveModel
	@modelName NVARCHAR(256),
    @imageName NVARCHAR(256) OUTPUT,
	@startDateTime DATETIME OUTPUT,
	@modflowExeName VARCHAR(50) OUTPUT,
	@namFileName VARCHAR(50) OUTPUT,
	@baselineFileName VARCHAR(50) OUTPUT,
	@runFileName VARCHAR(50) OUTPUT,
	@zonesFileName VARCHAR(50) OUTPUT,
	@nodeFlowProportionsFileName VARCHAR(50) OUTPUT,
	@friendlyZoneNamesFileName VARCHAR(50) OUTPUT,
	@locationMapFileName VARCHAR(50) OUTPUT,
	@mapBaselineFileName VARCHAR(50) OUTPUT,
	@mapRunFileName VARCHAR(50) OUTPUT,
	@allowablePercentDiscrepancy FLOAT OUTPUT
AS
BEGIN

	SET NOCOUNT ON;
SELECT @imageName = i.[Name],
       @startDateTime = m.StartDateTime,
	   @modflowExeName = m.ModflowExeName,
	   @namFileName = m.NamFileName,
	   @baselineFileName = m.BaselineFileName,
	   @runFileName = m.RunFileName,
	   @zonesFileName = m.ZonesFileName,
	   @nodeFlowProportionsFileName = m.NodeFlowProportionsFileName,
	   @friendlyZoneNamesFileName = m.FriendlyZoneNamesFileName,
	   @locationMapFileName = m.LocationMapFileName,
	   @mapBaselineFileName = m.MapBaselineFileName,
	   @mapRunFileName = m.MapRunFileName,
	   @allowablePercentDiscrepancy = m.AllowablePercentDiscrepancy
FROM Images i INNER JOIN 
     Models m ON i.Id = m.ImageId
WHERE @modelName = m.[Name]
END
GO