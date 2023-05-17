DECLARE @IsCustom BIT,
		@ImageId INT,
		@ScenarioId INT;

SET @IsCustom = 0; --Set this to 1 if this is a custom scenario

IF (@IsCustom = 1)
BEGIN

SELECT @ImageId = MAX(ImageID)+1 FROM [dbo].[Image] --Don't touch. This will get the new image id

INSERT INTO [dbo].[Image] (ImageID, ImageName, [Server], IsLinux, CpuCoreCount, Memory)
	VALUES(
		@ImageId, 
		'Insert Image Name Here',  --This is the name for the image
		'', -- Empty string for now since the column is required. Will delete this in the future
		1, -- Defaults to Windows container. Set to 1 if the image will be Linux container
		null, -- In code, this defaults to 1. Set this to a higher number if the process is memory-intensive
		null --In code, this defaults to 3.5 (gb). Set this to a higher number if the process is memory-intensive
	)
END

SELECT @ScenarioId = MAX(ScenarioID)+1 FROM [dbo].Scenario --Don't touch.  This will put in the next highest id.  If you need to get it for the model update, you can use SSMS to find the value.

INSERT INTO [dbo].[Scenario] (ScenarioID, ScenarioName, InputControlType, ShouldSwitchSign, InputImageId, ShowToAllUsersInScenarioList)
	VALUES (
		@ScenarioId, --Don't touch.  This will put in the next highest id.  If you need to get it for the model update, you can use SSMS to find the value.
		'Insert Scenario Name Here', --This is the name for the scenario
		1, --Input Control Type - 1==CSV Canal Upload, 2==Add Well Map, 3==Adjust Zone Slider, 4=particle count
		0, --0 = does not change the behavior of the input control type for setting records in the well file.  1 = Switches the sign vs the usual behavior for the input control type
		@ImageId,
		0 /*-- this can be changed in the UI, so we can default to false here*/
	);

IF (@IsCustom = 1)
BEGIN
-- Link model(s) and scenario
-- Copy and paste this as many times as the number of models tied to the custom scenario
INSERT INTO [dbo].[ModelScenario] (ModelId, ScenarioId)
VALUES ('Insert Model Id', @ScenarioId)

-- Set up custom files
-- Copy and paste this as many times as the number of files to be inserted for the custom scenario
INSERT INTO [dbo].[ScenarioFile] (ScenarioFileID, [ScenarioId], ScenarioFileName, ScenarioFileDescription, IsRequired)
	VALUES (
		(SELECT MAX(ScenarioFileID)+1 FROM [dbo].ScenarioFile), --Don't touch.
		@ScenarioId,
		'Insert Filename.Extension Here', --The file and extension
		'Insert Description Here', --The file description, if any
		1 --Set to 1 if the file is required in order to start a run
	)
END