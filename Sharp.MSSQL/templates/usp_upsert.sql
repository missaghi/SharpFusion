
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_[%tablename%]_ups]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_[%tablename%]_ups]
GO
CREATE PROCEDURE [dbo].[usp_[%tablename%]_ups] (
	[%params%]@InsertedID bigint output 
)
AS
-- Upsert (insert, if @id exists update) a record in [%tablename%] table.
--  [%datestamp%] - auto generated
BEGIN
  SET NOCOUNT ON;
  
  DECLARE @now DATETIME = GETDATE()
  DECLARE @output_result TABLE (inserted_id bigint)

  -- merge in new tags for a blog
  MERGE dbo.[[%tablename%]] AS target
  USING (
    SELECT @id AS [id]
  ) AS source (id)
  ON (target.id = source.id)
  WHEN MATCHED THEN 
    UPDATE SET 
		[%update%]updated_dt=@now
  WHEN NOT MATCHED THEN
    INSERT (
		[%insert%]created_dt,
		is_deleted
    )
    VALUES (
		[%values%]@now,
		0
    )
  OUTPUT INSERTED.id into @output_result;
  SELECT @InsertedID = inserted_id FROM @output_result
END
GO
