INSERT INTO [dbo].[Scenarios] (Id, [Name], InputControlType, ShouldSwitchSign)
	VALUES (
		(SELECT MAX(Id)+1 FROM [dbo].Scenarios), --Don't touch.  This will put in the next highest id.  If you need to get it for the model update, you can use SSMS to find the value.
		'Retire Additional Wells', --This is the name for the scenario
		2, --Input Control Type - 1==CSV Canal Upload, 2==Add Well Map, 3==Adjust Zone Slider
		1 --0 = does not change the behavior of the input control type for setting records in the well file.  1 = Switches the sign vs the usual behavior for the input control type
	);
	
INSERT INTO [dbo].[Scenarios] (Id, [Name], InputControlType, ShouldSwitchSign)
	VALUES (
		(SELECT MAX(Id)+1 FROM [dbo].Scenarios), --Don't touch.  This will put in the next highest id.  If you need to get it for the model update, you can use SSMS to find the value.
		'Specify Pumping', --This is the name for the scenario
		1, --Input Control Type - 1==CSV Canal Upload, 2==Add Well Map, 3==Adjust Zone Slider
		1 --0 = does not change the behavior of the input control type for setting records in the well file.  1 = Switches the sign vs the usual behavior for the input control type
	);

	--Assumptions
--1) All docker images are on the same server
--2) At least one image has been setup already in the database
--3) Cannot change image name or model name


----- DO NOT CHANGE -----
DECLARE @imageName NVARCHAR(256);
DECLARE @modelName NVARCHAR(256);
DECLARE @startDateTime DATETIME;
DECLARE @modflowExeName VARCHAR(50);
DECLARE @namFileName VARCHAR(50);
DECLARE @runFileName VARCHAR(50);
DECLARE @mapRunFileName VARCHAR(50);
DECLARE @mapSettings VARCHAR(1024);
DECLARE @mapModelArea VARCHAR(MAX);
DECLARE @mapZone VARCHAR(MAX);
DECLARE @zoneBudgetExeName VARCHAR(50);
DECLARE @isDoubleSizeHeatMapOutput BIT;
DECLARE @allowablePercentDiscrepancy FLOAT;
DECLARE @scenarios as dbo.ScenariosList;
DECLARE @numberOfStressPeriods int;
DECLARE @canalData VARCHAR(MAX);
-------------------------


----- Set These Values -----
SET @modelName = 'NCORPE Augmentation Pumping';
----- End Values to Set -----


----- DO NOT CHANGE -----
exec dbo.RetrieveModel @modelName, @imageName OUT, @startDateTime OUT, @modflowExeName OUT, @namFileName OUT, @runFileName OUT, @mapRunFileName OUT, @mapSettings OUT, @mapModelArea OUT, @zoneBudgetExeName OUT, @isDoubleSizeHeatMapOutput OUT, @allowablePercentDiscrepancy OUT, @mapZone OUT, @numberOfStressPeriods out, @canalData out;
-------------------------


----- Set Only The Values That Need Changes and Always Set @scenarios -----
----- Any variables from above can be set except imageName and modelName -----
----- Descriptions for these values can be found in InsertModel.sql   -----
set @canalData = '13-2,13-3,13-4,14-3,14-4,15-4,16-1,16-3,16-4,17-1,17-2,17-3,17-4,18-1,18-2,18-3,18-4,19-1,19-2,20-1,20-2,21-1,21-2,21-3,22-2,23-1,23-2,24-1,24-2,28-1'
insert @scenarios(id) values(4),(6),(7);
----- End Values to Set -----


----- DO NOT CHANGE -----
exec dbo.UpsertModel @imageName, @modelName, @startDateTime, @modflowExeName, @namFileName, @runFileName, @mapRunFileName, @mapSettings, @mapModelArea, @zoneBudgetExeName, @isDoubleSizeHeatMapOutput, @allowablePercentDiscrepancy, @scenarios, @mapZone, @numberOfStressPeriods ,@canalData ;
-------------------------