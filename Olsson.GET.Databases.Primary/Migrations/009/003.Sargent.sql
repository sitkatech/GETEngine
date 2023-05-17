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
SET @imageName = 'sargent';

--This is the name of the model as it will show up in the UI.
SET @modelName = 'Sargent';

--The start date for the model.
SET @startDateTime = '1991-01-01';

--The name of the modflow program for the model.
SET @modflowExeName = 'USGs_1.exe';

--The name of the name file that will be passed to modflow.
SET @namFileName = 'Sargent.nam';

--The name of the output file modflow will generate for the run.
SET @runFileName = null

--The name of the output heatmap binary file.  This can be null if @locationMapFileName is null.
SET @mapRunFileName = 'Sargent.hds';

--These are the map settings to be used by google maps
SET @mapSettings = '{zoom:11,center:{lat: 41.641201,lng:-99.385687},mapTypeId:"terrain"}';

--a set of points that makeup the border to be displayed on google maps
SET @mapModelArea = '[{lat:41.602740,lng:-99.278281},{lat:41.604552,lng:-99.278261},{lat:41.606364,lng:-99.278241},{lat:41.608176,lng:-99.278221},{lat:41.609988,lng:-99.278200},{lat:41.610894,lng:-99.278190},{lat:41.611800,lng:-99.278180},{lat:41.612705,lng:-99.278170},{lat:41.613611,lng:-99.278160},{lat:41.614517,lng:-99.278150},{lat:41.615423,lng:-99.278140},{lat:41.616329,lng:-99.278130},{lat:41.617235,lng:-99.278120},{lat:41.618141,lng:-99.278110},{lat:41.619047,lng:-99.278100},{lat:41.619953,lng:-99.278090},{lat:41.620859,lng:-99.278080},{lat:41.621764,lng:-99.278069},{lat:41.622670,lng:-99.278059},{lat:41.623576,lng:-99.278049},{lat:41.624482,lng:-99.278039},{lat:41.626294,lng:-99.278019},{lat:41.628106,lng:-99.277999},{lat:41.629918,lng:-99.277979},{lat:41.631729,lng:-99.277959},{lat:41.632635,lng:-99.277948},{lat:41.633541,lng:-99.277938},{lat:41.634447,lng:-99.277928},{lat:41.635353,lng:-99.277918},{lat:41.636259,lng:-99.277908},{lat:41.637165,lng:-99.277898},{lat:41.638071,lng:-99.277888},{lat:41.638977,lng:-99.277878},{lat:41.639882,lng:-99.277868},{lat:41.640788,lng:-99.277858},{lat:41.641694,lng:-99.277848},{lat:41.642600,lng:-99.277838},{lat:41.643506,lng:-99.277827},{lat:41.644412,lng:-99.277817},{lat:41.645318,lng:-99.277807},{lat:41.646224,lng:-99.277797},{lat:41.648036,lng:-99.277777},{lat:41.649847,lng:-99.277757},{lat:41.651659,lng:-99.277737},{lat:41.653471,lng:-99.277716},{lat:41.655283,lng:-99.277696},{lat:41.657095,lng:-99.277676},{lat:41.658906,lng:-99.277656},{lat:41.660718,lng:-99.277636},{lat:41.664342,lng:-99.277595},{lat:41.667965,lng:-99.277555},{lat:41.671589,lng:-99.277515},{lat:41.675212,lng:-99.277474},{lat:41.682459,lng:-99.277393},{lat:41.689707,lng:-99.277313},{lat:41.696954,lng:-99.277232},{lat:41.704201,lng:-99.277151},{lat:41.704261,lng:-99.286821},{lat:41.704320,lng:-99.296492},{lat:41.711567,lng:-99.296413},{lat:41.711626,lng:-99.306085},{lat:41.711684,lng:-99.315757},{lat:41.711741,lng:-99.325429},{lat:41.718988,lng:-99.325353},{lat:41.719044,lng:-99.335026},{lat:41.719099,lng:-99.344699},{lat:41.726346,lng:-99.344626},{lat:41.726401,lng:-99.354299},{lat:41.726455,lng:-99.363973},{lat:41.726508,lng:-99.373647},{lat:41.726560,lng:-99.383321},{lat:41.726611,lng:-99.392995},{lat:41.726661,lng:-99.402670},{lat:41.726711,lng:-99.412344},{lat:41.726760,lng:-99.422018},{lat:41.726808,lng:-99.431692},{lat:41.726855,lng:-99.441366},{lat:41.726902,lng:-99.451040},{lat:41.726947,lng:-99.460714},{lat:41.726992,lng:-99.470388},{lat:41.727036,lng:-99.480062},{lat:41.719789,lng:-99.480121},{lat:41.719832,lng:-99.489794},{lat:41.712585,lng:-99.489851},{lat:41.712627,lng:-99.499523},{lat:41.705380,lng:-99.499579},{lat:41.698133,lng:-99.499635},{lat:41.690886,lng:-99.499691},{lat:41.687262,lng:-99.499719},{lat:41.683638,lng:-99.499747},{lat:41.681826,lng:-99.499761},{lat:41.680015,lng:-99.499775},{lat:41.678203,lng:-99.499789},{lat:41.676391,lng:-99.499803},{lat:41.675485,lng:-99.499810},{lat:41.674579,lng:-99.499817},{lat:41.673673,lng:-99.499823},{lat:41.672767,lng:-99.499830},{lat:41.671862,lng:-99.499837},{lat:41.670956,lng:-99.499844},{lat:41.670050,lng:-99.499851},{lat:41.669144,lng:-99.499858},{lat:41.668238,lng:-99.499865},{lat:41.667332,lng:-99.499872},{lat:41.666426,lng:-99.499879},{lat:41.665520,lng:-99.499886},{lat:41.664614,lng:-99.499893},{lat:41.663708,lng:-99.499900},{lat:41.662802,lng:-99.499907},{lat:41.661897,lng:-99.499914},{lat:41.660085,lng:-99.499928},{lat:41.658273,lng:-99.499942},{lat:41.656461,lng:-99.499956},{lat:41.654649,lng:-99.499970},{lat:41.652837,lng:-99.499984},{lat:41.651026,lng:-99.499998},{lat:41.649214,lng:-99.500012},{lat:41.647402,lng:-99.500026},{lat:41.643778,lng:-99.500054},{lat:41.640155,lng:-99.500082},{lat:41.632907,lng:-99.500138},{lat:41.632865,lng:-99.490478},{lat:41.632822,lng:-99.480818},{lat:41.632778,lng:-99.471158},{lat:41.632733,lng:-99.461497},{lat:41.632688,lng:-99.451837},{lat:41.632641,lng:-99.442177},{lat:41.639889,lng:-99.442115},{lat:41.639865,lng:-99.437284},{lat:41.639841,lng:-99.432454},{lat:41.636218,lng:-99.432486},{lat:41.632594,lng:-99.432517},{lat:41.632570,lng:-99.427687},{lat:41.632546,lng:-99.422857},{lat:41.632534,lng:-99.420442},{lat:41.632522,lng:-99.418027},{lat:41.632510,lng:-99.415612},{lat:41.632497,lng:-99.413197},{lat:41.632491,lng:-99.411990},{lat:41.632485,lng:-99.410782},{lat:41.632479,lng:-99.409575},{lat:41.632473,lng:-99.408367},{lat:41.632466,lng:-99.407160},{lat:41.632460,lng:-99.405952},{lat:41.632454,lng:-99.404745},{lat:41.632448,lng:-99.403537},{lat:41.631995,lng:-99.403541},{lat:41.631542,lng:-99.403545},{lat:41.631089,lng:-99.403550},{lat:41.630636,lng:-99.403554},{lat:41.630183,lng:-99.403558},{lat:41.629730,lng:-99.403562},{lat:41.629277,lng:-99.403566},{lat:41.628824,lng:-99.403570},{lat:41.628371,lng:-99.403575},{lat:41.627918,lng:-99.403579},{lat:41.627465,lng:-99.403583},{lat:41.627012,lng:-99.403587},{lat:41.626559,lng:-99.403591},{lat:41.626106,lng:-99.403595},{lat:41.625653,lng:-99.403600},{lat:41.625200,lng:-99.403604},{lat:41.625197,lng:-99.403000},{lat:41.625194,lng:-99.402396},{lat:41.625191,lng:-99.401793},{lat:41.625188,lng:-99.401189},{lat:41.625185,lng:-99.400585},{lat:41.625182,lng:-99.399982},{lat:41.625179,lng:-99.399378},{lat:41.625175,lng:-99.398774},{lat:41.625172,lng:-99.398171},{lat:41.625169,lng:-99.397567},{lat:41.625166,lng:-99.396963},{lat:41.625163,lng:-99.396360},{lat:41.625160,lng:-99.395756},{lat:41.625156,lng:-99.395152},{lat:41.625153,lng:-99.394549},{lat:41.625150,lng:-99.393945},{lat:41.625147,lng:-99.393341},{lat:41.625144,lng:-99.392738},{lat:41.625141,lng:-99.392134},{lat:41.625137,lng:-99.391530},{lat:41.625134,lng:-99.390926},{lat:41.625131,lng:-99.390323},{lat:41.625128,lng:-99.389719},{lat:41.625125,lng:-99.389115},{lat:41.625121,lng:-99.388512},{lat:41.625118,lng:-99.387908},{lat:41.625115,lng:-99.387304},{lat:41.625112,lng:-99.386701},{lat:41.625109,lng:-99.386097},{lat:41.625105,lng:-99.385493},{lat:41.625102,lng:-99.384890},{lat:41.625099,lng:-99.384286},{lat:41.625097,lng:-99.383984},{lat:41.625096,lng:-99.383682},{lat:41.625094,lng:-99.383380},{lat:41.625092,lng:-99.383079},{lat:41.625091,lng:-99.382777},{lat:41.625089,lng:-99.382475},{lat:41.625088,lng:-99.382173},{lat:41.625086,lng:-99.381871},{lat:41.625084,lng:-99.381569},{lat:41.625083,lng:-99.381268},{lat:41.625081,lng:-99.380966},{lat:41.625079,lng:-99.380664},{lat:41.625078,lng:-99.380362},{lat:41.625076,lng:-99.380060},{lat:41.625075,lng:-99.379758},{lat:41.625073,lng:-99.379457},{lat:41.625071,lng:-99.379155},{lat:41.625070,lng:-99.378853},{lat:41.625068,lng:-99.378551},{lat:41.625066,lng:-99.378249},{lat:41.625065,lng:-99.377947},{lat:41.625063,lng:-99.377646},{lat:41.625062,lng:-99.377344},{lat:41.625060,lng:-99.377042},{lat:41.625058,lng:-99.376740},{lat:41.625057,lng:-99.376438},{lat:41.625055,lng:-99.376136},{lat:41.625053,lng:-99.375834},{lat:41.625052,lng:-99.375533},{lat:41.625050,lng:-99.375231},{lat:41.625049,lng:-99.374929},{lat:41.625047,lng:-99.374627},{lat:41.624594,lng:-99.374631},{lat:41.624141,lng:-99.374636},{lat:41.623688,lng:-99.374640},{lat:41.623235,lng:-99.374645},{lat:41.622782,lng:-99.374649},{lat:41.622329,lng:-99.374653},{lat:41.621876,lng:-99.374658},{lat:41.621423,lng:-99.374662},{lat:41.620970,lng:-99.374666},{lat:41.620517,lng:-99.374671},{lat:41.620064,lng:-99.374675},{lat:41.619611,lng:-99.374680},{lat:41.619158,lng:-99.374684},{lat:41.618706,lng:-99.374688},{lat:41.618253,lng:-99.374693},{lat:41.617800,lng:-99.374697},{lat:41.617796,lng:-99.374093},{lat:41.617793,lng:-99.373490},{lat:41.617790,lng:-99.372886},{lat:41.617786,lng:-99.372283},{lat:41.617783,lng:-99.371679},{lat:41.617780,lng:-99.371075},{lat:41.617777,lng:-99.370472},{lat:41.617773,lng:-99.369868},{lat:41.617770,lng:-99.369264},{lat:41.617767,lng:-99.368661},{lat:41.617763,lng:-99.368057},{lat:41.617760,lng:-99.367454},{lat:41.617757,lng:-99.366850},{lat:41.617753,lng:-99.366246},{lat:41.617750,lng:-99.365643},{lat:41.617747,lng:-99.365039},{lat:41.617743,lng:-99.364436},{lat:41.617740,lng:-99.363832},{lat:41.617737,lng:-99.363228},{lat:41.617733,lng:-99.362625},{lat:41.617730,lng:-99.362021},{lat:41.617727,lng:-99.361418},{lat:41.617723,lng:-99.360814},{lat:41.617720,lng:-99.360210},{lat:41.617717,lng:-99.359607},{lat:41.617713,lng:-99.359003},{lat:41.617710,lng:-99.358399},{lat:41.617707,lng:-99.357796},{lat:41.617703,lng:-99.357192},{lat:41.617700,lng:-99.356589},{lat:41.617697,lng:-99.355985},{lat:41.617693,lng:-99.355381},{lat:41.617690,lng:-99.354778},{lat:41.617686,lng:-99.354174},{lat:41.617683,lng:-99.353571},{lat:41.617680,lng:-99.352967},{lat:41.617676,lng:-99.352363},{lat:41.617673,lng:-99.351760},{lat:41.617669,lng:-99.351156},{lat:41.617666,lng:-99.350553},{lat:41.617663,lng:-99.349949},{lat:41.617659,lng:-99.349345},{lat:41.617656,lng:-99.348742},{lat:41.617652,lng:-99.348138},{lat:41.617649,lng:-99.347534},{lat:41.617646,lng:-99.346931},{lat:41.617642,lng:-99.346327},{lat:41.617639,lng:-99.345724},{lat:41.616733,lng:-99.345733},{lat:41.615827,lng:-99.345742},{lat:41.614921,lng:-99.345751},{lat:41.614015,lng:-99.345760},{lat:41.613109,lng:-99.345769},{lat:41.612203,lng:-99.345778},{lat:41.611297,lng:-99.345788},{lat:41.610391,lng:-99.345797},{lat:41.610385,lng:-99.344590},{lat:41.610378,lng:-99.343383},{lat:41.610371,lng:-99.342175},{lat:41.610364,lng:-99.340968},{lat:41.610357,lng:-99.339761},{lat:41.610350,lng:-99.338554},{lat:41.610343,lng:-99.337347},{lat:41.610336,lng:-99.336140},{lat:41.610329,lng:-99.334933},{lat:41.610322,lng:-99.333726},{lat:41.610315,lng:-99.332519},{lat:41.610308,lng:-99.331312},{lat:41.610301,lng:-99.330105},{lat:41.610294,lng:-99.328898},{lat:41.610287,lng:-99.327691},{lat:41.610280,lng:-99.326483},{lat:41.610266,lng:-99.324069},{lat:41.610252,lng:-99.321655},{lat:41.610238,lng:-99.319241},{lat:41.610223,lng:-99.316827},{lat:41.610195,lng:-99.311998},{lat:41.610166,lng:-99.307170},{lat:41.606542,lng:-99.307209},{lat:41.602918,lng:-99.307248},{lat:41.602889,lng:-99.302420},{lat:41.602860,lng:-99.297592},{lat:41.602845,lng:-99.295178},{lat:41.602830,lng:-99.292764},{lat:41.602815,lng:-99.290350},{lat:41.602801,lng:-99.287937},{lat:41.599177,lng:-99.287976},{lat:41.595553,lng:-99.288016},{lat:41.595523,lng:-99.283189},{lat:41.595493,lng:-99.278362},{lat:41.599117,lng:-99.278321},{lat:41.602740,lng:-99.278281}]';

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