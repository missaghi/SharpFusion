DECLARE @database nvarchar(50)
DECLARE @table nvarchar(50)

set @database = 'PlayGround' 

DECLARE @sql nvarchar(255)


 
 
WHILE EXISTS(select * from INFORMATION_SCHEMA.TABLE_CONSTRAINTS where constraint_catalog = @database and CONSTRAINT_TYPE = 'FOREIGN KEY' )
BEGIN
	select   @sql ='ALTER TABLE [' +TABLE_NAME + '] DROP CONSTRAINT [' + CONSTRAINT_NAME + ']'
	from    INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
	where    constraint_catalog = @database   and CONSTRAINT_TYPE = 'FOREIGN KEY'
    exec    sp_executesql @sql
END
 
 
WHILE EXISTS (select TABLE_NAME from INFORMATION_SCHEMA.TABLES where TABLE_CATALOG = @database)
BEGIN
 
 
		select  top 1 @sql ='DROP TABLE [' + TABLE_NAME + ']' 
		from    INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
		where    constraint_catalog = @database  
	   exec    sp_executesql @sql
	 
END
