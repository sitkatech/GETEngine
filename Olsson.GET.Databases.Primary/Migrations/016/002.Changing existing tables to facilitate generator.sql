SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

alter table dbo.ScenarioDocumentationImage drop constraint FK_ScenarioDocumentationImage_Scenarios_ScenarioID
alter table dbo.ScenarioDocumentationImage drop constraint FK_ScenarioDocumentationImage_FileResourceInfos_FileResourceInfoID
alter table dbo.ModelDocumentationImage drop constraint FK_ModelDocumentationImage_Models_ModelID
alter table dbo.ModelDocumentationImage drop constraint FK_ModelDocumentationImage_FileResourceInfos_FileResourceInfoID

exec sp_rename 'dbo.BaseflowTableProcessingConfigurations', 'BaseflowTableProcessingConfiguration'
exec sp_rename 'dbo.FileResourceMimeTypes', 'FileResourceMimeType'
exec sp_rename 'dbo.FileResourceInfos', 'FileResourceInfo'
exec sp_rename 'dbo.FileResourceDatas', 'FileResourceData'
exec sp_rename 'dbo.ReportTemplateModelTypes', 'ReportTemplateModelType';
exec sp_rename 'dbo.ReportTemplateModels', 'ReportTemplateModel';
exec sp_rename 'dbo.ReportTemplates', 'ReportTemplate';
GO

CREATE TABLE dbo.Customer
(
	CustomerID int not null IDENTITY(1,1) constraint PK_Customer_CustomerID primary key,
	CustomerName varchar(256) NOT NULL,
	IsTrial bit NOT NULL DEFAULT (0)
)

CREATE TABLE dbo.[Role](
	RoleID int NOT NULL constraint PK_Role_RoleID primary key,
	RoleName varchar(256) NOT NULL constraint AK_Role_RoleName unique,
	RoleDisplayName varchar(512) NOT NULL constraint AK_Role_RoleDisplayName unique,
	RoleCategory int NOT NULL DEFAULT (1)
)

CREATE TABLE dbo.[User](
	UserID int IDENTITY(1,1) NOT NULL constraint PK_User_UserID primary key,
	FullName varchar(256) NOT NULL,
	UserName nvarchar(256) NOT NULL,
	[Password] nvarchar(max) NULL,
	IsLockedOut bit NOT NULL,
	LockoutExpiration datetimeoffset(7) NULL,
	FailedAttemptCount int NOT NULL,
	SecurityStamp nvarchar(max) NULL,
	Email nvarchar(256) NULL,
	EmailConfirmed bit NOT NULL DEFAULT (1),
	CustomerID int NOT NULL constraint FK_User_Customer_CustomerID foreign key references dbo.Customer(CustomerID),
	PhoneNumber char(50) NULL,
	EulaAcceptedDate datetime NULL,
	CanManageReports bit NOT NULL
)

CREATE TABLE dbo.UserRole(
	UserRoleID int not null identity(1,1) constraint PK_UserRole_UserRoleID primary key,
	UserID int NOT NULL constraint FK_UserRole_User_UserID foreign key references dbo.[User](UserID) ON DELETE CASCADE,
	RoleID int NOT NULL constraint FK_UserRole_Role_RoleID foreign key references dbo.[Role](RoleID)
	CONSTRAINT AK_UserRole_UserID_RoleID unique(UserID, RoleID)
)

CREATE TABLE dbo.[Image](
	ImageID int NOT NULL constraint PK_Image_ImageID primary key,
	ImageName varchar(256) NOT NULL,
	[Server] varchar(256) NOT NULL,
	IsLinux bit NOT NULL default(0),
	CpuCoreCount int NULL,
	Memory decimal(4, 1) NULL
)

CREATE TABLE dbo.Model(
	ModelID int NOT NULL CONSTRAINT PK_Model_ModelID PRIMARY KEY,
	ModelName varchar(256) NOT NULL,
	ImageID int NOT NULL constraint FK_Model_Image_ImageID foreign key references dbo.[Image](ImageID),
	StartDateTime datetime NOT NULL,
	NamFileName varchar(50) NULL,
	RunFileName varchar(50) NULL,
	ModflowExeName varchar(50) NULL,
	AllowablePercentDiscrepancy float NULL,
	MapSettings varchar(1024) NULL,
	MapModelArea varchar(max) NULL,
	MapRunFileName varchar(50) NULL,
	IsDoubleSizeHeatMapOutput bit NOT NULL,
	InputZoneData varchar(max) NULL,
	NumberOfStressPeriods int NOT NULL DEFAULT (600),
	CanalData varchar(max) NULL,
	ZoneBudgetExeName varchar(50) NULL,
	ModpathExeName varchar(50) NULL,
	SimulationFileName varchar(50) NULL,
	BuddyGroup nvarchar(128) NULL,
	MapDrawdownFileName varchar(50) NULL,
	ListFileName varchar(50) NULL,
	OutputZoneData varchar(max) NULL,
	BaseflowTableProcessingConfigurationID int NULL CONSTRAINT FK_Model_BaseflowTableProcessingConfiguration_BaseflowTableProcessingConfigurationID FOREIGN KEY REFERENCES dbo.BaseflowTableProcessingConfiguration (BaseflowTableProcessingConfigurationID),
	ModelDescription varchar(500) NULL,
	ModelDocumentation dbo.html null
)

CREATE TABLE dbo.Scenario(
	ScenarioID int NOT NULL constraint PK_Scenario_ScenarioID primary key,
	ScenarioName varchar(256) NOT NULL,
	InputControlType int NOT NULL DEFAULT (0),
	ShouldSwitchSign bit NOT NULL DEFAULT (0),
	InputImageID int NULL constraint FK_Scenario_Image_InputImageID_ImageID foreign key references dbo.[Image](ImageID),
	ScenarioDescription varchar(500) NULL,
	ShowToAllUsersInScenarioList bit NOT NULL,
	ScenarioDocumentation dbo.html null
)

CREATE TABLE dbo.ModelStressPeriodCustomStartDate(
	ModelStressPeriodCustomStartDateID int IDENTITY(1,1) NOT NULL constraint PK_ModelStressPeriodCustomStartDate_ModelStressPeriodCustomStartDateID primary key,
	ModelID int NOT NULL constraint FK_ModelStressPeriodCustomStartDate_Model_ModelID FOREIGN KEY REFERENCES dbo.Model (ModelID),
	StressPeriod int NOT NULL,
	StressPeriodStartDate datetime NOT NULL
)

create table dbo.ModelScenario
(
    ModelScenarioID int not null identity(1,1) constraint PK_ModelScenario_ModelScenarioID primary key,
	ModelID int not null constraint FK_ModelScenario_Model_ModelID foreign key references dbo.Model(ModelID),
	ScenarioID int not null constraint FK_ModelScenario_Scenario_ScenarioID foreign key references dbo.Scenario(ScenarioID),
	constraint AK_ModelScenario_ModelID_ScenarioID unique (ModelID, ScenarioID)
)

create table dbo.CustomerModelScenario
(
    CustomerModelScenarioID int not null identity(1,1) constraint PK_CustomerModelScenario_CustomerModelScenarioID primary key,
    CustomerID int not null constraint FK_CustomerModelScenario_Customer_CustomerID foreign key references dbo.Customer(CustomerID),
	ModelID int not null constraint FK_CustomerModelScenario_Model_ModelID foreign key references dbo.Model(ModelID),
	ScenarioID int not null constraint FK_CustomerModelScenario_Scenario_ScenarioID foreign key references dbo.Scenario(ScenarioID),
	constraint AK_CustomerModelScenario_CustomerID_ModelID_ScenarioID unique (CustomerID, ModelID, ScenarioID)
)

create table dbo.ReportTemplateCustomerModelScenario
(
    ReportTemplateCustomerModelScenarioID int not null identity(1,1) constraint PK_ReportTemplateCustomerModelScenario_ReportTemplateCustomerModelScenarioID primary key,
	ReportTemplateID int NOT NULL CONSTRAINT FK_ReportTemplateCustomerModelScenario_ReportTemplate_ReportTemplateID FOREIGN KEY REFERENCES dbo.ReportTemplate (ReportTemplateID),
    CustomerID int not null constraint FK_ReportTemplateCustomerModelScenario_Customer_CustomerID foreign key references dbo.Customer(CustomerID),
	ModelID int not null constraint FK_ReportTemplateCustomerModelScenario_Model_ModelID foreign key references dbo.Model(ModelID),
	ScenarioID int not null constraint FK_ReportTemplateCustomerModelScenario_Scenario_ScenarioID foreign key references dbo.Scenario(ScenarioID),
	constraint AK_ReportTemplateCustomerModelScenario_ReportTemplateID_CustomerID_ModelID_ScenarioID unique (ReportTemplateID, CustomerID, ModelID, ScenarioID)
)


CREATE TABLE dbo.ScenarioFile(
	ScenarioFileID int NOT NULL constraint PK_ScenarioFile_ScenarioFileID primary key,
	ScenarioID int NOT NULL constraint FK_ScenarioFile_Scenario_ScenarioID foreign key references dbo.Scenario(ScenarioID),
	ScenarioFileName varchar(256) NOT NULL,
	ScenarioFileDescription varchar(512) NULL,
	IsRequired bit NOT NULL,
	CONSTRAINT AK_ScenarioFile_ScenarioID_ScenarioFileName UNIQUE (ScenarioID, ScenarioFileName)
)

CREATE TABLE dbo.VolumeUnit(
	VolumeUnitID int NOT NULL constraint PK_VolumeUnit_VolumeUnitID primary key,
	VolumeUnitName varchar(50) NOT NULL constraint AK_VolumeUnit_VolumeUnitName unique,
	VolumeUnitDisplayName varchar(50) NOT NULL constraint AK_VolumeUnit_VolumeUnitDisplayName unique
)


CREATE TABLE dbo.Run(
	RunID int IDENTITY(1,1) NOT NULL constraint PK_Run_RunID primary key,
	RunName varchar(256) NOT NULL,
	FileStorageLocator varchar(50) NOT NULL,
	ImageID int NULL constraint FK_Run_Image_ImageID foreign key references dbo.[Image](ImageID),
	ModelID int NOT NULL constraint FK_Run_Model_ModelID foreign key references dbo.Model(ModelID),
	ScenarioID int NOT NULL constraint FK_Run_Scenario_ScenarioID foreign key references dbo.Scenario(ScenarioID),
	UserID int NOT NULL constraint FK_Run_User_UserID foreign key references dbo.[User](UserID),
	CustomerID int NOT NULL constraint FK_Run_Customer_CustomerID foreign key references dbo.Customer(CustomerID),
	RunStatusID int NOT NULL constraint FK_Run_RunStatus_RunStatusID foreign key references dbo.RunStatus(RunStatusID),
	CreatedDate datetime NOT NULL,
	IsDeleted bit NOT NULL DEFAULT (0),
	InputFileName varchar(256) NULL,
	ProcessingStartDate datetime NULL,
	ProcessingEndDate datetime NULL,
	ShouldCreateMaps bit NULL DEFAULT (0),
	[Output] varchar(max) NULL,
	RestartCount int NOT NULL DEFAULT (0),
	InputVolumeUnitID int NOT NULL constraint FK_Run_VolumeUnit_InputVolumeUnitID_VolumeUnitID foreign key references dbo.VolumeUnit(VolumeUnitID),
	OutputVolumeUnitID int NOT NULL constraint FK_Run_VolumeUnit_OutputVolumeUnitID_VolumeUnitID foreign key references dbo.VolumeUnit(VolumeUnitID),
	IsDifferential bit NOT NULL DEFAULT (1),
	RunDescription varchar(max) NULL
)


CREATE TABLE dbo.RunGeography(
	RunGeographyID int IDENTITY(1,1) NOT NULL constraint PK_RunGeography_RunGeographyID primary key,
	RunID int NOT NULL CONSTRAINT FK_RunGeography_Run_RunID FOREIGN KEY REFERENCES dbo.Run (RunID) ON DELETE CASCADE,
	StressPeriod int NOT NULL,
	Color char(7) NOT NULL,
	[Geography] geography NULL
)


CREATE TABLE dbo.RunBucket(
	RunBucketID int IDENTITY(1,1) NOT NULL constraint PK_RunBucket_RunBucketID primary key,
	RunBucketName varchar(256) NOT NULL,
	CreatedDate datetime NOT NULL,
	UserID int NOT NULL constraint FK_RunBucket_User_UserID foreign key references dbo.[User](UserID),
	CustomerID int NOT NULL constraint FK_RunBucket_Customer_CustomerID foreign key references dbo.Customer(CustomerID),
	RunBucketDescription varchar(max) NULL
)

CREATE TABLE dbo.RunBucketRun(
	RunBucketRunID int IDENTITY(1,1) NOT NULL constraint PK_RunBucketRun_RunBucketRunID primary key,
	RunBucketID int NOT NULL constraint FK_RunBucketRun_RunBucket_RunBucketID foreign key references dbo.RunBucket(RunBucketID),
	RunID int NOT NULL constraint FK_RunBucketRun_Run_RunID foreign key references dbo.Run(RunID),
	CONSTRAINT AK_RunBucketRun_RunBucketID_RunID unique(RunBucketID, RunID)
)

GO


set identity_insert dbo.Customer on
insert into dbo.Customer(CustomerID, CustomerName, IsTrial)
select Id, [Name], IsTrial 
from dbo.Customers
order by Id
set identity_insert dbo.Customer off

insert into dbo.[Role](RoleID, RoleName, RoleDisplayName, RoleCategory)
select [Id], [Name], [Description], Category
from dbo.Roles
order by Id


set identity_insert dbo.[User] on
insert into dbo.[User](UserID, FullName, [UserName], [Password], [IsLockedOut], [LockoutExpiration], [FailedAttemptCount], [SecurityStamp], [Email], [EmailConfirmed], [CustomerID], [PhoneNumber], [EulaAcceptedDate], [CanManageReports])
select [Id], [Name], [UserName], [Password], [IsLockedOut], [LockoutExpiration], [FailedAttemptCount], [SecurityStamp], [Email], [EmailConfirmed], [CustomerId], [PhoneNumber], [EulaAcceptedDate], [CanManageReports]
from dbo.Users
order by Id
set identity_insert dbo.[User] off

insert into dbo.UserRole(UserID, RoleID)
select UserId, RoleId
from dbo.UserRoles
order by UserId, RoleId

insert into dbo.[Image](ImageID, ImageName, [Server], IsLinux, CpuCoreCount, Memory)
select [Id], [Name], [Server], [IsLinux], [CpuCoreCount], [Memory]
from dbo.[Images]
order by Id

insert into dbo.[Model](ModelID, ModelName, ImageID, StartDateTime, NamFileName, RunFileName, ModflowExeName, AllowablePercentDiscrepancy, MapSettings, MapModelArea, MapRunFileName, IsDoubleSizeHeatMapOutput, InputZoneData, NumberOfStressPeriods, CanalData, ZoneBudgetExeName, ModpathExeName, SimulationFileName, BuddyGroup, MapDrawdownFileName, ListFileName, OutputZoneData, BaseflowTableProcessingConfigurationID, ModelDescription, ModelDocumentation)
select [Id], [Name], [ImageId], [StartDateTime], [NamFileName], [RunFileName], [ModflowExeName], [AllowablePercentDiscrepancy], [MapSettings], [MapModelArea], [MapRunFileName], [IsDoubleSizeHeatMapOutput], [InputZoneData], [NumberOfStressPeriods], [CanalData], [ZoneBudgetExeName], [ModpathExeName], [SimulationFileName], [BuddyGroup], [MapDrawdownFileName], [ListFileName], [OutputZoneData], [BaseflowTableProcessingConfigurationID], [ModelDescription], ModelDocumentation
from dbo.[Models]
order by Id

insert into dbo.[Scenario](ScenarioID, ScenarioName, [InputControlType], [ShouldSwitchSign], [InputImageId], ScenarioDescription, [ShowToAllUsersInScenarioList], ScenarioDocumentation)
select [Id], [Name], [InputControlType], [ShouldSwitchSign], [InputImageId], [Description], [ShowToAllUsersInScenarioList], ScenarioDocumentation
from dbo.[Scenarios]
order by Id

set identity_insert dbo.ModelStressPeriodCustomStartDate on
insert into dbo.ModelStressPeriodCustomStartDate(ModelStressPeriodCustomStartDateID, ModelID, StressPeriod, StressPeriodStartDate)
select ModelStressPeriodCustomStartDateID, ModelID, StressPeriod, StressPeriodStartDate
from dbo.ModelStressPeriodCustomStartDates
order by ModelStressPeriodCustomStartDateID
set identity_insert dbo.ModelStressPeriodCustomStartDate off

insert into dbo.ModelScenario(ModelID, ScenarioID)
select ModelID, ScenarioID
from dbo.ModelScenarios

insert into dbo.CustomerModelScenario(CustomerID, ModelID, ScenarioID)
select CustomerID, ModelID, ScenarioID
from dbo.CustomerModelScenarios

insert into dbo.ReportTemplateCustomerModelScenario(ReportTemplateID, CustomerID, ModelID, ScenarioID)
select ReportTemplateID, CustomerID, ModelID, ScenarioID
from dbo.ReportCustomerModelScenarios

insert into dbo.ScenarioFile(ScenarioFileID, ScenarioID, ScenarioFileName, ScenarioFileDescription, IsRequired)
select [Id], [ScenarioId], [Name], [Description], [Required]
from dbo.ScenarioFiles
order by Id

insert into dbo.VolumeUnit(VolumeUnitID, VolumeUnitName, VolumeUnitDisplayName)
select [Id], VolumeType, VolumeType
from dbo.VolumeUnits
order by Id

set identity_insert dbo.Run on
insert into dbo.Run(RunID, RunName, FileStorageLocator, ImageID, ModelID, ScenarioID, UserID, CustomerID, RunStatusID, CreatedDate, IsDeleted, InputFileName, ProcessingStartDate, ProcessingEndDate, ShouldCreateMaps, [Output], RestartCount, InputVolumeUnitID, OutputVolumeUnitID, IsDifferential, RunDescription)
select [Id], [Name], [FileStorageLocator], [ImageId], [ModelId], [ScenarioId], [UserId], [CustomerId], [Status], [CreatedDate], [IsDeleted], [InputFileName], [ProcessingStartDate], [ProcessingEndDate], [ShouldCreateMaps], [Output], [RestartCount], [InputVolumeUnit], [OutputVolumeUnit], [IsDifferential], [Description]
from dbo.Runs
order by Id
set identity_insert dbo.Run off

set identity_insert dbo.RunGeography on
insert into dbo.RunGeography(RunGeographyID, RunID, StressPeriod, Color, [Geography])
select [Id], [RunId], [StressPeriod], [Color], [Geography]
from dbo.RunGeographies
order by Id
set identity_insert dbo.RunGeography off


set identity_insert dbo.RunBucket on
insert into dbo.RunBucket(RunBucketID, RunBucketName, CreatedDate, UserID, CustomerID, RunBucketDescription)
select [Id], [Name], [CreatedDate], [UserId], [CustomerId], [Description]
from dbo.RunBuckets
order by Id
set identity_insert dbo.RunBucket off


set identity_insert dbo.RunBucketRun on
insert into dbo.RunBucketRun(RunBucketRunID, RunBucketID, RunID)
select [Id], RunBucketID, RunID
from dbo.RunBucketRuns
order by Id
set identity_insert dbo.RunBucketRun off


alter table dbo.ScenarioDocumentationImage add constraint FK_ScenarioDocumentationImage_Scenario_ScenarioID foreign key (ScenarioID) references dbo.Scenario(ScenarioID)
alter table dbo.ScenarioDocumentationImage add constraint FK_ScenarioDocumentationImage_FileResourceInfo_FileResourceInfoID foreign key (FileResourceInfoID) references dbo.FileResourceInfo(FileResourceInfoID)
alter table dbo.ModelDocumentationImage add constraint FK_ModelDocumentationImage_Model_ModelID foreign key (ModelID) references dbo.Model(ModelID)
alter table dbo.ModelDocumentationImage add constraint FK_ModelDocumentationImage_FileResourceInfo_FileResourceInfoID foreign key (FileResourceInfoID) references dbo.FileResourceInfo(FileResourceInfoID)
