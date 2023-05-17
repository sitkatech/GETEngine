alter table dbo.Models add SimulationFileName varchar(50) null;
go

--better test data
update dbo.Models set [MapSettings] = (select [MapSettings] from dbo.Models where Name = 'Cohyst') where id = 19;
go

update dbo.Models set [MapModelArea] = (select [MapModelArea] from dbo.Models where Name = 'Cohyst')  where id = 19;
go

update dbo.Models set Name = 'Cohyst - Modpath'  where id = 19;
go

update dbo.Models set SimulationFileName = 'COHYST.mpsim'  where id = 19;
go

update dbo.Models set [ModpathExeName] = 'MPath7.exe'  where id = 19;
go

update dbo.Images set Name = 'cohystmp' where name = 'modpathtest';
go

alter table dbo.Models alter column NamFileName varchar(50) null;
go

alter table dbo.Models alter column ModflowExeName varchar(50) null;
go

update dbo.Models set NamFileName = null where NamFileName = '';
go

update dbo.Models set ModflowExeName = null where ModflowExeName = '';
go