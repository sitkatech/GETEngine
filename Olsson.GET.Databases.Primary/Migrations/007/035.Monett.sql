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
SET @imageName = 'monett';

--This is the name of the model as it will show up in the UI.
SET @modelName = 'Monett';

--The start date for the model.
SET @startDateTime = '2015-01-01';

--The name of the modflow program for the model.
SET @modflowExeName = 'USGs_1.exe';

--The name of the name file that will be passed to modflow.
SET @namFileName = 'Monett_cln.nam';

--The name of the output file modflow will generate for the run.
SET @runFileName = null;

--The name of the output heatmap binary file.  This can be null if @locationMapFileName is null.
SET @mapRunFileName = 'Monett_cln.hds';

--These are the map settings to be used by google maps
SET @mapSettings = '{zoom:11,center:{lat:36.921748,lng:-93.915484},mapTypeId:"terrain"}';

--a set of points that makeup the border to be displayed on google maps
SET @mapModelArea = '[{lat:36.767048,lng:-94.153089},{lat:36.767035,lng:-94.148583},{lat:36.767022,lng:-94.144076},{lat:36.767008,lng:-94.139569},{lat:36.766995,lng:-94.135063},{lat:36.766981,lng:-94.130556},{lat:36.766967,lng:-94.126050},{lat:36.766952,lng:-94.121543},{lat:36.766938,lng:-94.117037},{lat:36.766923,lng:-94.112530},{lat:36.766909,lng:-94.108024},{lat:36.766894,lng:-94.103518},{lat:36.766879,lng:-94.099011},{lat:36.766863,lng:-94.094505},{lat:36.766848,lng:-94.089998},{lat:36.766832,lng:-94.085492},{lat:36.766816,lng:-94.080985},{lat:36.766800,lng:-94.076479},{lat:36.766784,lng:-94.071972},{lat:36.766768,lng:-94.067466},{lat:36.766752,lng:-94.062959},{lat:36.766735,lng:-94.058453},{lat:36.766718,lng:-94.053946},{lat:36.766701,lng:-94.049440},{lat:36.766684,lng:-94.044934},{lat:36.766667,lng:-94.040427},{lat:36.766649,lng:-94.035921},{lat:36.766631,lng:-94.031414},{lat:36.766614,lng:-94.026908},{lat:36.766596,lng:-94.022401},{lat:36.766577,lng:-94.017895},{lat:36.766559,lng:-94.013389},{lat:36.766541,lng:-94.008882},{lat:36.766522,lng:-94.004376},{lat:36.766503,lng:-93.999869},{lat:36.766484,lng:-93.995363},{lat:36.766465,lng:-93.990857},{lat:36.766445,lng:-93.986350},{lat:36.766426,lng:-93.981844},{lat:36.766406,lng:-93.977337},{lat:36.766386,lng:-93.972831},{lat:36.766366,lng:-93.968325},{lat:36.766346,lng:-93.963818},{lat:36.766326,lng:-93.959312},{lat:36.766305,lng:-93.954806},{lat:36.766284,lng:-93.950299},{lat:36.766263,lng:-93.945793},{lat:36.766242,lng:-93.941287},{lat:36.766221,lng:-93.936780},{lat:36.766200,lng:-93.932274},{lat:36.766178,lng:-93.927768},{lat:36.766156,lng:-93.923261},{lat:36.766134,lng:-93.918755},{lat:36.766112,lng:-93.914249},{lat:36.766090,lng:-93.909742},{lat:36.766068,lng:-93.905236},{lat:36.766045,lng:-93.900730},{lat:36.766022,lng:-93.896223},{lat:36.765999,lng:-93.891717},{lat:36.765976,lng:-93.887211},{lat:36.765953,lng:-93.882705},{lat:36.765929,lng:-93.878198},{lat:36.765906,lng:-93.873692},{lat:36.765882,lng:-93.869186},{lat:36.765858,lng:-93.864679},{lat:36.765834,lng:-93.860173},{lat:36.765809,lng:-93.855667},{lat:36.765785,lng:-93.851161},{lat:36.765760,lng:-93.846655},{lat:36.765736,lng:-93.842148},{lat:36.765711,lng:-93.837642},{lat:36.765685,lng:-93.833136},{lat:36.765660,lng:-93.828630},{lat:36.765634,lng:-93.824123},{lat:36.765609,lng:-93.819617},{lat:36.765583,lng:-93.815111},{lat:36.765557,lng:-93.810605},{lat:36.765531,lng:-93.806099},{lat:36.765504,lng:-93.801592},{lat:36.765478,lng:-93.797086},{lat:36.765451,lng:-93.792580},{lat:36.765424,lng:-93.788074},{lat:36.765397,lng:-93.783568},{lat:36.765370,lng:-93.779062},{lat:36.765343,lng:-93.774556},{lat:36.765315,lng:-93.770049},{lat:36.765287,lng:-93.765543},{lat:36.765259,lng:-93.761037},{lat:36.765231,lng:-93.756531},{lat:36.765203,lng:-93.752025},{lat:36.765175,lng:-93.747519},{lat:36.765146,lng:-93.743013},{lat:36.765117,lng:-93.738507},{lat:36.765088,lng:-93.734001},{lat:36.765059,lng:-93.729494},{lat:36.765030,lng:-93.724988},{lat:36.765001,lng:-93.720482},{lat:36.764971,lng:-93.715976},{lat:36.764941,lng:-93.711470},{lat:36.764911,lng:-93.706964},{lat:36.764881,lng:-93.702458},{lat:36.764851,lng:-93.697952},{lat:36.764820,lng:-93.693446},{lat:36.764790,lng:-93.688940},{lat:36.768415,lng:-93.688902},{lat:36.772040,lng:-93.688864},{lat:36.775666,lng:-93.688825},{lat:36.779291,lng:-93.688787},{lat:36.782917,lng:-93.688749},{lat:36.786542,lng:-93.688711},{lat:36.790167,lng:-93.688673},{lat:36.793793,lng:-93.688634},{lat:36.797418,lng:-93.688596},{lat:36.801043,lng:-93.688558},{lat:36.804669,lng:-93.688520},{lat:36.808294,lng:-93.688481},{lat:36.811919,lng:-93.688443},{lat:36.815545,lng:-93.688405},{lat:36.819170,lng:-93.688367},{lat:36.822796,lng:-93.688328},{lat:36.826421,lng:-93.688290},{lat:36.830046,lng:-93.688252},{lat:36.833672,lng:-93.688213},{lat:36.837297,lng:-93.688175},{lat:36.840922,lng:-93.688137},{lat:36.844548,lng:-93.688098},{lat:36.848173,lng:-93.688060},{lat:36.851798,lng:-93.688022},{lat:36.855424,lng:-93.687983},{lat:36.859049,lng:-93.687945},{lat:36.862674,lng:-93.687907},{lat:36.866299,lng:-93.687868},{lat:36.869925,lng:-93.687830},{lat:36.873550,lng:-93.687792},{lat:36.877175,lng:-93.687753},{lat:36.880801,lng:-93.687715},{lat:36.884426,lng:-93.687676},{lat:36.888051,lng:-93.687638},{lat:36.891677,lng:-93.687600},{lat:36.895302,lng:-93.687561},{lat:36.898927,lng:-93.687523},{lat:36.902553,lng:-93.687484},{lat:36.906178,lng:-93.687446},{lat:36.909803,lng:-93.687407},{lat:36.913428,lng:-93.687369},{lat:36.917054,lng:-93.687330},{lat:36.920679,lng:-93.687292},{lat:36.924304,lng:-93.687253},{lat:36.927930,lng:-93.687215},{lat:36.931555,lng:-93.687176},{lat:36.935180,lng:-93.687138},{lat:36.938805,lng:-93.687099},{lat:36.942431,lng:-93.687061},{lat:36.946056,lng:-93.687022},{lat:36.949681,lng:-93.686984},{lat:36.953306,lng:-93.686945},{lat:36.956932,lng:-93.686907},{lat:36.960557,lng:-93.686868},{lat:36.964182,lng:-93.686830},{lat:36.967807,lng:-93.686791},{lat:36.971433,lng:-93.686753},{lat:36.975058,lng:-93.686714},{lat:36.978683,lng:-93.686675},{lat:36.982308,lng:-93.686637},{lat:36.985934,lng:-93.686598},{lat:36.989559,lng:-93.686560},{lat:36.993184,lng:-93.686521},{lat:36.996809,lng:-93.686482},{lat:37.000435,lng:-93.686444},{lat:37.004060,lng:-93.686405},{lat:37.007685,lng:-93.686367},{lat:37.011310,lng:-93.686328},{lat:37.014936,lng:-93.686289},{lat:37.018561,lng:-93.686251},{lat:37.022186,lng:-93.686212},{lat:37.025811,lng:-93.686173},{lat:37.029436,lng:-93.686135},{lat:37.033062,lng:-93.686096},{lat:37.036687,lng:-93.686057},{lat:37.040312,lng:-93.686018},{lat:37.043937,lng:-93.685980},{lat:37.047562,lng:-93.685941},{lat:37.051188,lng:-93.685902},{lat:37.054813,lng:-93.685864},{lat:37.058438,lng:-93.685825},{lat:37.062063,lng:-93.685786},{lat:37.065688,lng:-93.685747},{lat:37.069314,lng:-93.685709},{lat:37.072939,lng:-93.685670},{lat:37.076564,lng:-93.685631},{lat:37.080189,lng:-93.685592},{lat:37.083814,lng:-93.685554},{lat:37.083845,lng:-93.690078},{lat:37.083876,lng:-93.694603},{lat:37.083907,lng:-93.699128},{lat:37.083937,lng:-93.703653},{lat:37.083968,lng:-93.708178},{lat:37.083998,lng:-93.712703},{lat:37.084028,lng:-93.717228},{lat:37.084057,lng:-93.721752},{lat:37.084087,lng:-93.726277},{lat:37.084117,lng:-93.730802},{lat:37.084146,lng:-93.735327},{lat:37.084175,lng:-93.739852},{lat:37.084204,lng:-93.744377},{lat:37.084233,lng:-93.748902},{lat:37.084261,lng:-93.753427},{lat:37.084290,lng:-93.757952},{lat:37.084318,lng:-93.762477},{lat:37.084346,lng:-93.767002},{lat:37.084374,lng:-93.771526},{lat:37.084401,lng:-93.776051},{lat:37.084429,lng:-93.780576},{lat:37.084456,lng:-93.785101},{lat:37.084483,lng:-93.789626},{lat:37.084510,lng:-93.794151},{lat:37.084537,lng:-93.798676},{lat:37.084564,lng:-93.803201},{lat:37.084590,lng:-93.807726},{lat:37.084617,lng:-93.812251},{lat:37.084643,lng:-93.816776},{lat:37.084669,lng:-93.821301},{lat:37.084695,lng:-93.825826},{lat:37.084720,lng:-93.830351},{lat:37.084746,lng:-93.834876},{lat:37.084771,lng:-93.839401},{lat:37.084796,lng:-93.843926},{lat:37.084821,lng:-93.848451},{lat:37.084846,lng:-93.852977},{lat:37.084871,lng:-93.857502},{lat:37.084895,lng:-93.862027},{lat:37.084919,lng:-93.866552},{lat:37.084943,lng:-93.871077},{lat:37.084967,lng:-93.875602},{lat:37.084991,lng:-93.880127},{lat:37.085015,lng:-93.884652},{lat:37.085038,lng:-93.889177},{lat:37.085061,lng:-93.893702},{lat:37.085084,lng:-93.898227},{lat:37.085107,lng:-93.902752},{lat:37.085130,lng:-93.907278},{lat:37.085152,lng:-93.911803},{lat:37.085175,lng:-93.916328},{lat:37.085197,lng:-93.920853},{lat:37.085219,lng:-93.925378},{lat:37.085241,lng:-93.929903},{lat:37.085262,lng:-93.934428},{lat:37.085284,lng:-93.938954},{lat:37.085305,lng:-93.943479},{lat:37.085326,lng:-93.948004},{lat:37.085347,lng:-93.952529},{lat:37.085368,lng:-93.957054},{lat:37.085389,lng:-93.961579},{lat:37.085409,lng:-93.966105},{lat:37.085430,lng:-93.970630},{lat:37.085450,lng:-93.975155},{lat:37.085470,lng:-93.979680},{lat:37.085489,lng:-93.984205},{lat:37.085509,lng:-93.988731},{lat:37.085528,lng:-93.993256},{lat:37.085548,lng:-93.997781},{lat:37.085567,lng:-94.002306},{lat:37.085586,lng:-94.006831},{lat:37.085604,lng:-94.011357},{lat:37.085623,lng:-94.015882},{lat:37.085641,lng:-94.020407},{lat:37.085659,lng:-94.024932},{lat:37.085678,lng:-94.029458},{lat:37.085695,lng:-94.033983},{lat:37.085713,lng:-94.038508},{lat:37.085731,lng:-94.043033},{lat:37.085748,lng:-94.047559},{lat:37.085765,lng:-94.052084},{lat:37.085782,lng:-94.056609},{lat:37.085799,lng:-94.061134},{lat:37.085816,lng:-94.065660},{lat:37.085832,lng:-94.070185},{lat:37.085848,lng:-94.074710},{lat:37.085865,lng:-94.079235},{lat:37.085881,lng:-94.083761},{lat:37.085896,lng:-94.088286},{lat:37.085912,lng:-94.092811},{lat:37.085927,lng:-94.097337},{lat:37.085943,lng:-94.101862},{lat:37.085958,lng:-94.106387},{lat:37.085973,lng:-94.110912},{lat:37.085987,lng:-94.115438},{lat:37.086002,lng:-94.119963},{lat:37.086016,lng:-94.124488},{lat:37.086031,lng:-94.129014},{lat:37.086045,lng:-94.133539},{lat:37.086059,lng:-94.138064},{lat:37.086072,lng:-94.142590},{lat:37.086086,lng:-94.147115},{lat:37.086099,lng:-94.151640},{lat:37.086113,lng:-94.156166},{lat:37.082487,lng:-94.156182},{lat:37.078862,lng:-94.156198},{lat:37.075236,lng:-94.156215},{lat:37.071611,lng:-94.156231},{lat:37.067985,lng:-94.156248},{lat:37.064360,lng:-94.156264},{lat:37.060734,lng:-94.156280},{lat:37.057109,lng:-94.156297},{lat:37.053483,lng:-94.156313},{lat:37.049858,lng:-94.156329},{lat:37.046232,lng:-94.156346},{lat:37.042607,lng:-94.156362},{lat:37.038981,lng:-94.156378},{lat:37.035356,lng:-94.156395},{lat:37.031730,lng:-94.156411},{lat:37.028105,lng:-94.156427},{lat:37.024479,lng:-94.156444},{lat:37.020854,lng:-94.156460},{lat:37.017228,lng:-94.156476},{lat:37.013602,lng:-94.156493},{lat:37.009977,lng:-94.156509},{lat:37.006351,lng:-94.156525},{lat:37.002726,lng:-94.156542},{lat:36.999100,lng:-94.156558},{lat:36.995475,lng:-94.156574},{lat:36.991849,lng:-94.156591},{lat:36.988224,lng:-94.156607},{lat:36.984598,lng:-94.156623},{lat:36.980973,lng:-94.156639},{lat:36.977347,lng:-94.156656},{lat:36.973722,lng:-94.156672},{lat:36.970096,lng:-94.156688},{lat:36.966470,lng:-94.156705},{lat:36.962845,lng:-94.156721},{lat:36.959219,lng:-94.156737},{lat:36.955594,lng:-94.156753},{lat:36.951968,lng:-94.156770},{lat:36.948343,lng:-94.156786},{lat:36.944717,lng:-94.156802},{lat:36.941092,lng:-94.156818},{lat:36.937466,lng:-94.156835},{lat:36.933840,lng:-94.156851},{lat:36.930215,lng:-94.156867},{lat:36.926589,lng:-94.156883},{lat:36.922964,lng:-94.156900},{lat:36.919338,lng:-94.156916},{lat:36.915712,lng:-94.156932},{lat:36.912087,lng:-94.156948},{lat:36.908461,lng:-94.156965},{lat:36.904836,lng:-94.156981},{lat:36.901210,lng:-94.156997},{lat:36.897584,lng:-94.157013},{lat:36.893959,lng:-94.157030},{lat:36.890333,lng:-94.157046},{lat:36.886708,lng:-94.157062},{lat:36.883082,lng:-94.157078},{lat:36.879456,lng:-94.157094},{lat:36.875831,lng:-94.157111},{lat:36.872205,lng:-94.157127},{lat:36.868580,lng:-94.157143},{lat:36.864954,lng:-94.157159},{lat:36.861328,lng:-94.157175},{lat:36.857703,lng:-94.157192},{lat:36.854077,lng:-94.157208},{lat:36.850452,lng:-94.157224},{lat:36.846826,lng:-94.157240},{lat:36.843200,lng:-94.157256},{lat:36.839575,lng:-94.157273},{lat:36.835949,lng:-94.157289},{lat:36.832323,lng:-94.157305},{lat:36.828698,lng:-94.157321},{lat:36.825072,lng:-94.157337},{lat:36.821446,lng:-94.157353},{lat:36.817821,lng:-94.157370},{lat:36.814195,lng:-94.157386},{lat:36.810570,lng:-94.157402},{lat:36.806944,lng:-94.157418},{lat:36.803318,lng:-94.157434},{lat:36.799693,lng:-94.157450},{lat:36.796067,lng:-94.157466},{lat:36.792441,lng:-94.157483},{lat:36.788816,lng:-94.157499},{lat:36.785190,lng:-94.157515},{lat:36.781564,lng:-94.157531},{lat:36.777939,lng:-94.157547},{lat:36.774313,lng:-94.157563},{lat:36.770687,lng:-94.157579},{lat:36.767062,lng:-94.157596},{lat:36.767048,lng:-94.153089}]';

--the name of the zone budget executeable.  This can be null if we do not want to generate the zone budget data.
SET @zoneBudgetExeName = null;

--Does the heat map output file use double sized value (0=Single, 1=Double)
SET @isDoubleSizeHeatMapOutput = 0;

--The the maximum varience allowed in the percent discrepancy.  This can be set to null (percent discrepancy will not be verified).
SET @allowablePercentDiscrepancy = 1.0;

--Add one value for each scenario that this model supports. 1=Add a Well, 2=Remove a Well, 3=Move a Well, 4=Canal Recharge, 5=Adjust Zone, 6=Retire Additional Wells, 7=Specify Pumping, 8 = ASR, 9 = Adjust Pumping, 10 = particle track
insert @scenarios(id) values(1),(7);

--array of zone name, zone number, and bounds defined a a set of points to draw the zone polygon. Sample at https://jsoneditoronline.org/?id=6efc0290cfe1ed97af040d8592a457da
set @zoneData = null;

--Total count of stress periods for the model
set @numberOfStressPeriods = 192;

--Canal Names
set @canalData = 'Well 1,Well 4,Well 5,Well 9,Well 10,Well 11,Well 12,Well 13,Well 14,Well 15,Well 16,Well 17,Well 18,Well 19,Well 20,Well 21'

--Modpath exe
set @modPathExeName = null

--Modpath simFile name
set @simulationFileName = null
----- End Values to Set -----


----- DO NOT CHANGE -----
exec dbo.UpsertModel @imageName, @modelName, @startDateTime, @modflowExeName, @namFileName,  @runFileName, @mapRunFileName, @mapSettings, @mapModelArea, @zoneBudgetExeName, @isDoubleSizeHeatMapOutput, @allowablePercentDiscrepancy, @scenarios, @zoneData, @numberOfStressPeriods, @canalData, @modPathExeName, @simulationFileName;
-------------------------