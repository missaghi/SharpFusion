IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_[%tablename%]_del]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_[%tablename%]_del]
GO
CREATE PROCEDURE [dbo].[usp_[%tablename%]_del] (
  @id BIGINT,
  @hard bit
)
AS
-- Delete (soft) record in [%tablename%].
--  [%datestamp%] - auto generated
BEGIN
  SET NOCOUNT ON;
  
  if (@hard = 1) 
  BEGIN
	DELETE FROM [dbo].[[%tablename%]] WHERE id = @id
  END
  ELSE
  BEGIN 
	  DECLARE @now DATETIME = GETDATE()
  
	  UPDATE [dbo].[[%tablename%]] 
	  SET 
		is_deleted = 1,
		deleted_dt=@now
	  WHERE
		id = @id
	  RETURN
  END
END
GO
