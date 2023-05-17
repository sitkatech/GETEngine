alter table [dbo].[Runs]
	add ProcessingStartDate DateTime null;
go

alter table [dbo].[Runs]
	add ProcessingEndDate DateTime null;
go