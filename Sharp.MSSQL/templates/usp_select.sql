
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_[%tablename%]_sel]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_[%tablename%]_sel]
GO
CREATE PROCEDURE [dbo].[usp_[%tablename%]_sel] (
  @id bigint
) as
-- Select a record from [%tablename%] by id.
--  [%datestamp%] - auto generated
BEGIN
  SET NOCOUNT ON;
  SELECT
   [%columns%]
  FROM [dbo].[[%tablename%]] t
  WHERE 
    t.id = @id  
	AND t.is_deleted = 0
END
GO
