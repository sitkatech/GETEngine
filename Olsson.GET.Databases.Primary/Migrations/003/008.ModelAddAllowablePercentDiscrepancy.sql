alter table [dbo].[Models]
	add AllowablePercentDiscrepancy float null;
go

update dbo.models set 
AllowablePercentDiscrepancy = 1
where id = 1;