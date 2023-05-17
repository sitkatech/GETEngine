insert into dbo.GETPageType (GETPageTypeID, GETPageTypeName, GETPageTypeDisplayName)
values (2, 'ExternalMapLayerList', 'External Map Layer List')

insert into dbo.GETPage (GETPageTypeID, GETPageContent)
values(2, 'Default content for ''External Map Layer List''');