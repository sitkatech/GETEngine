exec dbo.DropConstraintOnColumn 'dbo', 'Models', 'IsModflowSix';

ALTER TABLE [dbo].[Models]
DROP COLUMN [IsModflowSix];
