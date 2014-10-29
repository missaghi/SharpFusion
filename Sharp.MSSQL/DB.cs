using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Caching;

namespace Sharp
{

    public class DBTransaction : IDisposable
    {
        private StateBag SB { get; set; }

        public DBTransaction(StateBag sb)
        {
            SB = sb;
            CreateTransaction();
        }

        public void Dispose()
        { 
            Commit();
        }

        /// <summary>
        /// Creates a transaction that sits in the session items box.
        /// </summary>
        public void CreateTransaction()
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connstr"].ConnectionString);
            conn.Open();
            SB.Tn = conn.BeginTransaction(IsolationLevel.ReadUncommitted);
        }

        /// <summary>
        /// Commit transaction and exclude proceeding commands to be in a transaction;
        /// </summary>
        public void Commit()
        {
            if (SB.Valid)
            {
                if (SB.Tn != null)
                {
                    SB.Tn.Commit();
                    SB.Tn = null;
                }
            }
            else
                RollbackTransaction();
        }

        public void RollbackTransaction()
        {
            if (SB.Tn != null)
            {
                SB.Tn.Rollback();
                SB.Tn = null;
            }
        }
    }

    public class DB : IDisposable
    {
        
        private SqlConnection conn;
        public SqlCommand cmd { get; set; }
        public String Error { get; set; }
        public SqlDataReader dr { get; set; }  

        public  DB(String sp)
        {  
            //SqlCommand com = conn.CreateCommand();
            //com.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED";
            //com.CommandType = CommandType.Text;
            //if (conn.State == ConnectionState.Closed)
            //    conn.Open();
            //com.ExecuteNonQuery();
            //com.Dispose();

            cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = sp;
            cmd.CommandTimeout = 60;
        }

        public void Execute(ref StateBag sb)
        {

            if (sb.Valid)
            {
                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                System.Diagnostics.Stopwatch sp = new System.Diagnostics.Stopwatch();
                sp.Start();
                cmd.ExecuteNonQuery();
                HttpContext.Current.Items["Queries"] += ", " + cmd.CommandText + ":" + (Convert.ToDecimal(sp.ElapsedTicks) / Convert.ToDecimal(TimeSpan.TicksPerMillisecond)).ToString();
                sp.Stop();

            }

            CheckForErr(ref sb);
        }

        public void ExecuteReader( StateBag sb)
        { 
            if (sb.Valid)
            {
                if (sb.Tn == null)
                {
                    if (ConfigurationManager.ConnectionStrings["connstr"] == null) throw new Exception("Add connection string 'connstr' to web.config");
                    conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connstr"].ConnectionString);
                    cmd.Connection = conn;
                }
                else
                {
                    conn = sb.Tn.Connection;
                    cmd.Connection = conn;
                    cmd.Transaction = sb.Tn;
                }

                


                    if (conn.State == ConnectionState.Closed)
                        conn.Open();
                //store SP for error logging
                    if (HttpContext.Current != null && HttpContext.Current.IsDebuggingEnabled)
                {
                    if (HttpContext.Current.Items.Contains("LastSP"))
                        HttpContext.Current.Items["LastSP"] = DumpSqlData();
                    else
                        HttpContext.Current.Items.Add("LastSP", DumpSqlData());
                } 

                //EXECUTE
                if (sb.Valid)
                {

                    SqlParameter retval = new SqlParameter("RetVal", SqlDbType.Int);
                    retval.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(retval);
                    try
                    {
                        dr = cmd.ExecuteReader();
                    } 
                    catch (Exception ex)
                    {
                        //if (sb.Tn != null)
                        //{
                            //System.Diagnostics.Debug.Assert(false, ex.ToString());
                            sb.ErrorMsg = ex.Message.ToString(); 
                        //}
                    }
                }

            }

            //check result error
            CheckForErr(ref sb);
        }

        public void NewParameter(String name, SqlDbType type, Int16 size, object value, bool isOutput)
        {
            SqlParameter par = new SqlParameter();
            par.ParameterName = name;
            par.SqlDbType = type;
            if (isOutput) par.Direction = ParameterDirection.Output;
            par.Size = size;
            par.Value = value;
            this.Add(par);
        }

        public void CheckForErr(ref StateBag sb)
        {
            Boolean errflag = false;

            if (cmd.Parameters.Contains("errnum"))
            {
                if (cmd.Parameters["errnum"] != null)
                {
                    if (cmd.Parameters["errnum"].Value != null)
                    {
                        if (cmd.Parameters["errnum"].Value.ToString() != "0" && !String.IsNullOrEmpty(cmd.Parameters["errnum"].Value.ToString()))
                        {
                            errflag = true;
                            sb.Valid = false;
                            Error = cmd.Parameters["errnum"].Value.ToString();

                            if (cmd.Parameters["result"] != null)
                            {
                                if (cmd.Parameters["result"].Value != null)
                                {

                                    Error += "\n" + cmd.Parameters["result"].Value.ToString();

                                }
                            }
                        }
                    }
                }
            }

            if (cmd.Parameters.Contains("result") && !errflag)
            {
                if (cmd.Parameters["result"] != null)
                {
                    if (cmd.Parameters["result"].Value != null)
                    {
                        if (!String.IsNullOrEmpty(cmd.Parameters["result"].Value.ToString()) && !cmd.Parameters["result"].Value.ToString().ToLower().Equals("ok"))
                        {

                            errflag = true;
                            sb.Valid = false;
                            sb.ErrorMsg = "\n" + cmd.Parameters["result"].Value.ToString();

                        }
                    }
                }
            }

            if (errflag)
            {
                sb.Valid = false;
                sb.ErrorMsg = Error;
                if (HttpContext.Current.IsDebuggingEnabled)
                {
                    File.AppendAllText(HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["DBLog"]), DateTime.Now.ToString() + ", \"" + Error + "\", " + DumpSqlData() + "\n");
                }
            }
        }

        private String DumpSqlData()
        {
            String errDump = cmd.CommandText;
            foreach (SqlParameter par in cmd.Parameters)
            {
                errDump += ", \"" + (par.ParameterName + ": " + ((par.Value ?? DBNull.Value) == DBNull.Value ? "DBNULL" : par.Value.ToString()).Replace("\"", "\"\"").Replace("\n", "\\n") + "\"");
            }

            return errDump;
        }

        public void Add(SqlParameter par)
        {
            cmd.Parameters.Add(par);
        }

        #region IDisposable Members

  

        public void Dispose()
        {
            if (dr != null)
                dr.Dispose();

            if (cmd.Transaction == null)
            {
                if (conn != null)
                    conn.Close();
            }
            
            cmd.Dispose();
        }

        #endregion

    }
}
