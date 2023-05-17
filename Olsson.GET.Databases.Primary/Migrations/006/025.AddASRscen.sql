INSERT INTO [dbo].[Scenarios] (Id, [Name], InputControlType, ShouldSwitchSign)
	VALUES (
		(SELECT MAX(Id)+1 FROM [dbo].Scenarios), --Don't touch.  This will put in the next highest id.  If you need to get it for the model update, you can use SSMS to find the value.
		'ASR Wells', --This is the name for the scenario
		1, --Input Control Type - 1==CSV Canal Upload, 2==Add Well Map, 3==Adjust Zone Slider
		0 --0 = does not change the behavior of the input control type for setting records in the well file.  1 = Switches the sign vs the usual behavior for the input control type
	);
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
SET @modelName = 'Spanish Springs';
----- End Values to Set -----


----- DO NOT CHANGE -----
exec dbo.RetrieveModel @modelName, @imageName OUT, @startDateTime OUT, @modflowExeName OUT, @namFileName OUT, @runFileName OUT, @mapRunFileName OUT, @mapSettings OUT, @mapModelArea OUT, @zoneBudgetExeName OUT, @isDoubleSizeHeatMapOutput OUT, @allowablePercentDiscrepancy OUT, @mapZone OUT, @numberOfStressPeriods out, @canalData out;
-------------------------


----- Set Only The Values That Need Changes and Always Set @scenarios -----
----- Any variables from above can be set except imageName and modelName -----
----- Descriptions for these values can be found in InsertModel.sql   -----
insert @scenarios(id) values(4),(8);
----- End Values to Set -----


----- DO NOT CHANGE -----
exec dbo.UpsertModel @imageName, @modelName, @startDateTime, @modflowExeName, @namFileName, @runFileName, @mapRunFileName, @mapSettings, @mapModelArea, @zoneBudgetExeName, @isDoubleSizeHeatMapOutput, @allowablePercentDiscrepancy, @scenarios, @mapZone, @numberOfStressPeriods ,@canalData ;
-------------------------