CREATE TABLE VolumeUnits
(
	Id INTEGER PRIMARY KEY, -- no identity
	VolumeType NVARCHAR (50) NOT NULL UNIQUE
);

INSERT INTO VolumeUnits (Id, VolumeType)
values
  (0, 'Unknown'),
  (1, 'Acre Feet'),
  (2, 'Cubic Feet'),
  (3, 'Cubic Yard'),
  (4, 'Cubic Meter'),
  (5, 'US Gallon'),
  (6, 'US Gallons in Millions');
GO

ALTER TABLE dbo.[Runs] 
ADD InputVolumeUnit INTEGER NULL
GO

ALTER TABLE dbo.[Runs] 
ADD OutputVolumeUnit INTEGER NULL
GO

ALTER TABLE dbo.[Runs]
ADD CONSTRAINT FK_Runs_InputVolumeUnit FOREIGN KEY (InputVolumeUnit)
    REFERENCES dbo.VolumeUnits (Id);
GO

ALTER TABLE dbo.[Runs]
ADD CONSTRAINT FK_Runs_OutputVolumeUnit FOREIGN KEY (OutputVolumeUnit)
    REFERENCES dbo.VolumeUnits (Id);
GO

-- Update Runs with WellMap InputControlType
UPDATE R
SET 
    R.InputVolumeUnit = 5, -- Gallons
    R.OutputVolumeUnit = 1 -- Acre Feet
FROM dbo.[Runs] AS R
INNER JOIN dbo.[Scenarios] S ON R.ScenarioId = S.Id
WHERE S.InputControlType = 2
GO

-- Update Runs that are not WellMap or ZoneMap
UPDATE R
SET 
    R.InputVolumeUnit = 1, -- Acre Feet
    R.OutputVolumeUnit = 1 -- Acre Feet
FROM dbo.[Runs] AS R
INNER JOIN dbo.[Scenarios] S ON R.ScenarioId = S.Id
WHERE S.InputControlType != 2 AND S.InputControlType != 3
GO

-- UPDATE all other Runs
UPDATE R
SET 
    R.InputVolumeUnit = 2, -- Cubic Feet
    R.OutputVolumeUnit = 1 -- Acre Feet
FROM dbo.[Runs] AS R
WHERE R.InputVolumeUnit IS NULL OR R.OutputVolumeUnit IS NULL
GO

ALTER TABLE dbo.[Runs]
ALTER COLUMN InputVolumeUnit INTEGER NOT NULL
GO

ALTER TABLE dbo.[Runs]
ALTER COLUMN OutputVolumeUnit INTEGER NOT NULL
GO