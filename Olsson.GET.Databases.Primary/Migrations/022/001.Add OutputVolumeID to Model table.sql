alter table dbo.Model add OutputVolumeUnitID int null constraint FK_Model_VolumeUnit_OutputVolumeUnitID_VolumeUnitID foreign key references dbo.VolumeUnit(VolumeUnitID)
GO

update dbo.Model set OutputVolumeUnitID = 2

update dbo.Model set OutputVolumeUnitID = 4 where ModelName = 'PVHM CCSM4'

alter table dbo.Model alter column OutputVolumeUnitID int not null