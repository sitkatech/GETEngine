/****** Object:  StoredProcedure [dbo].[pInsertBaseflowTableProcessingConfiguration]    Script Date: 2/22/2021 8:16:32 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pInsertBaseflowTableProcessingConfiguration]
	@baseflowTableIndicatorRegexPattern varchar(200),
	@segmentColumnNum int,
	@flowToAquiferColumnNum int,
	@reachColumnNum int
AS
BEGIN

	SET NOCOUNT ON;
	
	insert into dbo.BaseflowTableProcessingConfigurations(BaseflowTableIndicatorRegexPattern, SegmentColumnNum, FlowToAquiferColumnNum, ReachColumnNum)
	values (@baseflowTableIndicatorRegexPattern, @segmentColumnNum, @flowToAquiferColumnNum, @reachColumnNum)

END
GO


