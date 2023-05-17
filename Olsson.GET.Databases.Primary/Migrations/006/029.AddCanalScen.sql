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
SET @modelName = 'CENEB';
----- End Values to Set -----


----- DO NOT CHANGE -----
exec dbo.RetrieveModel @modelName, @imageName OUT, @startDateTime OUT, @modflowExeName OUT, @namFileName OUT, @runFileName OUT, @mapRunFileName OUT, @mapSettings OUT, @mapModelArea OUT, @zoneBudgetExeName OUT, @isDoubleSizeHeatMapOutput OUT, @allowablePercentDiscrepancy OUT, @mapZone OUT, @numberOfStressPeriods out, @canalData out;
-------------------------


----- Set Only The Values That Need Changes and Always Set @scenarios -----
----- Any variables from above can be set except imageName and modelName -----
----- Descriptions for these values can be found in InsertModel.sql   -----
set @canalData = 'Burwell-Sumter,Elba,Farwell Central,Farwell Main,Farwell South,Fullerton,Fullerton Canal,Geranium,Kent Canal,Loup Public Power Canal (Upper),Middle Loup No. 1,Middle Loup No. 2,Middle Loup No. 3,Middle Loup No. 4,Mirdan,Ord-North Loup,Sargent Canal,Scotia,Sherman Feeder,Taylor-Ord'
insert @scenarios(id) values(1),(5),(4);
----- End Values to Set -----


----- DO NOT CHANGE -----
exec dbo.UpsertModel @imageName, @modelName, @startDateTime, @modflowExeName, @namFileName, @runFileName, @mapRunFileName, @mapSettings, @mapModelArea, @zoneBudgetExeName, @isDoubleSizeHeatMapOutput, @allowablePercentDiscrepancy, @scenarios, @mapZone, @numberOfStressPeriods ,@canalData ;
-------------------------