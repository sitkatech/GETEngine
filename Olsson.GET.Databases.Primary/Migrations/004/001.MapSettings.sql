alter table [dbo].[Models]
	add MapSettings varchar(1024) null;
go

alter table [dbo].[Models]
	add MapModelArea varchar(max) null;
go

update models set MapSettings = '{zoom:9,center:{lat:40.8258,lng:-96.682},mapTypeId:"terrain"}';
go

update models set MapModelArea = '[{lat:40.24,lng:-97.1},{lat:40.25,lng:-96.25},{lat:41.37,lng:-96.1},{lat:41.31,lng:-97.4}]';
go