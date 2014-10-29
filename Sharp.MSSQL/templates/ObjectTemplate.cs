using System;
using System.Collections.Generic;
using System.Linq;
using System.Web; 
using Sharp;
using Sharp;

namespace [%namespace%]
{
    public partial class [%TableName%]  : GenObjects
    {

        //Properties [%properties%]
        
        //Parents [%parents%]
        
        //Children [%children%]

        //default
        public [%TableName%](StateBag sb)
        {
            this.sb = sb;
        }

        //select one
        public [%TableName%](long ID, StateBag sb)
        {
            this.sb = sb;

            //select
            using (DAL.Procs.usp_[%lTableName%]_sel dal = new DAL.Procs.usp_[%lTableName%]_sel())
            {
                dal.id = ID;
                dal.Execute(sb);

                foreach (DAL.Procs.usp_[%lTableName%]_sel.ResultSet1 rs1 in dal.RS1)
                {
                     
[%select%]
                }
            }
        }

#region Lists        
[%FKs%]
#endregion

        public [%TableName%] UpSert() //GenObjects executinguser) 
        {
            //sb.Test(executinguser.AuthorizedLevel == AuthLevel.Write, "You are not authorized perform this action");

            preUpsertEvent(sb);

            using (DAL.Procs.usp_[%lTableName%]_ups dal = new DAL.Procs.usp_[%lTableName%]_ups())
            {
[%upsert%]
                dal.Execute(sb);

                if (dal.InsertedID.HasValue)
                    this.id = dal.InsertedID.Value;
            }
            postUpsertEvent(sb);

            return this;
        }

        public void Delete(bool HardDelete) //, GenObjects executinguser) 
        {  
           //sb.Test(executinguser.AuthorizedLevel == AuthLevel.Write, "You are not authorized perform this action");

            using (DAL.Procs.usp_[%lTableName%]_del dal = new DAL.Procs.usp_[%lTableName%]_del())
            {
                dal.id = this.id;
                dal.hard = HardDelete;
                dal.Execute(sb);
            }
        }


    }
}