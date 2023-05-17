CREATE TYPE dbo.html FROM varchar(max) NULL
go

alter table dbo.Scenarios
add ScenarioDocumentation dbo.html null

alter table dbo.Models
add ModelDocumentation dbo.html null

create table dbo.ScenarioDocumentationImage (
	ScenarioDocumentationImageID int not null identity(1,1) constraint PK_ScenarioDocumentationImage_ScenarioDocumentationImageID primary key,
	ScenarioID int not null constraint FK_ScenarioDocumentationImage_Scenarios_ScenarioID foreign key references dbo.Scenarios(Id),
	FileResourceInfoID int not null constraint FK_ScenarioDocumentationImage_FileResourceInfos_FileResourceInfoID foreign key references dbo.FileResourceInfos(FileResourceInfoID)
)

create table dbo.ModelDocumentationImage (
	ModelDocumentationImageID int not null identity(1,1) constraint PK_ModelDocumentationImage_ModelDocumentationImageID primary key,
	ModelID int not null constraint FK_ModelDocumentationImage_Models_ModelID foreign key references dbo.Models(Id),
	FileResourceInfoID int not null constraint FK_ModelDocumentationImage_FileResourceInfos_FileResourceInfoID foreign key references dbo.FileResourceInfos(FileResourceInfoID)
)