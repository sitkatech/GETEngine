Drop Table if exists dbo.FileResourceDatas
Drop Table if exists dbo.ReportTemplates
Drop Table if exists dbo.FileResourceInfos
Drop Table if exists dbo.FileResourceMimeTypes
Drop Table if exists dbo.ReportTemplateModels
Drop Table if exists dbo.ReportTemplateModelTypes

/****** Object:  Table [dbo].[FileResourceData]    Script Date: 9/9/2021 11:01:57 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FileResourceDatas](
	[FileResourceDataID] [int] IDENTITY(1,1) NOT NULL,
	[FileResourceInfoID] [int] NOT NULL,
	[Data] [varbinary](max) NOT NULL,
 CONSTRAINT [PK_FileResourceData_FileResourceDataID] PRIMARY KEY CLUSTERED 
(
	[FileResourceDataID] ASC
),
 CONSTRAINT [AK_FileResourceData_FileResourceDataID_FileResourceInfoID] UNIQUE NONCLUSTERED 
(
	[FileResourceDataID] ASC,
	[FileResourceInfoID] ASC
)
)
GO
/****** Object:  Table [dbo].[FileResourceInfo]    Script Date: 9/9/2021 11:01:57 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FileResourceInfos](
	[FileResourceInfoID] [int] IDENTITY(1,1) NOT NULL,
	[FileResourceMimeTypeID] [int] NOT NULL,
	[OriginalBaseFilename] [varchar](255) NOT NULL,
	[OriginalFileExtension] [varchar](255) NOT NULL,
	[FileResourceGUID] [uniqueidentifier] NOT NULL,
	[UserId] [int] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_FileResourceInfo_FileResourceInfoID] PRIMARY KEY CLUSTERED 
(
	[FileResourceInfoID] ASC
),
 CONSTRAINT [AK_FileResourceInfo_FileResourceGUID] UNIQUE NONCLUSTERED 
(
	[FileResourceGUID] ASC
)
)
GO
/****** Object:  Table [dbo].[FileResourceMimeType]    Script Date: 9/9/2021 11:01:57 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FileResourceMimeTypes](
	[FileResourceMimeTypeID] [int] NOT NULL,
	[FileResourceMimeTypeName] [varchar](100) NOT NULL,
	[FileResourceMimeTypeDisplayName] [varchar](100) NOT NULL,
	[FileResourceMimeTypeContentTypeName] [varchar](100) NOT NULL,
	[FileResourceMimeTypeIconSmallFilename] varchar(100) null,
	[FileResourceMimeTypeIconNormalFilename] varchar(100) null,
 CONSTRAINT [PK_FileResourceMimeType_FileResourceMimeTypeID] PRIMARY KEY CLUSTERED 
(
	[FileResourceMimeTypeID] ASC
),
 CONSTRAINT [AK_FileResourceMimeType_FileResourceMimeTypeDisplayName] UNIQUE NONCLUSTERED 
(
	[FileResourceMimeTypeDisplayName] ASC
),
 CONSTRAINT [AK_FileResourceMimeType_FileResourceMimeTypeName] UNIQUE NONCLUSTERED 
(
	[FileResourceMimeTypeName] ASC
)
)
GO
/****** Object:  Table [dbo].[ReportTemplate]    Script Date: 9/9/2021 11:01:57 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReportTemplates](
	[ReportTemplateID] [int] IDENTITY(1,1) NOT NULL,
	[FileResourceInfoID] [int] NOT NULL,
	[DisplayName] [varchar](50) NOT NULL,
	[Description] [varchar](250) NULL,
	[ReportTemplateModelTypeID] [int] NOT NULL,
	[ReportTemplateModelID] [int] NOT NULL,
 CONSTRAINT [PK_ReportTemplate_ReportTemplateID] PRIMARY KEY CLUSTERED 
(
	[ReportTemplateID] ASC
),
 CONSTRAINT [AK_ReportTemplate_DisplayName] UNIQUE NONCLUSTERED 
(
	[DisplayName] ASC
)
)
GO
/****** Object:  Table [dbo].[ReportTemplateModel]    Script Date: 9/9/2021 11:01:57 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReportTemplateModels](
	[ReportTemplateModelID] [int] NOT NULL,
	[ReportTemplateModelName] [varchar](100) NOT NULL,
	[ReportTemplateModelDisplayName] [varchar](100) NOT NULL,
	[ReportTemplateModelDescription] [varchar](250) NOT NULL,
 CONSTRAINT [PK_ReportTemplateModel_ReportTemplateModelID] PRIMARY KEY CLUSTERED 
(
	[ReportTemplateModelID] ASC
),
 CONSTRAINT [AK_ReportTemplateModel_ReportTemplateModelDisplayName] UNIQUE NONCLUSTERED 
(
	[ReportTemplateModelDisplayName] ASC
),
 CONSTRAINT [AK_ReportTemplateModel_ReportTemplateModelName] UNIQUE NONCLUSTERED 
(
	[ReportTemplateModelName] ASC
)
)
GO
/****** Object:  Table [dbo].[ReportTemplateModelType]    Script Date: 9/9/2021 11:01:57 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReportTemplateModelTypes](
	[ReportTemplateModelTypeID] [int] NOT NULL,
	[ReportTemplateModelTypeName] [varchar](100) NOT NULL,
	[ReportTemplateModelTypeDisplayName] [varchar](100) NOT NULL,
	[ReportTemplateModelTypeDescription] [varchar](250) NOT NULL,
 CONSTRAINT [PK_ReportTemplateModelType_ReportTemplateModelTypeID] PRIMARY KEY CLUSTERED 
(
	[ReportTemplateModelTypeID] ASC
),
 CONSTRAINT [AK_ReportTemplateModelType_ReportTemplateModelTypeDisplayName] UNIQUE NONCLUSTERED 
(
	[ReportTemplateModelTypeDisplayName] ASC
),
 CONSTRAINT [AK_ReportTemplateModelType_ReportTemplateModelTypeName] UNIQUE NONCLUSTERED 
(
	[ReportTemplateModelTypeName] ASC
)
)
GO
ALTER TABLE [dbo].[FileResourceDatas]  WITH CHECK ADD  CONSTRAINT [FK_FileResourceData_FileResourceInfo_FileResourceInfoID] FOREIGN KEY([FileResourceInfoID])
REFERENCES [dbo].[FileResourceInfos] ([FileResourceInfoID])
GO
ALTER TABLE [dbo].[FileResourceDatas] CHECK CONSTRAINT [FK_FileResourceData_FileResourceInfo_FileResourceInfoID]
GO
ALTER TABLE [dbo].[FileResourceInfos]  WITH CHECK ADD  CONSTRAINT [FK_FileResourceInfo_FileResourceMimeType_FileResourceMimeTypeID] FOREIGN KEY([FileResourceMimeTypeID])
REFERENCES [dbo].[FileResourceMimeTypes] ([FileResourceMimeTypeID])
GO
ALTER TABLE [dbo].[FileResourceInfos] CHECK CONSTRAINT [FK_FileResourceInfo_FileResourceMimeType_FileResourceMimeTypeID]
GO
ALTER TABLE [dbo].[FileResourceInfos]  WITH CHECK ADD  CONSTRAINT [FK_FileResourceInfo_Users_UserId_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[FileResourceInfos] CHECK CONSTRAINT [FK_FileResourceInfo_Users_UserId_UserId]
GO
ALTER TABLE [dbo].[ReportTemplates]  WITH CHECK ADD  CONSTRAINT [FK_ReportTemplate_FileResourceInfo_FileResourceInfoID] FOREIGN KEY([FileResourceInfoID])
REFERENCES [dbo].[FileResourceInfos] ([FileResourceInfoID])
GO
ALTER TABLE [dbo].[ReportTemplates] CHECK CONSTRAINT [FK_ReportTemplate_FileResourceInfo_FileResourceInfoID]
GO
ALTER TABLE [dbo].[ReportTemplates]  WITH CHECK ADD  CONSTRAINT [FK_ReportTemplate_ReportTemplateModel_ReportTemplateModelID] FOREIGN KEY([ReportTemplateModelID])
REFERENCES [dbo].[ReportTemplateModels] ([ReportTemplateModelID])
GO
ALTER TABLE [dbo].[ReportTemplates] CHECK CONSTRAINT [FK_ReportTemplate_ReportTemplateModel_ReportTemplateModelID]
GO
ALTER TABLE [dbo].[ReportTemplates]  WITH CHECK ADD  CONSTRAINT [FK_ReportTemplate_ReportTemplateModelType_ReportTemplateModelTypeID] FOREIGN KEY([ReportTemplateModelTypeID])
REFERENCES [dbo].[ReportTemplateModelTypes] ([ReportTemplateModelTypeID])
GO
ALTER TABLE [dbo].[ReportTemplates] CHECK CONSTRAINT [FK_ReportTemplate_ReportTemplateModelType_ReportTemplateModelTypeID]
GO