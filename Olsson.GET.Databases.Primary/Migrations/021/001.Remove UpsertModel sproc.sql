IF EXISTS(SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.UpsertModel'))
	DROP PROCEDURE dbo.UpsertModel
go
