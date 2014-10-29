

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_[%tablename%]_sel_by_[%fieldname%]]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_[%tablename%]_sel_by_[%fieldname%]]
GO

CREATE PROCEDURE [dbo].[usp_[%tablename%]_sel_by_[%fieldname%]] (
  @[%fieldname%] [%datatypesize%]
) as
-- Select a set of [%tablename%] table records by [%fieldname%].
--  [%datestamp%] - auto generated
BEGIN
  SET NOCOUNT ON;
  SELECT
    [%columns%] 
  FROM [dbo].[[%tablename%]] t
  WHERE 
    t.[%fieldname%] = @[%fieldname%]  AND
    t.is_deleted = 0
END
GO