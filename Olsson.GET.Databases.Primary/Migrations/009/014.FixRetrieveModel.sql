ALTER PROCEDURE dbo.RetrieveModel
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
	@mapZone VARCHAR(MAX) OUTPUT,
	@numberOfStressPeriods int OUTPUT,
	@canalData varchar(max) OUTPUT,
	@modPathExeName VARCHAR(50) OUTPUT,
	@simulationFileName VARCHAR(50) OUTPUT,
	@listFileName VARCHAR(50) OUTPUT
AS
BEGIN

	SET NOCOUNT ON;
SELECT @imageName = i.[Name],
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
	   @mapZone = m.ZoneData,
	   @numberOfStressPeriods = m.NumberOfStressPeriods,
	   @canalData = m.CanalData,
	   @modPathExeName = m.ModPathExeName,
	   @simulationFileName = m.SimulationFileName,
	   @listFileName = m.ListFileName
FROM Images i INNER JOIN 
     Models m ON i.Id = m.ImageId
WHERE @modelName = m.[Name]
END
GO