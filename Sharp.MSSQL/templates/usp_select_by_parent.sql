

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_[%tablename%]_sel_by_parent_[%parenttable%][%fks%]]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_[%tablename%]_sel_by_parent_[%parenttable%][%fks%]]
GO

CREATE PROCEDURE [dbo].[usp_[%tablename%]_sel_by_parent_[%parenttable%][%fks%]] (
  [%additionalparams%]
  [%params%]
  
) as
-- Select a set of [%tablename%] table records by  [%parenttable%][%fks%].
--  [%datestamp%] - auto generated
BEGIN
  SET NOCOUNT ON;
  SELECT
    A.* 
  FROM [dbo].[[%tablename%]] A [%joins%]
  WHERE 
    [%whereitems%]
    REF_[%toptable%].[%fieldname%] = @[%fieldname%]  AND 
    A.is_deleted = 0
END
GO