create table dbo.BaseflowTableProcessingConfigurations (
	BaseflowTableProcessingConfigurationID int not null identity(1,1) constraint PK_BaseflowProcessingConfiguration_BaseflowProcessingConfigurationID primary key,
	BaseflowTableIndicatorRegexPattern varchar(200) not null,
	SegmentColumnNum int not null,
	FlowToAquiferColumnNum int not null,
	ReachColumnNum int null
)

insert into dbo.BaseflowTableProcessingConfigurations (BaseflowTableIndicatorRegexPattern, SegmentColumnNum, FlowToAquiferColumnNum, ReachColumnNum)
values ('^\s+STREAM LISTING\s+PERIOD\s+[0-9]+\s+STEP\s+[0-9]+$', 4, 7, 5),
('^\s+STREAM LISTING\s+PERIOD\s+[0-9]+\s+STEP\s+[0-9]+$', 2, 5, 3),
('^\s+SFR \(SFR-\d+\) FLOWS\s+PERIOD\s+[0-9]+\s+STEP\s+[0-9]+$', 1, 7, null),
('^\s+SFR \(STREAMS_SFR\) FLOWS\s+PERIOD\s+[0-9]+\s+STEP\s+[0-9]+$', 2, 8, null),
('^\s+SFR-\d+ PACKAGE - SUMMARY OF FLOWS FOR EACH CONTROL VOLUME\s+PERIOD\s+[0-9]+\s+STEP\s+[0-9]+$', 1, 5, null)

alter table dbo.Models
add BaseflowTableProcessingConfigurationID int null constraint FK_Models_BaseflowTableProcessingConfigurations_BaseflowTableProcessingConfigurationID foreign key references dbo.BaseflowTableProcessingConfigurations (BaseflowTableProcessingConfigurationID) 