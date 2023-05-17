create table dbo.ExternalMapLayerCustomerModel (
	ExternalMapLayerCustomerModelID int not null identity(1,1) constraint PK_ExternalMapLayerCustomerModel_ExternalMapLayerCustomerModelID primary key,
	ExternalMapLayerID int not null constraint FK_ExternalMapLayerCustomerModel_ExternalMapLayer_ExternalMapLayerID foreign key references dbo.ExternalMapLayer (ExternalMapLayerID),
	CustomerID int not null constraint FK_ExternalMapLayerCustomerModel_Customer_CustomerID foreign key references dbo.Customer (CustomerID),
	ModelID int not null constraint FK_ExternalMapLayerCustomerModel_Model_ModelID foreign key references dbo.Model (ModelID),
	constraint AK_ExternalMapLayerCustomerModel_ExternalMapLayerID_CustomerID_ModelID unique (ExternalMapLayerID, CustomerID, ModelID)
)

alter table dbo.ExternalMapLayer
add IsAvailableForAllConfigurations bit null
go

update dbo.ExternalMapLayer
set IsAvailableForAllConfigurations = 1

alter table dbo.ExternalMapLayer
alter column IsAvailableForAllConfigurations bit not null