Alter table dbo.ReportTemplates
Add IsAvailableForAllConfigurations bit null
go

update dbo.ReportTemplates
set IsAvailableForallConfigurations = 0

Alter table dbo.ReportTemplates
Alter column IsAvailableForAllConfigurations bit not null
go