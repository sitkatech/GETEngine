alter table dbo.Users
add CanManageReports bit null
go

update dbo.Users
set CanManageReports = 0

alter table dbo.Users
alter column CanManageReports bit not null