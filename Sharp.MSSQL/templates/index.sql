
----- [%clustering%] Index for [%table_name%] on [%columns%]
IF EXISTS (SELECT name FROM sys.indexes
            WHERE name = N'C_IDX_[%table_name%]')
    DROP INDEX C_IDX_[%table_name%] ON [dbo].[[%table_name%]] 
GO
CREATE [%clustering%] INDEX C_IDX_[%table_name%] ON [dbo].[[%table_name%]] (
  [%columns%]
)
GO