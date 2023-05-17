insert into dbo.ExternalMapLayerType (ExternalMapLayerTypeID, ExternalMapLayerTypeName, ExternalMapLayerTypeDisplayName)
values (3, 'TokenProtectedESRIFeatureServer', 'Token Protected ESRI Feature Server (WFS / vector)')

alter table dbo.ExternalMapLayer
add Token varchar(255) null