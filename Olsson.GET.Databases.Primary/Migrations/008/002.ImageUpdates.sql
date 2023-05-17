ALTER TABLE Images
ADD 
	IsLinux BIT NOT NULL DEFAULT(0),
	CpuCoreCount INT,
	Memory DECIMAL(4,1);

ALTER TABLE Scenarios
DROP COLUMN ImageName;

ALTER TABLE Scenarios
ADD InputImageId INT;

ALTER TABLE Scenarios
ADD FOREIGN KEY (InputImageId) REFERENCES Images(Id);
