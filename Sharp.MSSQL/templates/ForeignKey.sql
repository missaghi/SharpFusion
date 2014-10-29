-- [%tablename%] FK constraint to [[%tablefkname%]] ([[%fieldfkname%]])
ALTER TABLE [dbo].[[%tablename%]] WITH CHECK ADD CONSTRAINT [FK_[%tablename%]_[%fieldname%]] FOREIGN KEY([[%fieldname%]])
REFERENCES [dbo].[[%tablefkname%]] ([[%fieldfkname%]])
GO
ALTER TABLE [dbo].[[%tablename%]] CHECK CONSTRAINT [FK_[%tablename%]_[%fieldname%]]
GO