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
		'keithajustq',  --This is the name for the image
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
		'Adjust Pumping by .SHP', --This is the name for the scenario
		1, --Input Control Type - 1==CSV Canal Upload, 2==Add Well Map, 3==Adjust Zone Slider, 4=particle count
		0, --0 = does not change the behavior of the input control type for setting records in the well file.  1 = Switches the sign vs the usual behavior for the input control type
		@ImageId
	);

IF (@IsCustom = 1)
BEGIN
-- Link model(s) and scenario
-- Copy and paste this as many times as the number of models tied to the custom scenario
INSERT INTO [dbo].[ModelScenarios] (ModelId, ScenarioId)
VALUES ('18', @ScenarioId)

-- Set up custom files
-- Copy and paste this as many times as the number of files to be inserted for the custom scenario
INSERT INTO [dbo].[ScenarioFiles] (Id, [ScenarioId], [Name], [Description], [Required])
	VALUES (
		(SELECT ISNULL(MAX(Id),0) +1 FROM [dbo].ScenarioFiles), --Don't touch.
		@ScenarioId,
		'AdjustmentFactors.csv', --The file and extension
		'Monthly RCH adjustment factors by zone', --The file description, if any
		1 --Set to 1 if the file is required in order to start a run
	)

INSERT INTO [dbo].[ScenarioFiles] (Id, [ScenarioId], [Name], [Description], [Required])
	VALUES (
		(SELECT ISNULL(MAX(Id),0)+1 FROM [dbo].ScenarioFiles), --Don't touch.
		@ScenarioId,
		'Zones.shp', --The file and extension
		'.shp file with zones', --The file description, if any
		1 --Set to 1 if the file is required in order to start a run
	)

INSERT INTO [dbo].[ScenarioFiles] (Id, [ScenarioId], [Name], [Description], [Required])
	VALUES (
		(SELECT ISNULL(MAX(Id),0)+1 FROM [dbo].ScenarioFiles), --Don't touch.
		@ScenarioId,
		'Zones.shx', --The file and extension
		'.shx file with zones', --The file description, if any
		1 --Set to 1 if the file is required in order to start a run
	)
	
INSERT INTO [dbo].[ScenarioFiles] (Id, [ScenarioId], [Name], [Description], [Required])
	VALUES (
		(SELECT ISNULL(MAX(Id),0)+1 FROM [dbo].ScenarioFiles), --Don't touch.
		@ScenarioId,
		'Zones.dbf', --The file and extension
		'.shx file with zones', --The file description, if any
		1 --Set to 1 if the file is required in order to start a run
	)
	
INSERT INTO [dbo].[ScenarioFiles] (Id, [ScenarioId], [Name], [Description], [Required])
	VALUES (
		(SELECT ISNULL(MAX(Id),0)+1 FROM [dbo].ScenarioFiles), --Don't touch.
		@ScenarioId,
		'Zones.prj', --The file and extension
		'.prj file with zones', --The file description, if any
		1 --Set to 1 if the file is required in order to start a run
	)
END