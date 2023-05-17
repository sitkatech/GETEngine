create table dbo.MeasurementType (
	MeasurementTypeID int not null constraint PK_MeasurementType_MeasurementTypeID primary key,
	MeasurementTypeName varchar(50) not null constraint AK_MeasurementType_MeasurementTypeName unique,
	MeasurementTypeDisplayName varchar(50) not null constraint AK_MeasurementType_MeasurementTypeDisplayName unique
)

insert into dbo.MeasurementType (MeasurementTypeID, MeasurementTypeName, MeasurementTypeDisplayName)
values (1, 'None', 'None'),
(2, 'Volume', 'Volume'),
(3, 'Rate', 'Rate')