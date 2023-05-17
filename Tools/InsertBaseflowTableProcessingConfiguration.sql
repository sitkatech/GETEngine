/*----- DO NOT CHANGE -----*/
DECLARE @baseflowTableIndicatorRegexPattern varchar(200)
DECLARE @segmentColumnNum int
DECLARE @flowToAquiferColumnNum int
DECLARE @reachColumnNum int
/*-------------------------*/


/*----- Set These Values -----*/
/*A regex pattern that will match the entirety of the string immediately preceding the table that contains the Baseflow values.
To get help constructing the string, reach out to a developer or visit https://regex101.com/ to test one you create yourself.
The pattern created should be a match on the entire line (start pattern with ^ and end with $) to ensure we don't match on anything incorrectly. 
*/
SET @baseflowTableIndicatorRegexPattern = '^\s+STREAM LISTING\s+PERIOD\s+[0-9]+\s+STEP\s+[0-9]+$';

/*The column that contains the Segment Number (not zero-indexed). For Modflow6, this will contain the Reach number, as the concept of Segment and Reach no longer applies.*/
set @segmentColumnNum = 1

/*The column that contains the actual Baseflow data (not zero-indexed)*/
set @flowToAquiferColumnNum = 2

/*The column that contains the Reach number (not zero-indexed). This column should be set to null for any configurations that are for a Modflow6 or later executable*/
set @reachColumnNum = null

/*----- End Values to Set -----*/


/*----- DO NOT CHANGE -----*/
exec dbo.pInsertBaseflowTableProcessingConfiguration @baseflowTableIndicatorRegexPattern, @segmentColumnNum, @flowToAquiferColumnNum, @reachColumnNum
/*-------------------------*/