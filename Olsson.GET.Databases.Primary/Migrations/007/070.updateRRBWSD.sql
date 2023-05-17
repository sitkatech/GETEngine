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
DECLARE @zoneData VARCHAR(MAX);
DECLARE @zoneBudgetExeName VARCHAR(50);
DECLARE @isDoubleSizeHeatMapOutput BIT;
DECLARE @allowablePercentDiscrepancy FLOAT;
DECLARE @scenarios as dbo.ScenariosList;
DECLARE @numberOfStressPeriods int;
DECLARE @canalData VARCHAR(MAX);
DECLARE @modPathExeName VARCHAR(50);
DECLARE @simulationFileName VARCHAR(50);
-------------------------


----- Set These Values -----
SET @modelName = 'RRBWSD';
----- End Values to Set -----


----- DO NOT CHANGE -----
exec dbo.RetrieveModel @modelName, @imageName OUT, @startDateTime OUT, @modflowExeName OUT, @namFileName OUT, @runFileName OUT, @mapRunFileName OUT, @mapSettings OUT, @mapModelArea OUT, @zoneBudgetExeName OUT, @isDoubleSizeHeatMapOutput OUT, @allowablePercentDiscrepancy OUT, @zoneData OUT, @numberOfStressPeriods out, @canalData out, @modPathExeName out, @simulationFileName out ;
-------------------------



----- Set Only The Values That Need Changes and Always Set @scenarios -----
----- Any variables from above can be set except imageName and modelName -----
----- Descriptions for these values can be found in InsertModel.sql   -----
set @canalData = 'Blacco Canal,Goose Lake Slough,Kern River ,McCaslin Canal,Rosedale Turnout No. 1 Channel,Rosedale Turnout No. 2 Channel,Strand Channel,West Basin Channel,Allen Pool,Blacco,Cell,E. Superior,Enns,Mayer Basin,S. Allen,SE Pond,SW Basin,Selvidge,Strand Basin,Superior Pool,W. Superior'

insert @scenarios(id) values(1),(4);
----- End Values to Set -----


----- DO NOT CHANGE -----
exec dbo.UpsertModel @imageName, @modelName, @startDateTime, @modflowExeName, @namFileName, @runFileName, @mapRunFileName, @mapSettings, @mapModelArea, @zoneBudgetExeName, @isDoubleSizeHeatMapOutput, @allowablePercentDiscrepancy, @scenarios, @zoneData, @numberOfStressPeriods ,@canalData, @modPathExeName, @simulationFileName;
-------------------------