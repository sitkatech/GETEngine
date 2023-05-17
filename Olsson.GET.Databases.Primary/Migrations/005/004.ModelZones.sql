alter table [dbo].[Models]
	add ZoneData varchar(max) null;
go

update models set ZoneData = '[{"ZoneNumber":"1","Name":"Zone A","Bounds":[{"Lat":40.9577,"Lng":-100.3192},{"Lat":40.9536,"Lng":-100.2725},{"Lat":40.9121,"Lng":-100.2711},{"Lat":40.9245,"Lng":-100.3192}]},{"ZoneNumber":"2","Name":"Zone B","Bounds":[{"Lat":40.8934,"Lng":-100.0066},{"Lat":40.8851,"Lng":-99.916},{"Lat":40.8477,"Lng":-99.9435}]},{"ZoneNumber":"3","Name":"Zone C","Bounds":[{"Lat":40.8072,"Lng":-99.9154},{"Lat":40.7718,"Lng":-99.9662},{"Lat":40.7801,"Lng":-99.8358}]}]'
where id = 1;
go

insert into Scenarios values(5, 'Adjust Irrigation', 3);
go

insert into ModelScenarios values (1,5);
go