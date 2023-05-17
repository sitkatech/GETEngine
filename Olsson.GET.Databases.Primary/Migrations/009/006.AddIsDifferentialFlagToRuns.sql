ALTER TABLE dbo.[Runs] 
ADD [IsDifferential] BIT NOT NULL
CONSTRAINT DF__Runs__IsDifferential DEFAULT(1)
GO
