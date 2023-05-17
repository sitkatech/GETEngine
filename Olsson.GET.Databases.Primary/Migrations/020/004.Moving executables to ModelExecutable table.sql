create table dbo.ModelExecutable
(
	ModelExecutableID int not null identity(1,1) constraint PK_ModelExecutable_ModelExecutableID primary key,
	ModelID int not null constraint FK_ModelExecutable_Model_ModelID foreign key references dbo.Model(ModelID),
	ExecutableName varchar(200) not null,
	Arguments varchar(200) null,
	RunOrder int not null,
	WorkingDirectory varchar(200) null,
	WrapWithBatchFile bit not null,
	UseShellExecute bit not null,
	RedirectStandardOutput bit not null,
	CreateNoWindow bit not null
)
GO

insert into dbo.ModelExecutable(ModelID, ExecutableName, Arguments, WorkingDirectory, WrapWithBatchFile, UseShellExecute, RedirectStandardOutput, CreateNoWindow, RunOrder)
select ModelID, EntryPointExeName as ExecutableName, SimulationFileName as Arguments, null as WorkingDirectory, 0 as WrapWithBatchFile, 0 as UseShellExecute, 1 as RedirectStandardOutput, 1 as CreateNoWindow, 10 as RunOrder
from dbo.Model
where ModelEngineTypeID = 1 and EntryPointExeName is not null

insert into dbo.ModelExecutable(ModelID, ExecutableName, Arguments, WorkingDirectory, WrapWithBatchFile, UseShellExecute, RedirectStandardOutput, CreateNoWindow, RunOrder)
select ModelID, EntryPointExeName as ExecutableName, NamFileName as Arguments, null as WorkingDirectory, 0 as WrapWithBatchFile, 0 as UseShellExecute, 1 as RedirectStandardOutput, 0 as CreateNoWindow, 10 as RunOrder
from dbo.Model
where ModelEngineTypeID in (2, 3) and EntryPointExeName is not null

insert into dbo.ModelExecutable(ModelID, ExecutableName, Arguments, WorkingDirectory, WrapWithBatchFile, UseShellExecute, RedirectStandardOutput, CreateNoWindow, RunOrder)
select ModelID, PostProcessingExeName as ExecutableName, null as Arguments, null as WorkingDirectory, case when PostProcessingExeName = 'zbud6.exe' then 0 else 1 end as WrapWithBatchFile, 0 as UseShellExecute, 1 as RedirectStandardOutput, 0 as CreateNoWindow, 20 as RunOrder
from dbo.Model
where ModelEngineTypeID in (2, 3) and PostProcessingExeName is not null

insert into dbo.ModelExecutable(ModelID, ExecutableName, Arguments, WorkingDirectory, WrapWithBatchFile, UseShellExecute, RedirectStandardOutput, CreateNoWindow, RunOrder)
select ModelID, '..\!bin\IWFM2015.0.961\Simulation2015_x64.exe' as ExecutableName, NamFileName as Arguments, 'Simulation' as WorkingDirectory, 0 as WrapWithBatchFile, 0 as UseShellExecute, 1 as RedirectStandardOutput, 0 as CreateNoWindow, 10 as RunOrder
from dbo.Model
where ModelEngineTypeID = 4 and EntryPointExeName is not null

insert into dbo.ModelExecutable(ModelID, ExecutableName, Arguments, WorkingDirectory, WrapWithBatchFile, UseShellExecute, RedirectStandardOutput, CreateNoWindow, RunOrder)
select ModelID, 'Budget.bat' as ExecutableName, null as Arguments, 'Budget' as WorkingDirectory, 0 as WrapWithBatchFile, 0 as UseShellExecute, 1 as RedirectStandardOutput, 0 as CreateNoWindow, 20 as RunOrder
from dbo.Model
where ModelEngineTypeID = 4 and PostProcessingExeName is not null

alter table dbo.Model drop column EntryPointExeName
alter table dbo.Model drop column PostProcessingExeName
alter table dbo.Model drop column NamFileName
alter table dbo.Model drop column SimulationFileName