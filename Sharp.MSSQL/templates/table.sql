-- ****** [%tablename%] creation....
CREATE TABLE [dbo].[[%tablename%]](
 [%columns%]
)
GO
-- [%tablename%] PK constraint 
ALTER TABLE [dbo].[[%tablename%]] ADD CONSTRAINT [PK_[%tablename%]] PRIMARY KEY [%pk_clustering%] ([%columnnames%])
GO

 


 

