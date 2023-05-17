alter table [dbo].Customers
	add IsTrial bit default 0 not null;
go