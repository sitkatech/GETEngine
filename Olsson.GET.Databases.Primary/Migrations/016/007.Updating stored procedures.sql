ALTER PROCEDURE [dbo].[RetrieveModel]
	@modelName NVARCHAR(256),
    @imageName NVARCHAR(256) OUTPUT,
	@startDateTime DATETIME OUTPUT,
	@modflowExeName VARCHAR(50) OUTPUT,
	@namFileName VARCHAR(50) OUTPUT,
	@runFileName VARCHAR(50) OUTPUT,
	@mapRunFileName VARCHAR(50) OUTPUT,
	@mapDrawdownFileName VARCHAR(50) OUTPUT,
	@mapSettings VARCHAR(1024) OUTPUT,
	@mapModelArea VARCHAR(MAX) OUTPUT,
	@zoneBudgetExeName VARCHAR(50) OUTPUT,
	@isDoubleSizeHeatMapOutput BIT OUTPUT,
	@allowablePercentDiscrepancy FLOAT OUTPUT,
	@mapInputZone VARCHAR(MAX) OUTPUT,
	@mapOutputZone VARCHAR(MAX) OUTPUT,
	@numberOfStressPeriods int OUTPUT,
	@canalData varchar(max) OUTPUT,
	@modPathExeName VARCHAR(50) OUTPUT,
	@simulationFileName VARCHAR(50) OUTPUT,
	@listFileName VARCHAR(50) OUTPUT,
	@baseflowTableProcessingConfigurationID int OUTPUT
AS
BEGIN

	SET NOCOUNT ON;
SELECT @imageName = i.ImageName,
       @startDateTime = m.StartDateTime,
	   @modflowExeName = m.ModflowExeName,
	   @namFileName = m.NamFileName,
	   @runFileName = m.RunFileName,
	   @mapRunFileName = m.MapRunFileName,
	   @mapDrawdownFileName = m.MapDrawdownFileName,
	   @mapSettings = m.MapSettings,
	   @mapModelArea = m.MapModelArea,
	   @zoneBudgetExeName = m.ZoneBudgetExeName,
	   @isDoubleSizeHeatMapOutput = m.IsDoubleSizeHeatMapOutput,
	   @allowablePercentDiscrepancy = m.AllowablePercentDiscrepancy,
	   @mapInputZone = m.InputZoneData,
	   @mapOutputZone = m.OutputZoneData,
	   @numberOfStressPeriods = m.NumberOfStressPeriods,
	   @canalData = m.CanalData,
	   @modPathExeName = m.ModPathExeName,
	   @simulationFileName = m.SimulationFileName,
	   @listFileName = m.ListFileName,
	   @baseflowTableProcessingConfigurationID = m.BaseflowTableProcessingConfigurationID
FROM [Image] i INNER JOIN 
     Model m ON i.ImageID = m.ImageId
WHERE @modelName = m.ModelName
END
GO

ALTER PROCEDURE [dbo].[pInsertBaseflowTableProcessingConfiguration]
	@baseflowTableIndicatorRegexPattern varchar(200),
	@segmentColumnNum int,
	@flowToAquiferColumnNum int,
	@reachColumnNum int
AS
BEGIN

	SET NOCOUNT ON;
	
	insert into dbo.BaseflowTableProcessingConfiguration(BaseflowTableIndicatorRegexPattern, SegmentColumnNum, FlowToAquiferColumnNum, ReachColumnNum)
	values (@baseflowTableIndicatorRegexPattern, @segmentColumnNum, @flowToAquiferColumnNum, @reachColumnNum)

END
GO

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
					   MapModelArea = @mapModelArea,
					   ImageId = @imageId,
					   InputZoneData = @inputZoneData,
					   OutputZoneData = @outputZoneData,
					   ZoneBudgetExeName = @zoneBudgetExeName,
					   NumberOfStressPeriods = @numberOfStressPeriods,
					   CanalData = @canalData,
					   ModpathExeName = @modPathExeName,
					   SimulationFileName = @simulationFileName,
					   ListFileName = @listFileName,
					   BaseflowTableProcessingConfigurationID = @baseflowTableProcessingConfigurationID
		WHEN NOT MATCHED THEN
			INSERT (ModelID, ModelName, StartDateTime, ModflowExeName, NamFileName, RunFileName, AllowablePercentDiscrepancy, MapRunFileName, MapDrawdownFileName, IsDoubleSizeHeatMapOutput, MapSettings, MapModelArea, ImageId, InputZoneData, OutputZoneData, ZoneBudgetExeName, NumberOfStressPeriods, CanalData, ModpathExeName, SimulationFileName, ListFileName, BaseflowTableProcessingConfigurationID) 
			VALUES ((select (max(ModelID) + 1) from Model), @modelName, @startDateTime, @modflowExeName, @namFileName, @runFileName, @allowablePercentDiscrepancy, @mapRunFileName, @mapDrawdownFileName, @isDoubleSizeHeatMapOutput, @mapSettings, @mapModelArea, @imageId, @inputZoneData, @outputZoneData, @zoneBudgetExeName, @numberOfStressPeriods, @canalData,  @modPathExeName, @simulationFileName, @listFileName, @baseflowTableProcessingConfigurationID);

	SELECT @modelId = ModelID FROM Model WHERE ModelName = @modelName;

	MERGE ModelScenario as Target
		USING @scenarios as Source
		ON Target.ModelId = @ModelId and Target.ScenarioId = Source.id
		WHEN NOT MATCHED THEN
			INSERT (ModelId, ScenarioId) VALUES (@modelId, Source.Id)
		WHEN NOT MATCHED BY SOURCE AND Target.ModelId = @modelId THEN
			DELETE;

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
GO


