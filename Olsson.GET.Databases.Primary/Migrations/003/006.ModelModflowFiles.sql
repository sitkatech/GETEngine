alter table [dbo].[Models]
	add StartDateTime DateTime null;
go

alter table [dbo].[Models]
	add NamFileName varchar(50) null;
go

alter table [dbo].[Models]
	add BaselineFileName varchar(50) null;
go

alter table [dbo].[Models]
	add RunFileName varchar(50) null;
go

alter table [dbo].[Models]
	add ZonesFileName varchar(50) null;
go

alter table [dbo].[Models]
	add ModflowExeName varchar(50) null;
go

--update values
update dbo.models set StartDateTime = '2011-01-01',
NamFileName = 'CPNRD.nam',
BaselineFileName = 'Baseline.dat',
RunFileName = 'CPNRD_streamflow.dat',
ZonesFileName = 'SegRchZones.csv',
ModflowExeName = 'USGs_1.exe'
where id = 1;

--mark fields as not null

alter table [dbo].[Models]
	alter column StartDateTime DateTime not null;
go

alter table [dbo].[Models]
	alter column NamFileName varchar(50) not null;
go

alter table [dbo].[Models]
	alter column BaselineFileName varchar(50) not null;
go

alter table [dbo].[Models]
	alter column RunFileName varchar(50) not null;
go

alter table [dbo].[Models]
	alter column ZonesFileName varchar(50) not null;
go

alter table [dbo].[Models]
	alter column ModflowExeName varchar(50) not null;
go