if exists (select * from dbo.sysobjects where id = object_id('dbo.vModelAndScenarioCountForCustomerID'))
	drop view dbo.vModelAndScenarioCountForCustomerID
go

create view dbo.vModelAndScenarioCountForCustomerID
as
select c.ID as CustomerID, count(distinct(ModelID)) as ModelCount, count(distinct(ScenarioID)) as ScenarioCount
from dbo.Customers c
left join dbo.CustomerModelScenarios cms on c.ID = cms.CustomerID
group by c.ID