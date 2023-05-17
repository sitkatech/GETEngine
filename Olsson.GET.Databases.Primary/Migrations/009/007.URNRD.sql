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
SET @imageName = 'urnrdc';

--This is the name of the model as it will show up in the UI.
SET @modelName = 'URNRD Calibration';

--The start date for the model.
SET @startDateTime = '1955-01-01';

--The name of the modflow program for the model.
SET @modflowExeName = 'mf2005.exe';

--The name of the name file that will be passed to modflow.
SET @namFileName = 'Base_days.nam';

--The name of the output file modflow will generate for the run.
SET @runFileName = null

--The name of the output heatmap binary file.  This can be null if @locationMapFileName is null.
SET @mapRunFileName = 'Base.hds';

--These are the map settings to be used by google maps
SET @mapSettings = '{zoom:10,center:{lat:40.588986,lng:-101.339220},mapTypeId:"terrain"}';

--a set of points that makeup the border to be displayed on google maps
SET @mapModelArea = '[{lat:41.124422,lng:-101.153393},{lat:41.138909,lng:-101.153867},{lat:41.153396,lng:-101.154341},{lat:41.153035,lng:-101.173507},{lat:41.152672,lng:-101.192672},{lat:41.152305,lng:-101.211838},{lat:41.151936,lng:-101.231003},{lat:41.137449,lng:-101.230512},{lat:41.137077,lng:-101.249672},{lat:41.136701,lng:-101.268832},{lat:41.122215,lng:-101.268333},{lat:41.121836,lng:-101.287488},{lat:41.121454,lng:-101.306643},{lat:41.121068,lng:-101.325798},{lat:41.120680,lng:-101.344952},{lat:41.120288,lng:-101.364106},{lat:41.119893,lng:-101.383260},{lat:41.119495,lng:-101.402413},{lat:41.119094,lng:-101.421566},{lat:41.133578,lng:-101.422099},{lat:41.133174,lng:-101.441255},{lat:41.132766,lng:-101.460411},{lat:41.132355,lng:-101.479567},{lat:41.117871,lng:-101.479022},{lat:41.117457,lng:-101.498173},{lat:41.131941,lng:-101.498723},{lat:41.131523,lng:-101.517878},{lat:41.131103,lng:-101.537032},{lat:41.130679,lng:-101.556186},{lat:41.130252,lng:-101.575340},{lat:41.129822,lng:-101.594494},{lat:41.129389,lng:-101.613647},{lat:41.128953,lng:-101.632799},{lat:41.128513,lng:-101.651951},{lat:41.128070,lng:-101.671103},{lat:41.127624,lng:-101.690254},{lat:41.127175,lng:-101.709405},{lat:41.126723,lng:-101.728556},{lat:41.126267,lng:-101.747706},{lat:41.125809,lng:-101.766855},{lat:41.111328,lng:-101.766247},{lat:41.110867,lng:-101.785392},{lat:41.110402,lng:-101.804536},{lat:41.109934,lng:-101.823680},{lat:41.109463,lng:-101.842824},{lat:41.094983,lng:-101.842199},{lat:41.094509,lng:-101.861338},{lat:41.094032,lng:-101.880476},{lat:41.093551,lng:-101.899614},{lat:41.093068,lng:-101.918752},{lat:41.092581,lng:-101.937889},{lat:41.092091,lng:-101.957025},{lat:41.077613,lng:-101.956376},{lat:41.077120,lng:-101.975508},{lat:41.076624,lng:-101.994639},{lat:41.076125,lng:-102.013770},{lat:41.061647,lng:-102.013108},{lat:41.061145,lng:-102.032235},{lat:41.060640,lng:-102.051360},{lat:41.046163,lng:-102.050691},{lat:41.045655,lng:-102.069812},{lat:41.045143,lng:-102.088933},{lat:41.030667,lng:-102.088256},{lat:41.030153,lng:-102.107372},{lat:41.015677,lng:-102.106691},{lat:41.015159,lng:-102.125802},{lat:41.014639,lng:-102.144913},{lat:41.000163,lng:-102.144224},{lat:40.999640,lng:-102.163330},{lat:40.999114,lng:-102.182436},{lat:40.984638,lng:-102.181739},{lat:40.984109,lng:-102.200840},{lat:40.969634,lng:-102.200140},{lat:40.955159,lng:-102.199440},{lat:40.940684,lng:-102.198740},{lat:40.926209,lng:-102.198042},{lat:40.911734,lng:-102.197343},{lat:40.897258,lng:-102.196646},{lat:40.882783,lng:-102.195948},{lat:40.868308,lng:-102.195252},{lat:40.853833,lng:-102.194555},{lat:40.839357,lng:-102.193860},{lat:40.824882,lng:-102.193164},{lat:40.810406,lng:-102.192470},{lat:40.795931,lng:-102.191775},{lat:40.781455,lng:-102.191082},{lat:40.766979,lng:-102.190389},{lat:40.752504,lng:-102.189696},{lat:40.738028,lng:-102.189004},{lat:40.723552,lng:-102.188312},{lat:40.709076,lng:-102.187621},{lat:40.694601,lng:-102.186930},{lat:40.680125,lng:-102.186240},{lat:40.665649,lng:-102.185550},{lat:40.651173,lng:-102.184861},{lat:40.636697,lng:-102.184172},{lat:40.622221,lng:-102.183484},{lat:40.607744,lng:-102.182796},{lat:40.593268,lng:-102.182109},{lat:40.578792,lng:-102.181422},{lat:40.564316,lng:-102.180736},{lat:40.549839,lng:-102.180051},{lat:40.535363,lng:-102.179365},{lat:40.520887,lng:-102.178681},{lat:40.506410,lng:-102.177996},{lat:40.491934,lng:-102.177313},{lat:40.477457,lng:-102.176629},{lat:40.462980,lng:-102.175947},{lat:40.448504,lng:-102.175264},{lat:40.434027,lng:-102.174583},{lat:40.419550,lng:-102.173902},{lat:40.405073,lng:-102.173221},{lat:40.390597,lng:-102.172541},{lat:40.376120,lng:-102.171861},{lat:40.361643,lng:-102.171182},{lat:40.347166,lng:-102.170503},{lat:40.332689,lng:-102.169824},{lat:40.318212,lng:-102.169147},{lat:40.303735,lng:-102.168469},{lat:40.289257,lng:-102.167793},{lat:40.274780,lng:-102.167116},{lat:40.260303,lng:-102.166440},{lat:40.245826,lng:-102.165765},{lat:40.231348,lng:-102.165090},{lat:40.216871,lng:-102.164416},{lat:40.202393,lng:-102.163742},{lat:40.187916,lng:-102.163069},{lat:40.173438,lng:-102.162396},{lat:40.158961,lng:-102.161723},{lat:40.144483,lng:-102.161051},{lat:40.130006,lng:-102.160380},{lat:40.115528,lng:-102.159709},{lat:40.101050,lng:-102.159039},{lat:40.086572,lng:-102.158369},{lat:40.072094,lng:-102.157699},{lat:40.057617,lng:-102.157030},{lat:40.043139,lng:-102.156362},{lat:40.028661,lng:-102.155693},{lat:40.014183,lng:-102.155026},{lat:39.999704,lng:-102.154359},{lat:39.985226,lng:-102.153692},{lat:39.970748,lng:-102.153026},{lat:39.956270,lng:-102.152361},{lat:39.941792,lng:-102.151695},{lat:39.927313,lng:-102.151031},{lat:39.912835,lng:-102.150367},{lat:39.913345,lng:-102.131566},{lat:39.913852,lng:-102.112764},{lat:39.914355,lng:-102.093962},{lat:39.914856,lng:-102.075160},{lat:39.915353,lng:-102.056357},{lat:39.915848,lng:-102.037554},{lat:39.916340,lng:-102.018750},{lat:39.916828,lng:-101.999945},{lat:39.917313,lng:-101.981141},{lat:39.917796,lng:-101.962336},{lat:39.918275,lng:-101.943530},{lat:39.918751,lng:-101.924724},{lat:39.919225,lng:-101.905917},{lat:39.919695,lng:-101.887110},{lat:39.920162,lng:-101.868303},{lat:39.920626,lng:-101.849495},{lat:39.921087,lng:-101.830687},{lat:39.921545,lng:-101.811878},{lat:39.922000,lng:-101.793069},{lat:39.922452,lng:-101.774260},{lat:39.922901,lng:-101.755450},{lat:39.923347,lng:-101.736639},{lat:39.923790,lng:-101.717828},{lat:39.924229,lng:-101.699017},{lat:39.924666,lng:-101.680205},{lat:39.925100,lng:-101.661393},{lat:39.925530,lng:-101.642581},{lat:39.925958,lng:-101.623768},{lat:39.926382,lng:-101.604955},{lat:39.926804,lng:-101.586141},{lat:39.927222,lng:-101.567327},{lat:39.927638,lng:-101.548513},{lat:39.928050,lng:-101.529698},{lat:39.928459,lng:-101.510883},{lat:39.928865,lng:-101.492067},{lat:39.929268,lng:-101.473251},{lat:39.929669,lng:-101.454435},{lat:39.930066,lng:-101.435618},{lat:39.930460,lng:-101.416801},{lat:39.930851,lng:-101.397983},{lat:39.931238,lng:-101.379165},{lat:39.931623,lng:-101.360347},{lat:39.932005,lng:-101.341529},{lat:39.932384,lng:-101.322710},{lat:39.932759,lng:-101.303890},{lat:39.933132,lng:-101.285071},{lat:39.933502,lng:-101.266251},{lat:39.933868,lng:-101.247431},{lat:39.934232,lng:-101.228610},{lat:39.934592,lng:-101.209789},{lat:39.934949,lng:-101.190967},{lat:39.935304,lng:-101.172146},{lat:39.935655,lng:-101.153324},{lat:39.936003,lng:-101.134501},{lat:39.936348,lng:-101.115679},{lat:39.950839,lng:-101.116125},{lat:39.965329,lng:-101.116572},{lat:39.979819,lng:-101.117019},{lat:39.994310,lng:-101.117467},{lat:40.008800,lng:-101.117915},{lat:40.023290,lng:-101.118363},{lat:40.037780,lng:-101.118811},{lat:40.052270,lng:-101.119260},{lat:40.066760,lng:-101.119709},{lat:40.081250,lng:-101.120159},{lat:40.095740,lng:-101.120609},{lat:40.110230,lng:-101.121059},{lat:40.124720,lng:-101.121509},{lat:40.139210,lng:-101.121960},{lat:40.153700,lng:-101.122411},{lat:40.168189,lng:-101.122862},{lat:40.182679,lng:-101.123314},{lat:40.197169,lng:-101.123766},{lat:40.211658,lng:-101.124219},{lat:40.226148,lng:-101.124671},{lat:40.240637,lng:-101.125124},{lat:40.255127,lng:-101.125578},{lat:40.269616,lng:-101.126031},{lat:40.284106,lng:-101.126485},{lat:40.298595,lng:-101.126940},{lat:40.313085,lng:-101.127395},{lat:40.327574,lng:-101.127850},{lat:40.342063,lng:-101.128305},{lat:40.356552,lng:-101.128761},{lat:40.371042,lng:-101.129217},{lat:40.385531,lng:-101.129673},{lat:40.400020,lng:-101.130130},{lat:40.414509,lng:-101.130587},{lat:40.428998,lng:-101.131044},{lat:40.443487,lng:-101.131502},{lat:40.457976,lng:-101.131960},{lat:40.472465,lng:-101.132418},{lat:40.486953,lng:-101.132877},{lat:40.501442,lng:-101.133336},{lat:40.515931,lng:-101.133795},{lat:40.530420,lng:-101.134255},{lat:40.544908,lng:-101.134715},{lat:40.559397,lng:-101.135175},{lat:40.573886,lng:-101.135636},{lat:40.588374,lng:-101.136097},{lat:40.602863,lng:-101.136558},{lat:40.617351,lng:-101.137020},{lat:40.631840,lng:-101.137482},{lat:40.646328,lng:-101.137944},{lat:40.660816,lng:-101.138407},{lat:40.675305,lng:-101.138870},{lat:40.689793,lng:-101.139333},{lat:40.704281,lng:-101.139797},{lat:40.718769,lng:-101.140261},{lat:40.733257,lng:-101.140726},{lat:40.747746,lng:-101.141190},{lat:40.762234,lng:-101.141655},{lat:40.776722,lng:-101.142121},{lat:40.791210,lng:-101.142587},{lat:40.805698,lng:-101.143053},{lat:40.820185,lng:-101.143519},{lat:40.834673,lng:-101.143986},{lat:40.849161,lng:-101.144453},{lat:40.863649,lng:-101.144920},{lat:40.878137,lng:-101.145388},{lat:40.892624,lng:-101.145856},{lat:40.907112,lng:-101.146325},{lat:40.921600,lng:-101.146793},{lat:40.936087,lng:-101.147263},{lat:40.950575,lng:-101.147732},{lat:40.965062,lng:-101.148202},{lat:40.979550,lng:-101.148672},{lat:40.994037,lng:-101.149143},{lat:41.008524,lng:-101.149613},{lat:41.023012,lng:-101.150085},{lat:41.037499,lng:-101.150556},{lat:41.051986,lng:-101.151028},{lat:41.066473,lng:-101.151500},{lat:41.080961,lng:-101.151973},{lat:41.095448,lng:-101.152446},{lat:41.109935,lng:-101.152919},{lat:41.124422,lng:-101.153393}]';

--the name of the zone budget executeable.  This can be null if we do not want to generate the zone budget data.
SET @zoneBudgetExeName = null

--Does the heat map output file use double sized value (0=Single, 1=Double)
SET @isDoubleSizeHeatMapOutput = 0;

--The the maximum varience allowed in the percent discrepancy.  This can be set to null (percent discrepancy will not be verified).
SET @allowablePercentDiscrepancy = 1.0;

--Add one value for each scenario that this model supports. 1=Add a Well, 2=Remove a Well, 3=Move a Well, 4=Canal Recharge, 5=Adjust Zone, 6=Retire Additional Wells, 7=Specify Pumping, 8 = ASR, 9 = Adjust Pumping, 10 = particle track
insert @scenarios(id) values(1);

--array of zone name, zone number, and bounds defined a a set of points to draw the zone polygon. Sample at https://jsoneditoronline.org/?id=6efc0290cfe1ed97af040d8592a457da
set @zoneData = null

--Total count of stress periods for the model
set @numberOfStressPeriods = 756;

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