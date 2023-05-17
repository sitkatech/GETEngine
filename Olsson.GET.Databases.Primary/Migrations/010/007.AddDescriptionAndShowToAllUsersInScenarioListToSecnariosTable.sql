alter table dbo.Scenarios
add [Description] varchar(100) null

alter table dbo.Scenarios
add ShowToAllUsersInScenarioList bit null
go

update dbo.Scenarios
set ShowToAllUsersInScenarioList = 0

alter table dbo.Scenarios
alter column ShowToAllUsersInScenarioList bit not null