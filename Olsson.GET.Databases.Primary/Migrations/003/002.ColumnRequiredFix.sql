alter table dbo.users
	alter column PhoneNumber CHAR(50) NULL;
GO

alter table dbo.runs
	alter column [InputControlId] int null;
go

alter table dbo.runs
	alter column [ImageId] int null;
GO