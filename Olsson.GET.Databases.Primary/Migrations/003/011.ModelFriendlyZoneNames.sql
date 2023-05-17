alter table [dbo].[Models]
	add FriendlyZoneNamesFileName varchar(50) null;
go

update dbo.models set 
FriendlyZoneNamesFileName = 'ZoneNames.csv'
where id = 2;