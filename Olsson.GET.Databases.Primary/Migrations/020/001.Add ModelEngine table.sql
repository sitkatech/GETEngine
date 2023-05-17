create table dbo.ModelEngineType (
	ModelEngineTypeID int not null constraint PK_ModelEngineType_ModelEngineTypeID primary key,
	ModelEngineTypeName varchar(100) not null constraint AK_ModelEngineType_ModelEngineTypeName unique,
	ModelEngineTypeDisplayName varchar(100) not null constraint AK_ModelEngineType_ModelEngineTypeDisplayName unique
)

create table dbo.ModelGridType (
	ModelGridTypeID int not null constraint PK_ModelGridType_ModelGridTypeID primary key,
	ModelGridTypeName varchar(100) not null constraint AK_ModelGridType_ModelGridTypeName unique,
	ModelGridTypeDisplayName varchar(100) not null constraint AK_ModelGridType_ModelGridTypeDisplayName unique
)


insert into dbo.ModelEngineType (ModelEngineTypeID, ModelEngineTypeName, ModelEngineTypeDisplayName)
values 
(1, 'Modpath', 'Modpath'),
(2, 'Modflow', 'Modflow'),
(3, 'Modflow6', 'Modflow 6'),
(4, 'IWFM', 'IWFM')


insert into dbo.ModelGridType (ModelGridTypeID, ModelGridTypeName, ModelGridTypeDisplayName)
values 
(1, 'Structured', 'Structured'),
(2, 'Unstructured', 'Unstructured')
