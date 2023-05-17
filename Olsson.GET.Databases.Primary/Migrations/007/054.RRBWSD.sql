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
--This is the name of the Docker image.  Is should be all lower case.
SET @imageName = 'rrbwsd';

--This is the name of the model as it will show up in the UI.
SET @modelName = 'RRBWSD';

--The start date for the model.
SET @startDateTime = '2020-01-01';

--The name of the modflow program for the model.
SET @modflowExeName = 'MODFLOW-NWT_64.exe';

--The name of the name file that will be passed to modflow.
SET @namFileName = '2018_Update_UZF.nam';

--The name of the output file modflow will generate for the run.
SET @runFileName = 'SFRout.dat';

--The name of the output heatmap binary file.  This can be null if @locationMapFileName is null.
SET @mapRunFileName = '2018_Update_UZF.hds';

--These are the map settings to be used by google maps
SET @mapSettings = '{zoom:11,center:{lat:35.348782,lng:-119.256059},mapTypeId:"terrain"}';

--a set of points that makeup the border to be displayed on google maps
SET @mapModelArea = '[{lat:35.303038,lng:-119.064147},{lat:35.444668,lng:-119.126628},{lat:35.445582,lng:-119.397731},{lat:35.423386,lng:-119.472337},{lat:35.334829,lng:-119.432802},{lat:35.333370,lng:-119.428159},{lat:35.331468,lng:-119.424326},{lat:35.329755,lng:-119.420971},{lat:35.327433,lng:-119.419743},{lat:35.324728,lng:-119.418034},{lat:35.323977,lng:-119.415646},{lat:35.324789,lng:-119.412810},{lat:35.325019,lng:-119.409727},{lat:35.323692,lng:-119.406617},{lat:35.322571,lng:-119.402560},{lat:35.321441,lng:-119.399216},{lat:35.320900,lng:-119.395407},{lat:35.320530,lng:-119.393739},{lat:35.318033,lng:-119.390847},{lat:35.315146,lng:-119.387949},{lat:35.313435,lng:-119.384358},{lat:35.312302,lng:-119.381252},{lat:35.311550,lng:-119.378865},{lat:35.309820,lng:-119.376936},{lat:35.307130,lng:-119.373804},{lat:35.306181,lng:-119.371652},{lat:35.305829,lng:-119.368322},{lat:35.304688,lng:-119.365928},{lat:35.303072,lng:-119.361849},{lat:35.302598,lng:-119.361638},{lat:35.302835,lng:-119.360847},{lat:35.301973,lng:-119.359017},{lat:35.301205,lng:-119.357361},{lat:35.300641,lng:-119.355924},{lat:35.300400,lng:-119.354678},{lat:35.300284,lng:-119.353168},{lat:35.300215,lng:-119.351146},{lat:35.300229,lng:-119.349882},{lat:35.299712,lng:-119.348944},{lat:35.298618,lng:-119.347504},{lat:35.298039,lng:-119.346830},{lat:35.297026,lng:-119.345651},{lat:35.296725,lng:-119.345131},{lat:35.296211,lng:-119.344246},{lat:35.295647,lng:-119.343272},{lat:35.295250,lng:-119.342271},{lat:35.294878,lng:-119.341331},{lat:35.294153,lng:-119.340563},{lat:35.293377,lng:-119.340101},{lat:35.292257,lng:-119.340003},{lat:35.291569,lng:-119.339602},{lat:35.291123,lng:-119.339342},{lat:35.290259,lng:-119.338042},{lat:35.289969,lng:-119.336790},{lat:35.289588,lng:-119.335139},{lat:35.289417,lng:-119.334303},{lat:35.289171,lng:-119.333097},{lat:35.287872,lng:-119.331362},{lat:35.286847,lng:-119.330182},{lat:35.285915,lng:-119.329110},{lat:35.284662,lng:-119.327668},{lat:35.283759,lng:-119.326644},{lat:35.282840,lng:-119.325603},{lat:35.282256,lng:-119.325481},{lat:35.281693,lng:-119.325364},{lat:35.280565,lng:-119.324817},{lat:35.280151,lng:-119.324259},{lat:35.279440,lng:-119.323299},{lat:35.278421,lng:-119.321623},{lat:35.278059,lng:-119.321028},{lat:35.276920,lng:-119.320795},{lat:35.275882,lng:-119.320405},{lat:35.275432,lng:-119.320236},{lat:35.274217,lng:-119.318931},{lat:35.273291,lng:-119.317467},{lat:35.271900,lng:-119.315216},{lat:35.271058,lng:-119.313822},{lat:35.269451,lng:-119.311288},{lat:35.268596,lng:-119.310702},{lat:35.267718,lng:-119.310340},{lat:35.267342,lng:-119.310185},{lat:35.266509,lng:-119.309842},{lat:35.265623,lng:-119.309477},{lat:35.264849,lng:-119.309037},{lat:35.263877,lng:-119.308485},{lat:35.263258,lng:-119.308431},{lat:35.262385,lng:-119.308355},{lat:35.261504,lng:-119.307855},{lat:35.260263,lng:-119.307152},{lat:35.259188,lng:-119.306543},{lat:35.258107,lng:-119.305931},{lat:35.256371,lng:-119.304791},{lat:35.255385,lng:-119.304144},{lat:35.254286,lng:-119.303423},{lat:35.253035,lng:-119.302602},{lat:35.252089,lng:-119.301981},{lat:35.251109,lng:-119.301917},{lat:35.249861,lng:-119.301835},{lat:35.248890,lng:-119.301771},{lat:35.247788,lng:-119.301699},{lat:35.245943,lng:-119.301782},{lat:35.244273,lng:-119.301857},{lat:35.243301,lng:-119.302590},{lat:35.242765,lng:-119.303225},{lat:35.242150,lng:-119.304607},{lat:35.241294,lng:-119.306528},{lat:35.240729,lng:-119.307797},{lat:35.240343,lng:-119.308664},{lat:35.239822,lng:-119.310244},{lat:35.239262,lng:-119.312445},{lat:35.238375,lng:-119.315933},{lat:35.237468,lng:-119.319019},{lat:35.235757,lng:-119.324529},{lat:35.233405,lng:-119.330862},{lat:35.232916,lng:-119.331778},{lat:35.232815,lng:-119.332915},{lat:35.232172,lng:-119.334748},{lat:35.231530,lng:-119.336527},{lat:35.231328,lng:-119.338693},{lat:35.231305,lng:-119.340861},{lat:35.230973,lng:-119.342699},{lat:35.229968,lng:-119.345123},{lat:35.228186,lng:-119.349377},{lat:35.226368,lng:-119.352872},{lat:35.225093,lng:-119.355454},{lat:35.223795,lng:-119.356245},{lat:35.220985,lng:-119.356850},{lat:35.218849,lng:-119.356923},{lat:35.215892,lng:-119.356961},{lat:35.216179,lng:-119.356027},{lat:35.221561,lng:-119.338528},{lat:35.303038,lng:-119.064147}]';

--the name of the zone budget executeable.  This can be null if we do not want to generate the zone budget data.
SET @zoneBudgetExeName = 'zonbud.exe';

--Does the heat map output file use double sized value (0=Single, 1=Double)
SET @isDoubleSizeHeatMapOutput = 0;

--The the maximum varience allowed in the percent discrepancy.  This can be set to null (percent discrepancy will not be verified).
SET @allowablePercentDiscrepancy = 1.0;

--Add one value for each scenario that this model supports. 1=Add a Well, 2=Remove a Well, 3=Move a Well, 4=Canal Recharge, 5=Adjust Zone, 6=Retire Additional Wells, 7=Specify Pumping, 8 = ASR, 9 = Adjust Pumping, 10 = particle track
insert @scenarios(id) values(1);

--array of zone name, zone number, and bounds defined a a set of points to draw the zone polygon. Sample at https://jsoneditoronline.org/?id=6efc0290cfe1ed97af040d8592a457da
set @zoneData = null

--Total count of stress periods for the model
set @numberOfStressPeriods = 240;

--Canal Names
set @canalData = null

--Modpath exe
set @modPathExeName = null

--Modpath simFile name
set @simulationFileName = null
----- End Values to Set -----


----- DO NOT CHANGE -----
exec dbo.UpsertModel @imageName, @modelName, @startDateTime, @modflowExeName, @namFileName,  @runFileName, @mapRunFileName, @mapSettings, @mapModelArea, @zoneBudgetExeName, @isDoubleSizeHeatMapOutput, @allowablePercentDiscrepancy, @scenarios, @zoneData, @numberOfStressPeriods, @canalData, @modPathExeName, @simulationFileName;
-------------------------