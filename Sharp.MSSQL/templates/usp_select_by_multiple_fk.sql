
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_[%tablename%]_sel_by_[%fks%]]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_[%tablename%]_sel_by_[%fks%]]
GO
CREATE PROCEDURE [dbo].[usp_[%tablename%]_sel_by_[%fks%]] (
  [%params%]
) as
-- Select a set of [%tablename%] table records by [%fks%].
--  [%datestamp%] - auto generated
BEGIN
  SET NOCOUNT ON;
  SELECT
    [%columns%] 
  FROM [dbo].[[%tablename%]] t
  WHERE 
    --t.[%fieldname%] = @[%fieldname%]  
    [%fieldnames%]  AND
    t.is_deleted = 0
END
GO