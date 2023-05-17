CREATE TABLE [dbo].[ReportCustomerModelScenarios](
	[ReportTemplateID] [int] NOT NULL,
	[CustomerId] [int] NOT NULL,
	[ModelId] [int] NOT NULL,
	[ScenarioId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.ReportCustomerModelScenarios] PRIMARY KEY CLUSTERED 
(
	[ReportTemplateID] ASC,
	[CustomerId] ASC,
	[ModelId] ASC,
	[ScenarioId] ASC
)
)
GO

ALTER TABLE [dbo].[ReportCustomerModelScenarios]  WITH CHECK ADD  CONSTRAINT [FK_ReportCustomerModelScenarios_Cutomers] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customers] ([Id])
GO

ALTER TABLE [dbo].[ReportCustomerModelScenarios]  WITH CHECK ADD  CONSTRAINT [FK_ReportCustomerModelScenarios_Models] FOREIGN KEY([ModelId])
REFERENCES [dbo].[Models] ([Id])
GO

ALTER TABLE [dbo].[ReportCustomerModelScenarios]  WITH CHECK ADD  CONSTRAINT [FK_ReportCustomerModelScenarios_Scenarios] FOREIGN KEY([ScenarioId])
REFERENCES [dbo].[Scenarios] ([Id])
GO

ALTER TABLE [dbo].[ReportCustomerModelScenarios]  WITH CHECK ADD  CONSTRAINT [FK_ReportCustomerModelScenarios_ReportTemplates] FOREIGN KEY([ReportTemplateID])
REFERENCES [dbo].[ReportTemplates] ([ReportTemplateID])
GO


