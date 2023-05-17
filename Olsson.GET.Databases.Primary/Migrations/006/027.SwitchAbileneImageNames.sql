Update dbo.Models set imageid = (select id from images where name = 'abilene50yr') where name = 'Abilene';
go

Update dbo.Models set imageid = (select id from images where name = 'abilene') where name = 'Abilene Calibrated';
go