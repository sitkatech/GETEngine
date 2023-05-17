exec sp_rename 'dbo.Model.ZoneBudgetExeName', 'PostProcessingExeName', 'COLUMN'
GO
exec sp_rename 'dbo.Model.ModflowExeName', 'EntryPointExeName', 'COLUMN'
GO

update dbo.Model
set EntryPointExeName = ModpathExeName
where EntryPointExeName is null and ModpathExeName is not null

alter table dbo.Model drop column ModpathExeName