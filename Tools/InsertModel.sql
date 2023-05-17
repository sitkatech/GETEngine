/*Assumptions
1) All docker images are on the same server
2) At least one image has been setup already in the database
3) Cannot change image name or model name
*/

/*----- DO NOT CHANGE -----*/
DECLARE @imageName NVARCHAR(256);
DECLARE @modelName NVARCHAR(256);
DECLARE @startDateTime DATETIME;
DECLARE @runFileName VARCHAR(50);
DECLARE @mapRunFileName VARCHAR(50);
DECLARE @mapDrawdownFileName VARCHAR(50);
DECLARE @mapSettings VARCHAR(1024);
DECLARE @mapModelArea VARCHAR(MAX);
DECLARE @mapInputZone VARCHAR(MAX);
DECLARE @mapOutputZone VARCHAR(MAX);
DECLARE @isDoubleSizeHeatMapOutput BIT;
DECLARE @allowablePercentDiscrepancy FLOAT;
DECLARE @numberOfStressPeriods int;
DECLARE @canalData VARCHAR(MAX);
DECLARE @listFileName VARCHAR(50);
DECLARE @baseflowTableProcessingConfigurationID INT;
/*-------------------------*/

/*----- Set These Values -----*/
/*This is the name of the Docker image.  Is should be all lower case.*/
SET @imageName = 'dockerimagename';

/*This is the name of the model as it will show up in the UI.*/
SET @modelName = 'New Test';

/*The start date for the model.*/
SET @startDateTime = '2017-11-21';

/*The name of the output file modflow will generate for the run.*/
SET @runFileName = 'output.dat';

/*The name of the output heatmap binary file.  This can be null if @locationMapFileName is null.*/
SET @mapRunFileName = 'CPNRD.hds';

/*The name of the output heatmap binary file.  This can be null. */
SET @mapDrawdownFileName = 'CPNRD_DRAWDOWN.hds';

/*These are the map settings to be used by google maps*/
SET @mapSettings = '{zoom:8,center:{lat:40.8876131,lng:-100.0892906},mapTypeId:"terrain"}';

/*a set of points that makeup the border to be displayed on google maps*/
SET @mapModelArea = '[{lat:41.0213531047554,lng:-100.367575873715},{lat:41.0213375972734,lng:-100.372360865754}]';

/*Does the heat map output file use double sized value (0=Single, 1=Double)*/
SET @isDoubleSizeHeatMapOutput = 0;

/*The the maximum varience allowed in the percent discrepancy.  This can be set to null (percent discrepancy will not be verified).*/
SET @allowablePercentDiscrepancy = 1.0;

/*array of zone name, zone number, and bounds defined a a set of points to draw the zone polygon. Sample at https://jsoneditoronline.org/?id=6efc0290cfe1ed97af040d8592a457da*/
set @mapInputZone = '[{"ZoneNumber":"1","Name":"Zone A","Bounds":[{"Lat":40.9577,"Lng":-100.3192},{"Lat":40.9536,"Lng":-100.2725},{"Lat":40.9121,"Lng":-100.2711},{"Lat":40.9245,"Lng":-100.3192}]},{"ZoneNumber":"2","Name":"Zone B","Bounds":[{"Lat":40.8934,"Lng":-100.0066},{"Lat":40.8851,"Lng":-99.916},{"Lat":40.8477,"Lng":-99.9435}]},{"ZoneNumber":"3","Name":"Zone C","Bounds":[{"Lat":40.8072,"Lng":-99.9154},{"Lat":40.7718,"Lng":-99.9662},{"Lat":40.7801,"Lng":-99.8358}]}]';

/*array of zone name, zone number, and bounds defined a a set of points to draw the zone polygon. This will be used to generate Zones for any output Maps that include Zones Sample at https://jsoneditoronline.org/?id=6efc0290cfe1ed97af040d8592a457da*/
set @mapOutputZone = '[{"ZoneNumber":"1","Name":"Zone A","Bounds":[{"Lat":40.9577,"Lng":-100.3192},{"Lat":40.9536,"Lng":-100.2725},{"Lat":40.9121,"Lng":-100.2711},{"Lat":40.9245,"Lng":-100.3192}]},{"ZoneNumber":"2","Name":"Zone B","Bounds":[{"Lat":40.8934,"Lng":-100.0066},{"Lat":40.8851,"Lng":-99.916},{"Lat":40.8477,"Lng":-99.9435}]},{"ZoneNumber":"3","Name":"Zone C","Bounds":[{"Lat":40.8072,"Lng":-99.9154},{"Lat":40.7718,"Lng":-99.9662},{"Lat":40.7801,"Lng":-99.8358}]}]';

/*Total count of stress periods for the model*/
set @numberOfStressPeriods = 3;

/*Canal Names*/
set @canalData = 'canal 1,canal 2,canal 3'

/*Modflow 6 List File name*/
set @listFileName = 'test.lst'

/*Let's GET know how to get the values to calculate Baseflow and Impacts to Baseflow.
If left null, Baseflow will not be calculated. Run the following in a separate window
to see the values currently stored in the database:

		select *
		from dbo.BaseflowTableProcessingConfiguration

BaseflowTableIndicatorRegexPattern should match the line that exists BEFORE the dashed lines
that indicate the table headers for the table we are grabbing values from. This website can 
be helpful in checking if your indicator matches any of the patterns https://regex101.com/.
Also ensure that the column numbers match where the values will be located. For anything before
Modflow6, Segment and Reach column num should be defined. Anything Modflow6 and later will have the 
ReachColumnNum as null.
If you need to add a new pattern, consult the 'InsertBaseflowTableProcessingConfiguration.sql' script.
If the ID sent in is not null and does not match an ID present in the BaseflowTableProcessingConfiguration table, the insert will fail.
*/
set @baseflowTableProcessingConfigurationID = null

/*Add one value for each stress period IF the model does not have monthly stress periods. If the number of dates inserted here does not match the number of stress periods defined above, dates will not be inserted and monthly dates will be assumed*/
--insert @customStartDatesForStressPeriods([Date]) values('2014-01-01'),('2015-01-01'),('2016-01-01');

/*----- End Values to Set -----*/


DECLARE @imageId INT;
DECLARE @modelID INT;

SELECT @imageId = ImageID FROM [Image] WHERE ImageName = @imageName
IF @imageId is null
BEGIN
	declare @ID table (ID int);
	insert [Image](ImageID, ImageName, [Server])
	output inserted.ImageID into @ID
	values ((select (max(ImageID) + 1) from [Image]),@imageName,(select top 1 [server] from [Image]));
	SELECT @imageId = ID FROM @ID;
END

declare @modelEngineTypeID int, @modelGridTypeID int

-- See dbo.ModelEngineType -- 1 - Modpath, 2 - Modflow, 3 - Modflow6, 4 - IWFM
set @modelEngineTypeID = 1
-- See dbo.ModelGridType -- 1 - Structured, 2 - Unstructured
set @modelGridTypeID = 1

INSERT into dbo.Model(ModelID, ModelName, StartDateTime, ModelEngineTypeID, ModelGridTypeID, RunFileName, AllowablePercentDiscrepancy, MapRunFileName, MapDrawdownFileName, IsDoubleSizeHeatMapOutput, MapSettings, ImageId, NumberOfStressPeriods, CanalData, ListFileName, BaseflowTableProcessingConfigurationID) 
VALUES ((select (max(ModelID) + 1) from Model), @modelName, @startDateTime, @modelEngineTypeID, @modelGridTypeID, @runFileName, @allowablePercentDiscrepancy, @mapRunFileName, @mapDrawdownFileName, @isDoubleSizeHeatMapOutput, @mapSettings, @imageId, @numberOfStressPeriods, @canalData, @listFileName, @baseflowTableProcessingConfigurationID);

SELECT @modelID = ModelID FROM Model WHERE ModelName = @modelName;

INSERT INTO dbo.ModelExecutable(ModelID, ExecutableName, Arguments, RunOrder, WorkingDirectory, WrapWithBatchFile, UseShellExecute, RedirectStandardOutput, CreateNoWindow)
VALUES 
-- UseShellExecute and RedirectStandardOuptut should never change
-- (@modelID, 'exeName', 'args for exe', [run order of exes], 'working directory if not root (needs to be relative to it)', 0 (unless it's the zonebud.exe, where it wraps it with a dummy bat), 0, 1, 0 (modpath7 seems to set CreateNewWindow to 1))
(@modelID, 'usgs_1.exe', 'test.nam', 10, null, 0, 0, 1, 0),
(@modelID, 'zonebud.exe', null, 20, null, 0, 0, 1, 0)
    
INSERT dbo.ModelScenario (ModelId, ScenarioId) 
VALUES 
/*Add one value for each scenario that this model supports.  See dbo.Scenario table for full list
// e.g 1=Add a Well, 2=Remove a Well, 3=Move a Well, 4=Canal Recharge, 5=Adjust Zone, 6=Retire Additional Wells, 7=Specify Pumping, 8 = ASR, 9 = Adjust Pumping, 10 = particle track*/
(@modelID, 1),
(@modelID, 4)

insert into dbo.ModelMapAreaBoundary(ModelID, MapAreaBoundary) VALUES (@modelID, @mapModelArea)
insert into dbo.ModelInputZoneData (ModelID, InputZoneData) VALUES (@modelID, @mapInputZone)
insert into dbo.ModelOutputZoneData (ModelID, OutputZoneData) VALUES (@modelID, @mapOutputZone)

/*
-- if you have custom start dates for your stress periods, uncomments this section and enter your dates in order in format below
DELETE from dbo.ModelStressPeriodCustomStartDate where ModelID = @modelID
		
INSERT INTO dbo.ModelStressPeriodCustomStartDate(ModelID, StressPeriod, StressPeriodStartDate)
values
/*Add one value for each stress period IF the model does not have monthly stress periods. If the number of dates inserted here does not match the number of stress periods defined above, dates will not be inserted and monthly dates will be assumed*/
(@modelID, 1, '2014-01-01'),
(@modelID, 2, '2015-01-01'),
(@modelID, 3, '2016-01-01')
*/
