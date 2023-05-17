alter view [dbo].[vModelAndScenarioCountForCustomerID]
as
select c.CustomerID as PrimaryKey, c.CustomerID, count(distinct(ModelID)) as ModelCount, count(distinct(ScenarioID)) as ScenarioCount
from dbo.Customer c
left join dbo.CustomerModelScenario cms on c.CustomerID = cms.CustomerID
group by c.CustomerID
GO
