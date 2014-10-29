using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using Sharp;

namespace Sharp
{
    #region dal

    public class DAL : IDisposable
    {
        public DB db { get; set; }
        public int Return { get; set; }
        public SqlDataReader dr { get { return db.dr; } }
        public Object this[string index]
        {
            get
            {
                return this[dr.GetOrdinal(index)];
            }
        }
        public Object this[int index]
        {
            get
            {
                //object obj = dr[index] ?? new object();
                //if (obj.GetType() == typeof(String))
                //    return HttpUtility.HtmlEncode(dr[index].ToString()); // encode all output
                //else
                return dr[index];
            }
        }

        public string Encode(string str)
        {
            return HttpUtility.HtmlEncode(str).Replace("'", "&#39;");
        }

        public static string Decode(string str)
        {
            return str;
        }

        public DAL()
        {
        } 

        public Boolean Read(StateBag sb)
        {
            bool moreData = sb.Valid;
            try
            {
                moreData = moreData && dr.Read();
            }
            catch (Exception ex)
            {
                //System.Diagnostics.Debug.Assert(false, ex.ToString());
                sb.ErrorMsg = ex.ToString();
                sb.ErrorMsg = "<br>Proc: " + db.cmd.CommandText;
                foreach(SqlParameter parm in db.cmd.Parameters)
                {
                    sb.ErrorMsg = ("<br>" + parm.ParameterName + ": " + parm.Value);
                }

                moreData = false;
                if (sb.Tn != null)
                {
                    dr.Close(); 
                }

            }
            if (!moreData && dr != null)
            {
                dr.Close(); //make output parameters available
            }

            return moreData;
        }
 

        public Boolean Execute(StateBag sb)
        {
            //execute
            db.ExecuteReader(sb);
            return sb.Valid;
        }
        public void Dispose()
        {
            db.Dispose();
        }
    }

    #endregion
}
