create table dbo.TimeframeType (
	TimeframeTypeID int not null constraint PK_TimeframeType_TimeframeTypeID primary key,
	TimeframeTypeName varchar(50) not null constraint AK_TimeframeType_TimeframeTypeName unique,
	TimeframeTypeDisplayName varchar(50) not null constraint  AK_TimeframeType_TimeframeTypeDisplayName unique,
	TimeframeTypePluralizedName varchar(50) not null constraint AK_TimeframeType_TimeframeTypePluralizedName unique
)

insert into dbo.TimeframeType (TimeframeTypeID, TimeframeTypeName,  TimeframeTypeDisplayName, TimeframeTypePluralizedName)
values (1, 'Minute', 'Minute', 'Minutes'),
(2, 'Hour', 'Hour', 'Hours'),
(3, 'Day', 'Day', 'Days'),
(4, 'Week', 'Week', 'Weeks'),
(5, 'Month', 'Month', 'Months')
go

alter table dbo.VolumeUnit
add TimeframeTypeID int null constraint FK_VolumeUnit_TimeframeType_TimeframeTypeID foreign key references dbo.TimeframeType (TimeframeTypeID)
go

update dbo.VolumeUnit
set TimeframeTypeID = 1
where VolumeUnitID = 5

update dbo.VolumeUnit
set TimeframeTypeID = 3
where VolumeUnitID in (2,3,4,6)

update dbo.VolumeUnit
set TimeframeTypeID = 5
where VolumeUnitID = 1