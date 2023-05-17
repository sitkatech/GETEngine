alter table [dbo].[Runs]
	add ShouldCreateMaps BIT DEFAULT 0;
go

UPDATE [dbo].[Runs] SET ShouldCreateMaps = 0;