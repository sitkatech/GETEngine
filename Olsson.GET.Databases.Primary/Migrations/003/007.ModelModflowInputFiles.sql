alter table [dbo].[Models]
	add NodeFlowProportionsFileName varchar(50) null;
go

update dbo.models set 
NodeFlowProportionsFileName = 'CanalNodeRelativeContribution.csv'
where id = 1;

alter table [dbo].[Models]
	alter column NodeFlowProportionsFileName varchar(50) not null;
go