alter table [dbo].[Models]
	add LocationMapFileName varchar(50) null;
go

update dbo.models set 
LocationMapFileName = 'relateMat.txt'
where id = 2;