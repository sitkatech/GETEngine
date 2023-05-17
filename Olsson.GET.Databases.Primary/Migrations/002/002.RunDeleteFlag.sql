alter table dbo.runs
	add IsDeleted bit not null default(0);