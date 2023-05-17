alter table [dbo].Models
	add NumberOfStressPeriods int not null default (600);
go

alter table [dbo].Models
	add CanalData varchar(max) null;
go

update dbo.Models set CanalData = 'Southside,30-mile,Cozad' where id = 1;
update dbo.Models set CanalData = 'Dawson County Canal,Gothenburg Canal,Kearney Canal,Keith Lincoln Canal,Orchard Alfalfa Canal,Thirtymile Canal,Tri County Supply Canal,Western Canal' where id = 2;
update dbo.Models set CanalData = 'Western Canal' where id = 3;