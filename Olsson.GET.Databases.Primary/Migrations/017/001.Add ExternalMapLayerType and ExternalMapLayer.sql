create table dbo.ExternalMapLayerType (
	ExternalMapLayerTypeID int not null constraint PK_ExternalMapLayerType_ExternalMapLayerTypeID primary key,
	ExternalMapLayerTypeName varchar(100) not null constraint AK_ExternalMapLayerType_ExternalMapLayerTypeName unique,
	ExternalMapLayerTypeDisplayName varchar(100) not null constraint AK_ExternalMapLayerType_ExternalMapLayerTypeDisplayName unique
)

insert into dbo.ExternalMapLayerType (ExternalMapLayerTypeID, ExternalMapLayerTypeName, ExternalMapLayerTypeDisplayName)
values (1, 'ESRIFeatureServer', 'ESRI Feature Server (WFS / vector)'),
(2, 'ESRIMapServer', 'ESRI Map Server (WMS / raster)')

create table dbo.ExternalMapLayer (
	ExternalMapLayerID int not null identity(1,1) constraint PK_ExternalMapLayer_ExternalMapLayerID primary key,
	ExternalMapLayerDisplayName varchar(100) not null constraint AK_ExternalMapLayer_ExternalMapLayerDisplayName unique,
	ExternalMapLayerTypeID int not null constraint FK_ExternalMapLayer_ExternalMapLayerType_ExternalMapLayerTypeID foreign key references dbo.ExternalMapLayerType (ExternalMapLayerTypeID),
	ExternalMapLayerURL varchar(500) not null constraint AK_ExternalMapLayer_ExternalMapLayerURL unique,
	LayerIsOnByDefault bit not null,
	IsActive bit not null,
	ExternalMapLayerDescription varchar(200) null
)