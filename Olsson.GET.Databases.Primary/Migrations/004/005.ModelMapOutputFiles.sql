BEGIN
DECLARE @imageName NVARCHAR(256);
DECLARE @modelName NVARCHAR(256);
DECLARE @startDateTime DATETIME;
DECLARE @modflowExeName VARCHAR(50);
DECLARE @namFileName VARCHAR(50);
DECLARE @baselineFileName VARCHAR(50);
DECLARE @runFileName VARCHAR(50);
DECLARE @zonesFileName VARCHAR(50);
DECLARE @nodeFlowProportionsFileName VARCHAR(50);
DECLARE @friendlyZoneNamesFileName VARCHAR(50);
DECLARE @locationMapFileName VARCHAR(50);
DECLARE @mapBaselineFileName VARCHAR(50);
DECLARE @mapRunFileName VARCHAR(50);
DECLARE @allowablePercentDiscrepancy FLOAT;
DECLARE @scenarios as dbo.ScenariosList;

--- Set These Values -----
SET @modelName = 'Central Platte NRD';                
--- End Values to Set -----

exec dbo.RetrieveModel @modelName, @imageName OUT, @startDateTime OUT, @modflowExeName OUT, @namFileName OUT, @baselineFileName OUT, @runFileName OUT, @zonesFileName OUT, @nodeFlowProportionsFileName OUT, @friendlyZoneNamesFileName OUT, @locationMapFileName OUT, @mapBaselineFileName OUT, @mapRunFileName OUT, @allowablePercentDiscrepancy OUT;

--- Set Only The Values That Need Changes and Always Set @scenarios -----
SET @locationMapFileName = 'relateMat.txt';           --The name of the csv file that defines which locations are located at which position for the map.  This can be set to null (no map will be produced).
SET @mapBaselineFileName = 'Baseline.hds';            --The name of the heat map data baseline file.  This can be set to null (no map will be produced).
SET @mapRunFileName = 'CPNRD.hds';                      --The name of the heat map data for the run file.  This can be set to null (no map will be produced).
insert @scenarios(id) values(4);                  --Add one value for each scenario that this model supports. 1=Add a Well, 2=Remove a Well, 3=Move a Well, 4=Canal Recharge    
--- End Values to Set -----

exec dbo.UpsertModel @imageName, @modelName, @startDateTime, @modflowExeName, @namFileName, @baselineFileName, @runFileName, @zonesFileName, @nodeFlowProportionsFileName, @friendlyZoneNamesFileName, @locationMapFileName, @mapBaselineFileName, @mapRunFileName, @allowablePercentDiscrepancy, @scenarios;
END
GO
