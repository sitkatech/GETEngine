ALTER TABLE [dbo].[ScenarioFiles]
  ADD CONSTRAINT UC_ScenarioId_Name UNIQUE(ScenarioId, [Name]);