create type DateList as TABLE ( [Date] DATETIME)

create table dbo.ModelStressPeriodCustomStartDates (
	ModelStressPeriodCustomStartDateID int not null identity(1,1) constraint PK_ModelStressPeriodCustomStartDates_ModelStressPeriodCustomStartDateID primary key,
	ModelID int not null constraint FK_ModelStressPeriodCustomStartDates_Models_ModelID_Id foreign key references dbo.Models (Id),
	StressPeriod int not null,
	StressPeriodStartDate datetime not null
)