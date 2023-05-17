declare @col varchar(256);

SELECT @col = CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
WHERE TABLE_NAME='runs' and CONSTRAINT_NAME like '%InputCo%';

if(@col is not null)
begin

EXEC ( 'ALTER TABLE Runs DROP CONSTRAINT ' + @col);

end
go

alter table dbo.runs drop column InputControlId;
go

drop table dbo.InputControls;
go

alter table dbo.Scenarios 
	add InputControlType int not null default(0);
go

update dbo.Scenarios set InputControlType = 1 where InputControlType = 0;
go

update dbo.Scenarios set InputControlType = 2 where name in ('Add a Well', 'Remove a Well');
go

insert into dbo.ModelScenarios values(1,1);
go