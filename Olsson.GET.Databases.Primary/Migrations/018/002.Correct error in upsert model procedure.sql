ALTER PROCEDURE [dbo].[UpsertModel]
	@imageName NVARCHAR(256),
	@modelName NVARCHAR(256),
	@startDateTime DATETIME,
	@modflowExeName VARCHAR(50),
	@namFileName VARCHAR(50),
	@runFileName VARCHAR(50),
	@mapRunFileName VARCHAR(50),
	@mapDrawdownFileName VARCHAR(50) OUTPUT,
	@mapSettings VARCHAR(1024),
	@mapModelArea VARCHAR(MAX),
	@zoneBudgetExeName VARCHAR(50),
	@isDoubleSizeHeatMapOutput BIT,
	@allowablePercentDiscrepancy FLOAT,
	@scenarios dbo.ScenariosList READONLY,
	@inputZoneData varchar(MAX),
	@outputZoneData varchar(MAX),
	@numberOfStressPeriods int,
	@canalData varchar(max),
	@modPathExeName VARCHAR(50),
	@simulationFileName VARCHAR(50),
	@listFileName VARCHAR(50),
	@baseflowTableProcessingConfigurationID int,
	@customStartDatesForStressPeriods dbo.DateList READONLY
AS
BEGIN

DECLARE @imageId INT;
DECLARE @modelId INT;

	SET NOCOUNT ON;

    SELECT @imageId = ImageID FROM [Image] WHERE ImageName = @imageName
   IF @imageId is null
	BEGIN
		declare @ID table (ID int);
		insert [Image](ImageID, ImageName, [Server])
		output inserted.ImageID into @ID
		values ((select (max(ImageID) + 1) from [Image]),@imageName,(select top 1 [server] from [Image]));
		SELECT @imageId = ID FROM @ID;
	END

	MERGE Model as Target
		USING (select @modelName as ModelName) as Source
		ON Target.ModelName = Source.ModelName
		WHEN MATCHED THEN
			UPDATE SET StartDateTime = @startDateTime,
			           ModflowExeName = @modflowExeName,
					   NamFileName = @namFileName,
					   RunFileName = @runFileName,
					   AllowablePercentDiscrepancy = @allowablePercentDiscrepancy,
					   MapRunFileName = @mapRunFileName,
					   MapDrawdownFileName = @mapDrawdownFileName,
					   IsDoubleSizeHeatMapOutput = @isDoubleSizeHeatMapOutput,
					   MapSettings = @mapSettings,
					   ImageId = @imageId,
					   ZoneBudgetExeName = @zoneBudgetExeName,
					   NumberOfStressPeriods = @numberOfStressPeriods,
					   CanalData = @canalData,
					   ModpathExeName = @modPathExeName,
					   SimulationFileName = @simulationFileName,
					   ListFileName = @listFileName,
					   BaseflowTableProcessingConfigurationID = @baseflowTableProcessingConfigurationID
		WHEN NOT MATCHED THEN
			INSERT (ModelID, ModelName, StartDateTime, ModflowExeName, NamFileName, RunFileName, AllowablePercentDiscrepancy, MapRunFileName, MapDrawdownFileName, IsDoubleSizeHeatMapOutput, MapSettings, ImageId, ZoneBudgetExeName, NumberOfStressPeriods, CanalData, ModpathExeName, SimulationFileName, ListFileName, BaseflowTableProcessingConfigurationID) 
			VALUES ((select (max(ModelID) + 1) from Model), @modelName, @startDateTime, @modflowExeName, @namFileName, @runFileName, @allowablePercentDiscrepancy, @mapRunFileName, @mapDrawdownFileName, @isDoubleSizeHeatMapOutput, @mapSettings, @imageId, @zoneBudgetExeName, @numberOfStressPeriods, @canalData,  @modPathExeName, @simulationFileName, @listFileName, @baseflowTableProcessingConfigurationID);

	SELECT @modelId = ModelID FROM Model WHERE ModelName = @modelName;

	MERGE ModelScenario as Target
		USING @scenarios as Source
		ON Target.ModelId = @ModelId and Target.ScenarioId = Source.id
		WHEN NOT MATCHED THEN
			INSERT (ModelId, ScenarioId) VALUES (@modelId, Source.Id)
		WHEN NOT MATCHED BY SOURCE AND Target.ModelId = @modelId THEN
			DELETE;

	MERGE ModelMapAreaBoundary as Target
		USING (select @modelID as ModelID) as Source
		ON Target.ModelID = @ModelID
		WHEN MATCHED THEN
			UPDATE SET MapAreaBoundary = @mapModelArea
		WHEN NOT MATCHED THEN
			INSERT (ModelID, MapAreaBoundary) VALUES (Source.ModelID, @mapModelArea);

	MERGE ModelInputZoneData as Target
		USING (select @modelID as ModelID) as Source
		ON Target.ModelID = @ModelID
		WHEN MATCHED THEN
			UPDATE SET InputZoneData = @inputZoneData
		WHEN NOT MATCHED THEN
			INSERT (ModelID, InputZoneData) VALUES (Source.ModelID, @inputZoneData);

	MERGE ModelOutputZoneData as Target
		USING (select @modelID as ModelID) as Source
		ON Target.ModelID = @ModelID
		WHEN MATCHED THEN
			UPDATE SET OutputZoneData = @outputZoneData
		WHEN NOT MATCHED THEN
			INSERT (ModelID, OutputZoneData) VALUES (Source.ModelID, @outputZoneData);


	DECLARE @customStartDateCount int;
	SELECT @customStartDateCount = count(*) from @customStartDatesForStressPeriods
	IF (@customStartDateCount = @numberOfStressPeriods)
	BEGIN
		DELETE from dbo.ModelStressPeriodCustomStartDate where ModelID = @modelId
		
		INSERT INTO dbo.ModelStressPeriodCustomStartDate(ModelID, StressPeriod, StressPeriodStartDate)
		select @modelId, ROW_NUMBER() OVER(ORDER BY [Date]) as StressPeriod, [Date]
		from @customStartDatesForStressPeriods
		order by [Date]
	END
END
go