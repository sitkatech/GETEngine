insert into dbo.images
(id, [Name], [Server]) values
(2, 'cohyst', '$ImageServerUri$');

insert into dbo.models 
(id, [Name], ImageId, StartDateTime, NamFileName, BaselineFileName, RunFileName, ZonesFileName, ModflowExeName, NodeFlowProportionsFileName, AllowablePercentDiscrepancy) values
(2, 'Cohyst', 2, '1984-10-01', 'COHYST2010_28b_14_28.nam', 'Baseline.dat', 'COHYST2010_28b_14_28_sfr.out', 'SegRchZones.csv', 'mf2005.exe', 'CanalCells.csv', 1);

insert into dbo.modelscenarios
(ModelId, ScenarioId) values
(2, 4);