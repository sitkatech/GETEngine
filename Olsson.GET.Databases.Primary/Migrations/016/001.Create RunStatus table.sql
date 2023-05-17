Drop Table if exists dbo.RunStatus

CREATE TABLE dbo.RunStatus(
	RunStatusID int NOT NULL CONSTRAINT PK_RunStatus_RunStatusID PRIMARY KEY,
	RunStatusName varchar(100) NOT NULL CONSTRAINT AK_RunStatus_RunStatusName UNIQUE,
	RunStatusDisplayName varchar(100) NOT NULL CONSTRAINT AK_RunStatus_RunStatusDisplayName UNIQUE,
	RunStatusColor varchar(100) NOT NULL,
	IsTerminal bit not null
)
GO

delete from dbo.RunStatus

-- PDF (.pdf)
insert into dbo.RunStatus (RunStatusID, RunStatusName, RunStatusDisplayName, RunStatusColor, IsTerminal) 
values 
(0, 'Created', 'Created', '#e5ed4f', 0),
(1, 'Queued', 'Queued', '#e5ed4f', 0),
(2, 'Processing', 'Processing', '#e5ed4f', 0),
(3, 'Complete', 'Complete', '#23d776', 1),
(4, 'SystemError', 'System Error', '#db4142', 1),
(5, 'InvalidOutput', 'Invalid Output', '#db4142', 1),
(6, 'InvalidInput', 'Invalid Input', '#db4142', 1),
(7, 'HasDryCells', 'Completed with Dry Cells', '#23d776', 1),
(8, 'AnalysisFailed', 'Analysis Failed', '#db4142', 1),
(9, 'AnalysisSuccess', 'Analysis Succeeded', '#e5ed4f', 0),
(10, 'ProcesingInputs', 'Processing Inputs', '#e5ed4f', 0),
(11, 'RunningAnalysis', 'Running Analysis', '#e5ed4f', 0)
