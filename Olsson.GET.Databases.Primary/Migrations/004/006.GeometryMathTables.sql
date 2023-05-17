CREATE TABLE [dbo].[RunGeographies](
    [Id] INT IDENTITY NOT NULL,
	[RunId] INT NOT NULL,
    [StressPeriod] INT NOT NULL,
	[Color] NCHAR(7) NOT NULL,
	[Geography] Geography,
 CONSTRAINT [PK_dbo.RunGeographies] PRIMARY KEY CLUSTERED 
(
    [Id] ASC
),
	CONSTRAINT FK_RunGeographies_Run FOREIGN KEY (RunId) REFERENCES dbo.Runs (Id) ON DELETE CASCADE
) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX IX_RunGeographies ON dbo.RunGeographies
	(
	RunId,
	StressPeriod,
	Color
	) INCLUDE ([Geography]) ON [PRIMARY]
GO