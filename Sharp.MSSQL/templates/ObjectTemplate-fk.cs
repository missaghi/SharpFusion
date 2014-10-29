        //get [%tablename%]s by [%reftable%] [%fkey%]
        public static IList<[%TableName%]> Get[%tablename%]sBy[%reftable%]_[%fKey%]([%fkeytype%] [%fKey%], StateBag sb)
        {

            List<[%TableName%]> _[%TableName%]s = new List<[%TableName%]>();
            using (DAL.Procs.usp_[%lTableName%]_sel_by_[%fkey%] dal = new DAL.Procs.usp_[%lTableName%]_sel_by_[%fkey%]())
            {
                dal.[%fKey%] = [%fKey%]; 
                dal.Execute(sb);
                foreach (DAL.Procs.usp_[%lTableName%]_sel_by_[%fkey%].ResultSet1 rs1 in dal.RS1)
                {
                    [%TableName%] _[%TableName%] = new [%TableName%](sb);
                    [%list%]
                    _[%TableName%]s.Add(_[%TableName%]);
                }

            }
            return _[%TableName%]s;
        }   