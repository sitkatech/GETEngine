update dbo.Models set [NumberOfStressPeriods] = 72 where id = 1;
go

update dbo.Models set [NumberOfStressPeriods] = 316 where id = 2;
go

update dbo.Models set MapSettings = '{zoom:10,center:{lat:40.8876131,lng:-100.0892906},mapTypeId:"terrain"}' where id = 4;
go

update dbo.Models set CanalData = 'Southside,30-mile,Cozad' where id = 4;
go

update dbo.Models set ZoneData = (select zonedata from models where id = 4) where id = 1;
go