DECLARE @IsCustom BIT,
		@ImageId INT,
		@ScenarioId INT;

SET @IsCustom = 1; --Set this to 1 if this is a custom scenario

IF (@IsCustom = 1)
BEGIN

SELECT @ImageId = MAX(Id)+1 FROM [dbo].Images --Don't touch. This will get the new image id

INSERT INTO [dbo].[Images] (Id, [Name], [Server], IsLinux, CpuCoreCount, Memory)
	VALUES(
		@ImageId, 
		'userfileupload',  --This is the name for the image
		'', -- Empty string for now since the column is required. Will delete this in the future
		1, -- Defaults to Windows container. Set to 1 if the image will be Linux container
		null, -- In code, this defaults to 1. Set this to a higher number if the process is memory-intensive
		null --In code, this defaults to 3.5 (gb). Set this to a higher number if the process is memory-intensive
	)
END

SELECT @ScenarioId = MAX(Id)+1 FROM [dbo].Scenarios --Don't touch.  This will put in the next highest id.  If you need to get it for the model update, you can use SSMS to find the value.

INSERT INTO [dbo].[Scenarios] (Id, [Name], InputControlType, ShouldSwitchSign, InputImageId)
	VALUES (
		@ScenarioId, --Don't touch.  This will put in the next highest id.  If you need to get it for the model update, you can use SSMS to find the value.
		'Upload Model File', --This is the name for the scenario
		1, --Input Control Type - 1==CSV Canal Upload, 2==Add Well Map, 3==Adjust Zone Slider, 4=particle count
		0, --0 = does not change the behavior of the input control type for setting records in the well file.  1 = Switches the sign vs the usual behavior for the input control type
		@ImageId
	);

IF (@IsCustom = 1)
BEGIN
-- Link model(s) and scenario
-- Copy and paste this as many times as the number of models tied to the custom scenario
INSERT INTO [dbo].[ModelScenarios] (ModelId, ScenarioId)
VALUES ('76', @ScenarioId)


-- Set up custom files
-- Copy and paste this as many times as the number of files to be inserted for the custom scenario
INSERT INTO [dbo].[ScenarioFiles] (Id, [ScenarioId], [Name], [Description], [Required])
	VALUES (
		(SELECT ISNULL(MAX(Id),0) +1 FROM [dbo].ScenarioFiles), --Don't touch.
		@ScenarioId,
		'COHYST2010_28b_14_28_S1950.bas', --The file and extension
		'Basic File', --The file description, if any
		0 --Set to 1 if the file is required in order to start a run
	)

INSERT INTO [dbo].[ScenarioFiles] (Id, [ScenarioId], [Name], [Description], [Required])
	VALUES (
		(SELECT ISNULL(MAX(Id),0)+1 FROM [dbo].ScenarioFiles), --Don't touch.
		@ScenarioId,
		'COHYST2010_28b_14_28_S1950E2063.dis', --The file and extension
		'Discretization File', --The file description, if any
		0 --Set to 1 if the file is required in order to start a run
	)

INSERT INTO [dbo].[ScenarioFiles] (Id, [ScenarioId], [Name], [Description], [Required])
	VALUES (
		(SELECT ISNULL(MAX(Id),0)+1 FROM [dbo].ScenarioFiles), --Don't touch.
		@ScenarioId,
		'COHYST2010_28b_14_28_S1950E2063.drn', --The file and extension
		'Drain Package', --The file description, if any
		0 --Set to 1 if the file is required in order to start a run
	)
	
INSERT INTO [dbo].[ScenarioFiles] (Id, [ScenarioId], [Name], [Description], [Required])
	VALUES (
		(SELECT ISNULL(MAX(Id),0)+1 FROM [dbo].ScenarioFiles), --Don't touch.
		@ScenarioId,
		'COHYST2010_28b_14_28_S1950E2063.evt', --The file and extension
		'EVT Package', --The file description, if any
		0 --Set to 1 if the file is required in order to start a run
	)
	
INSERT INTO [dbo].[ScenarioFiles] (Id, [ScenarioId], [Name], [Description], [Required])
	VALUES (
		(SELECT ISNULL(MAX(Id),0)+1 FROM [dbo].ScenarioFiles), --Don't touch.
		@ScenarioId,
		'COHYST2010_28b_14_28_SP1368.ghb', --The file and extension
		'General Head Boundary Package', --The file description, if any
		0 --Set to 1 if the file is required in order to start a run
	)

INSERT INTO [dbo].[ScenarioFiles] (Id, [ScenarioId], [Name], [Description], [Required])
	VALUES (
		(SELECT ISNULL(MAX(Id),0)+1 FROM [dbo].ScenarioFiles), --Don't touch.
		@ScenarioId,
		'COHYST2010_28b_14_28.gmg', --The file and extension
		'Solver', --The file description, if any
		0 --Set to 1 if the file is required in order to start a run
	)
	
INSERT INTO [dbo].[ScenarioFiles] (Id, [ScenarioId], [Name], [Description], [Required])
	VALUES (
		(SELECT ISNULL(MAX(Id),0)+1 FROM [dbo].ScenarioFiles), --Don't touch.
		@ScenarioId,
		'COHYST2010_28b_14_28.lpf', --The file and extension
		'Layer-Property Flow Package', --The file description, if any
		0 --Set to 1 if the file is required in order to start a run
	)
	
INSERT INTO [dbo].[ScenarioFiles] (Id, [ScenarioId], [Name], [Description], [Required])
	VALUES (
		(SELECT ISNULL(MAX(Id),0)+1 FROM [dbo].ScenarioFiles), --Don't touch.
		@ScenarioId,
		'name.nam', --The file and extension
		'Name File', --The file description, if any
		0 --Set to 1 if the file is required in order to start a run
	)

INSERT INTO [dbo].[ScenarioFiles] (Id, [ScenarioId], [Name], [Description], [Required])
	VALUES (
		(SELECT ISNULL(MAX(Id),0)+1 FROM [dbo].ScenarioFiles), --Don't touch.
		@ScenarioId,
		'CombinedRCH.rch', --The file and extension
		'Recharge Package', --The file description, if any
		0 --Set to 1 if the file is required in order to start a run
	)
	
INSERT INTO [dbo].[ScenarioFiles] (Id, [ScenarioId], [Name], [Description], [Required])
	VALUES (
		(SELECT ISNULL(MAX(Id),0)+1 FROM [dbo].ScenarioFiles), --Don't touch.
		@ScenarioId,
		'COHYST2010_28b_14_28_S1950.riv', --The file and extension
		'River Package', --The file description, if any
		0 --Set to 1 if the file is required in order to start a run
	)
	
INSERT INTO [dbo].[ScenarioFiles] (Id, [ScenarioId], [Name], [Description], [Required])
	VALUES (
		(SELECT ISNULL(MAX(Id),0)+1 FROM [dbo].ScenarioFiles), --Don't touch.
		@ScenarioId,
		'COHYST2010_28_IN_1950_2063.sfr', --The file and extension
		'Streamflow-Routing Package', --The file description, if any
		0 --Set to 1 if the file is required in order to start a run
	)

INSERT INTO [dbo].[ScenarioFiles] (Id, [ScenarioId], [Name], [Description], [Required])
	VALUES (
		(SELECT ISNULL(MAX(Id),0)+1 FROM [dbo].ScenarioFiles), --Don't touch.
		@ScenarioId,
		'RobustReview_Base002+NDC.WEL', --The file and extension
		'Well Package', --The file description, if any
		0 --Set to 1 if the file is required in order to start a run
	)
	
INSERT INTO [dbo].[ScenarioFiles] (Id, [ScenarioId], [Name], [Description], [Required])
	VALUES (
		(SELECT ISNULL(MAX(Id),0)+1 FROM [dbo].ScenarioFiles), --Don't touch.
		@ScenarioId,
		'COHYST2010_28b_14_28.zone', --The file and extension
		'Zone File', --The file description, if any
		0 --Set to 1 if the file is required in order to start a run
	)

INSERT INTO [dbo].[ScenarioFiles] (Id, [ScenarioId], [Name], [Description], [Required])
	VALUES (
		(SELECT ISNULL(MAX(Id),0)+1 FROM [dbo].ScenarioFiles), --Don't touch.
		@ScenarioId,
		'NRDzones.DAT', --The file and extension
		'ZONEBUDGET Zone File', --The file description, if any
		0 --Set to 1 if the file is required in order to start a run
	)
	
INSERT INTO [dbo].[ScenarioFiles] (Id, [ScenarioId], [Name], [Description], [Required])
	VALUES (
		(SELECT ISNULL(MAX(Id),0)+1 FROM [dbo].ScenarioFiles), --Don't touch.
		@ScenarioId,
		'BaselineZoneBudget.csv', --The file and extension
		'Baseline ZONEBUDGET Output', --The file description, if any
		0 --Set to 1 if the file is required in order to start a run
	)

INSERT INTO [dbo].[ScenarioFiles] (Id, [ScenarioId], [Name], [Description], [Required])
	VALUES (
		(SELECT ISNULL(MAX(Id),0)+1 FROM [dbo].ScenarioFiles), --Don't touch.
		@ScenarioId,
		'ZoneBudgetNames.csv', --The file and extension
		'ZONEBUDGET Zone Names', --The file description, if any
		0 --Set to 1 if the file is required in order to start a run
	)
END