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
-------------------------


----- Set These Values -----
--This is the name of the Docker image.  Is should be all lower case.
SET @imageName = 'widwateruse';

--This is the name of the model as it will show up in the UI.
SET @modelName = 'WID Level of GW Development';

--The start date for the model.
SET @startDateTime = '2011-01-01';

--The name of the modflow program for the model.
SET @modflowExeName = 'usgs_1.exe';

--The name of the name file that will be passed to modflow.
SET @namFileName = 'gv24.nam';

--The name of the output file modflow will generate for the run.
SET @runFileName = 'gv24.dat';

--The name of the output heatmap binary file.  This can be null if @locationMapFileName is null.
SET @mapRunFileName = 'gv24.hds';

--These are the map settings to be used by google maps
SET @mapSettings = '{zoom:9,center:{lat:41.065257,lng:-101.896490},mapTypeId:"terrain"}';

--a set of points that makeup the border to be displayed on google maps
SET @mapModelArea = '[{lat:40.993499,lng:-102.199545},{lat:40.993866,lng:-102.180419},{lat:40.994230,lng:-102.161293},{lat:40.994591,lng:-102.142167},{lat:40.994948,lng:-102.123040},{lat:40.995303,lng:-102.103914},{lat:40.995654,lng:-102.084787},{lat:40.996002,lng:-102.065660},{lat:40.996346,lng:-102.046532},{lat:40.996688,lng:-102.027405},{lat:40.997026,lng:-102.008277},{lat:40.997361,lng:-101.989149},{lat:40.997693,lng:-101.970021},{lat:40.998022,lng:-101.950892},{lat:40.998347,lng:-101.931764},{lat:40.998670,lng:-101.912635},{lat:40.998989,lng:-101.893506},{lat:40.999304,lng:-101.874377},{lat:40.999617,lng:-101.855248},{lat:40.999927,lng:-101.836118},{lat:41.000233,lng:-101.816989},{lat:41.000536,lng:-101.797859},{lat:41.000836,lng:-101.778729},{lat:41.001132,lng:-101.759598},{lat:41.001426,lng:-101.740468},{lat:41.001716,lng:-101.721338},{lat:41.002003,lng:-101.702207},{lat:41.002287,lng:-101.683076},{lat:41.002567,lng:-101.663945},{lat:41.002844,lng:-101.644814},{lat:41.003119,lng:-101.625682},{lat:41.003390,lng:-101.606551},{lat:41.003657,lng:-101.587419},{lat:41.003922,lng:-101.568287},{lat:41.018415,lng:-101.568635},{lat:41.032909,lng:-101.568982},{lat:41.047403,lng:-101.569329},{lat:41.061896,lng:-101.569677},{lat:41.076390,lng:-101.570025},{lat:41.090883,lng:-101.570373},{lat:41.105377,lng:-101.570721},{lat:41.119870,lng:-101.571069},{lat:41.134364,lng:-101.571417},{lat:41.134099,lng:-101.590587},{lat:41.133830,lng:-101.609757},{lat:41.133559,lng:-101.628927},{lat:41.133284,lng:-101.648096},{lat:41.133006,lng:-101.667266},{lat:41.132725,lng:-101.686435},{lat:41.132441,lng:-101.705604},{lat:41.132153,lng:-101.724773},{lat:41.131863,lng:-101.743941},{lat:41.131569,lng:-101.763110},{lat:41.131271,lng:-101.782278},{lat:41.130971,lng:-101.801447},{lat:41.130667,lng:-101.820615},{lat:41.130361,lng:-101.839782},{lat:41.130051,lng:-101.858950},{lat:41.129737,lng:-101.878117},{lat:41.129421,lng:-101.897285},{lat:41.129101,lng:-101.916452},{lat:41.128778,lng:-101.935619},{lat:41.128452,lng:-101.954785},{lat:41.128123,lng:-101.973952},{lat:41.127790,lng:-101.993118},{lat:41.127454,lng:-102.012284},{lat:41.127116,lng:-102.031450},{lat:41.126773,lng:-102.050616},{lat:41.126428,lng:-102.069781},{lat:41.126079,lng:-102.088947},{lat:41.125728,lng:-102.108112},{lat:41.125373,lng:-102.127277},{lat:41.125014,lng:-102.146441},{lat:41.124653,lng:-102.165606},{lat:41.124288,lng:-102.184770},{lat:41.123920,lng:-102.203934},{lat:41.123549,lng:-102.223098},{lat:41.109058,lng:-102.222605},{lat:41.094567,lng:-102.222113},{lat:41.080076,lng:-102.221620},{lat:41.065585,lng:-102.221128},{lat:41.051093,lng:-102.220636},{lat:41.036602,lng:-102.220145},{lat:41.022111,lng:-102.219653},{lat:41.007620,lng:-102.219162},{lat:40.993129,lng:-102.218671},{lat:40.993499,lng:-102.199545}]';

--Does the heat map output file use double sized value (0=Single, 1=Double)
SET @isDoubleSizeHeatMapOutput = 0;

--The the maximum varience allowed in the percent discrepancy.  This can be set to null (percent discrepancy will not be verified).
SET @allowablePercentDiscrepancy = 1.0;

--Add one value for each scenario that this model supports. 1=Add a Well, 2=Remove a Well, 3=Move a Well, 4=Canal Recharge
insert @scenarios(id) values (7);

--array of zone name, zone number, and bounds defined a a set of points to draw the zone polygon. Sample at https://jsoneditoronline.org/?id=6efc0290cfe1ed97af040d8592a457da
set @zoneData = '[{"ZoneNumber":"1","Name":"Zone A","Bounds":[{"Lat":40.9577,"Lng":-100.3192},{"Lat":40.9536,"Lng":-100.2725},{"Lat":40.9121,"Lng":-100.2711},{"Lat":40.9245,"Lng":-100.3192}]},{"ZoneNumber":"2","Name":"Zone B","Bounds":[{"Lat":40.8934,"Lng":-100.0066},{"Lat":40.8851,"Lng":-99.916},{"Lat":40.8477,"Lng":-99.9435}]},{"ZoneNumber":"3","Name":"Zone C","Bounds":[{"Lat":40.8072,"Lng":-99.9154},{"Lat":40.7718,"Lng":-99.9662},{"Lat":40.7801,"Lng":-99.8358}]}]';

--Total count of stress periods for the model
set @numberOfStressPeriods = 600;

--Canal Names
set @canalData = '1950 SW Pits,1950 SW Fields,1997 SW Pits,1997 SW Fields,1997 GW Fields,Current SW Pits,Current GW Fields,Current SW Fields'
----- End Values to Set -----


----- DO NOT CHANGE -----
exec dbo.UpsertModel @imageName, @modelName, @startDateTime, @modflowExeName, @namFileName,  @runFileName, @mapRunFileName, @mapSettings, @mapModelArea, @zoneBudgetExeName, @isDoubleSizeHeatMapOutput, @allowablePercentDiscrepancy, @scenarios, @zoneData, @numberOfStressPeriods, @canalData;
-------------------------
