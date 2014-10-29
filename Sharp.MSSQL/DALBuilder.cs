using System;
using System.Configuration;
using System.Collections.Generic;
using System.Web.Services;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Linq;
using Microsoft.SqlServer.Management.Smo;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Sharp;
using Sharp.EndPoints;

namespace Sharp
{

    public partial class DALBuilder : EndpointHandler 
    {
        [Sharp.EndPoints.Endpoint("DALBuilder")]
        public void BuildDAL()  
        {
            string AssemblyDirectory = Utility.AssemblyDirectory;

            
            String connstr;
            try
            {
                connstr = ConfigurationManager.ConnectionStrings["connstr"].ConnectionString;
            }
            catch (Exception e)
            {
                throw new Exception("Please put a connection string in the web.config called connstr. eg" + @"
                <connectionStrings>
                    <add name=""connstr"" connectionString=""Data Source=localhost;Integrated Security=True;Initial Catalog=YourDBname""/>
                </connectionStrings>");
            }

            using (SqlConnection conn = new SqlConnection(connstr))
            {
                conn.Open();
                Server sv = new Server(new Microsoft.SqlServer.Management.Common.ServerConnection(conn));

                // sv.SetDefaultInitFields(typeof(StoredProcedureParameter), new String[] { "IsOutputParameter", "Name", "DataType", "Length" });
                //sv.SetDefaultInitFields(typeof(StoredProcedure), "IsSystemObject");

                // Parse the coonectionstring to a dictionary
                String dbname = connstr.ToLower().Split(";").Cast<String>().ToDictionary(x => x.Split("=")[0], x => x.Split("=")[1])["initial catalog"];
                Database db = sv.Databases[dbname];

                string folder = Path.GetFullPath(AssemblyDirectory + "../../"); //context.Server.MapPath("/") + "../" + ConfigurationManager.AppSettings["ProjectName"] + "/");
                //string folder = Path.GetFullPath(context.Server.MapPath("/") + "../DataAccessLayer/");
                if (File.Exists(folder + "dal.cs"))
                {
                    if (File.Exists(folder + "dal.cs.bak"))
                        File.Delete(folder + "dal.cs.bak");
                    File.Copy(folder + "dal.cs", folder + "dal.cs.bak");
                }

                if (HttpContext.Current != null)
                {
                    HttpContext context = HttpContext.Current;
                    context.Response.Clear();
                    context.Response.ContentType = "text/html";
                    context.Response.Buffer = false;

                    context.Response.Write("new file dal.cs is in the project folder root. add it to the project to compile. now browse to generate the object <br /><b>Procs</b> <br /> <br />");


                    context.Response.Write(folder);
                }

                StringBuilder sb = new StringBuilder();
                //TextWriter tw = new StreamWriter(context.Server.MapPath("/dal.cs"));
                
                    GenCode(db, sb);
                    File.WriteAllText(folder + "dal.cs", sb.ToString()); 
            } 
        }

        public String ConvertToCLR(SqlDataType sqltype)
        {
            String rtrn = "String";
            switch (sqltype)
            {
                case SqlDataType.BigInt: rtrn = "Int64"; break;
                case SqlDataType.Binary: rtrn = "Byte[]"; break;
                case SqlDataType.Bit: rtrn = "Boolean"; break;
                case SqlDataType.Char: rtrn = "String"; break;
                case SqlDataType.DateTime: rtrn = "DateTime"; break;
                case SqlDataType.Decimal: rtrn = "Decimal"; break;
                case SqlDataType.Float: rtrn = "Double"; break;
                case SqlDataType.Image: rtrn = "String"; break;
                case SqlDataType.Int: rtrn = "Int32"; break;
                case SqlDataType.Money: rtrn = "Decimal"; break;
                case SqlDataType.NChar: rtrn = "String"; break;
                case SqlDataType.NText: rtrn = "String"; break;
                case SqlDataType.NVarChar: rtrn = "String"; break;
                case SqlDataType.NVarCharMax: rtrn = "String"; break;
                case SqlDataType.None: rtrn = "String"; break;
                case SqlDataType.Numeric: rtrn = "Decimal"; break;
                case SqlDataType.Real: rtrn = "Single"; break;
                case SqlDataType.SmallDateTime: rtrn = "DateTime"; break;
                case SqlDataType.SmallInt: rtrn = "Int16"; break;
                case SqlDataType.SmallMoney: rtrn = "Decimal"; break;
                case SqlDataType.SysName: rtrn = "String"; break;
                case SqlDataType.Text: rtrn = "String"; break;
                case SqlDataType.Timestamp: rtrn = "String"; break;
                case SqlDataType.TinyInt: rtrn = "Byte"; break;
                case SqlDataType.UniqueIdentifier: rtrn = "Guid"; break;
                case SqlDataType.UserDefinedDataType: rtrn = "String"; break;
                case SqlDataType.UserDefinedType: rtrn = "String"; break;
                case SqlDataType.VarBinary: rtrn = "Byte[]"; break;
                case SqlDataType.VarBinaryMax: rtrn = "Byte[]"; break;
                case SqlDataType.VarChar: rtrn = "String"; break;
                case SqlDataType.VarCharMax: rtrn = "String"; break;
                case SqlDataType.Variant: rtrn = "String"; break;
                case SqlDataType.Xml: rtrn = "String"; break;
                default: break;
            }
            return rtrn;
        }

        public void GenCode(Database db, StringBuilder gencode)
        {
            HttpContext context = HttpContext.Current;

            gencode.Append(@"using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using Sharp;  

// Code Auto Generated " + DateTime.Now.ToString() + @"
namespace " + "DAL.Procs" + @"
{

");
            int count = 0;
            String error = "";

            foreach (StoredProcedure sp in db.StoredProcedures)
            {
                //try
                {
                    StringBuilder tw = new StringBuilder();
                    if (count < 10)
                    {
                        //count++;

                        if (!sp.IsSystemObject && sp.Name.ToLower().IndexOf("usp_") == 0) // sp.Owner == "dbo" && 
                        {

                            context.Response.Write(sp.Name + "<br>");

                            tw.Append("\npublic partial class " + sp.Name + "  : Sharp.DAL  \n{");

                            List<StoredProcedureParameter> param = new List<StoredProcedureParameter>();


                            foreach (StoredProcedureParameter par in sp.Parameters)
                            {
                                param.Add(par);

                                tw.Append("\n\n\t///<summary>  " + par.Properties["Length"].Value.ToString() + " " + (par.IsOutputParameter == true ? "output" : "") + " </summary>");
                                string isnullable = (ConvertToCLR(par.DataType.SqlDataType) == "String" || ConvertToCLR(par.DataType.SqlDataType) == "Byte[]" || par.Name.Contains("_id") || par.Name.Replace("@", "").Like("id") ? "" : "?");
                                tw.Append("\n\tpublic " + ConvertToCLR(par.DataType.SqlDataType) + isnullable + " " + par.Name.Replace("@", "") + "{get;set;}");
                            }

                            tw.Append("\n");

                            tw.Append("\n\n\tpublic " + sp.Name + "() {");

                            tw.Append("\n\n\t\tdb = new DB(\"" + sp.Name + "\");");
                            tw.Append("\n\t}");


                            tw.Append("\n\n\tprivate void preExecute() {");
                            foreach (StoredProcedureParameter par in param)
                            {
                                if (ConvertToCLR(par.DataType.SqlDataType) == "String" && (par.DataType.SqlDataType != SqlDataType.Xml))
                                    tw.Append("\n\t\tif (" + par.Name.Replace("@", "") + " == null)  db.NewParameter(\"" + par.Name.Replace("@", "") + "\", SqlDbType." + par.DataType.SqlDataType.ToString().Replace("Numeric", "Decimal").Replace("VarCharMax", "VarChar") + ", " + par.Properties["Length"].Value + ", DBNull.Value, " + par.IsOutputParameter.ToString().ToLower() + "); else  db.NewParameter(\"" + par.Name.Replace("@", "") + "\", SqlDbType." + par.DataType.SqlDataType.ToString().Replace("Numeric", "Decimal").Replace("VarCharMax", "VarChar") + ", " + par.Properties["Length"].Value + ",Encode(" + par.Name.Replace("@", "") + "), " + par.IsOutputParameter.ToString().ToLower() + "); ");
                                else if (par.DataType.SqlDataType == SqlDataType.Xml)
                                    tw.Append("\n\t\tif (" + par.Name.Replace("@", "") + " == null)  db.NewParameter(\"" + par.Name.Replace("@", "") + "\", SqlDbType." + par.DataType.SqlDataType.ToString().Replace("Numeric", "Decimal").Replace("VarCharMax", "VarChar") + ", " + par.Properties["Length"].Value + ", DBNull.Value, " + par.IsOutputParameter.ToString().ToLower() + "); else  db.NewParameter(\"" + par.Name.Replace("@", "") + "\", SqlDbType." + par.DataType.SqlDataType.ToString().Replace("Numeric", "Decimal").Replace("VarCharMax", "VarChar") + ", " + par.Properties["Length"].Value + ", " + par.Name.Replace("@", "") + ", " + par.IsOutputParameter.ToString().ToLower() + "); ");
                                else if (ConvertToCLR(par.DataType.SqlDataType) == "Byte[]" || par.Name.Contains("_id") || par.Name.Replace("@", "").Like("id"))
                                    tw.Append("\n\t\tdb.NewParameter(\"" + par.Name.Replace("@", "") + "\", SqlDbType." + par.DataType.SqlDataType.ToString().Replace("Numeric", "Decimal").Replace("VarCharMax", "VarChar") + ", " + par.Properties["Length"].Value + ", " + par.Name.Replace("@", "") + ", " + par.IsOutputParameter.ToString().ToLower() + "); ");
                                else
                                    tw.Append("\n\t\tif (!" + par.Name.Replace("@", "") + ".HasValue)  db.NewParameter(\"" + par.Name.Replace("@", "") + "\", SqlDbType." + par.DataType.SqlDataType.ToString().Replace("Numeric", "Decimal").Replace("VarCharMax", "VarChar") + ", " + par.Properties["Length"].Value + ", DBNull.Value, " + par.IsOutputParameter.ToString().ToLower() + "); else  db.NewParameter(\"" + par.Name.Replace("@", "") + "\", SqlDbType." + par.DataType.SqlDataType.ToString().Replace("Numeric", "Decimal").Replace("VarCharMax", "VarChar") + ", " + par.Properties["Length"].Value + ", " + par.Name.Replace("@", "") + ", " + par.IsOutputParameter.ToString().ToLower() + "); ");
                            }
                            tw.Append("\n\t}");

                            tw.Append("\n\n\tprivate void postExecute() {");
                            foreach (StoredProcedureParameter par in param)
                            {
                                tw.Append("\n\t\tif (base.db.cmd.Parameters[\"" + par.Name.Replace("@", "") + "\"].Value != DBNull.Value) " + par.Name.Replace("@", "") + " = (" + ConvertToCLR(par.DataType.SqlDataType) + ")base.db.cmd.Parameters[\"" + par.Name.Replace("@", "") + "\"].Value;");
                            }
                            tw.Append("\n\t}");

                            int i = 0;
                            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connstr"].ConnectionString))
                            {
                                using (SqlCommand cmd = new SqlCommand())
                                {
                                    StringBuilder sb = new StringBuilder();
                                    StringBuilder sb1 = new StringBuilder();
                                    foreach (StoredProcedureParameter par in param)
                                    {
                                        sb.AppendFormat(" {0} {1} ,", par.Name, par.DataType.SqlDataType.ToString().Replace("Numeric", "Decimal").Replace("VarCharMax", "VarChar"));
                                        if (par.IsOutputParameter)
                                            sb1.AppendFormat(" {0} = {0} OUTPUT,", par.Name);
                                        else
                                            sb1.AppendFormat(" {0} = NULL,", par.Name);
                                    }
                                    conn.Open();
                                    cmd.Connection = conn;
                                    cmd.CommandText = @" 

SET FMTONLY ON  " +
                      (sb.ToString().NNOE() ? "DECLARE " + sb.ToString().Trim(',') : "") + @"  
EXEC [dbo].[" + sp.Name + "] \n" + sb1.ToString().Trim(',') + @"
SET FMTONLY OFF 
 ";
                                    context.Response.Write("<pre>" + cmd.CommandText + "</pre>");

                                    SqlDataReader dr = cmd.ExecuteReader();

                                    if (dr.FieldCount > 0)
                                    {

                                        do
                                        {
                                            i++;
                                            tw.Append("\n\n\tpublic class ResultSet" + i + " \n\t{");
                                            StringBuilder binding = new StringBuilder();


                                            //DataTable dt = dr.GetSchemaTable();
                                            for (int f = 0; f < dr.FieldCount; f++)
                                            {
                                                bool notnullable = ConvertToCLR((SqlDataType)Enum.Parse(typeof(SqlDataType), dr.GetDataTypeName(f), true)).LikeOne(new string[] { "String", "Byte[]" }) || dr.GetName(f).Contains("_id") || dr.GetName(f).Replace("@", "").Like("id");

                                                //tw.Append("\n\t\tpublic " + ConvertToCLR((SqlDataType)Enum.Parse(typeof(SqlDataType), dr.GetDataTypeName(f), true)) + ((SqlDataType)Enum.Parse(typeof(SqlDataType), dr.GetDataTypeName(f), true) == SqlDataType.DateTime).ToCustom("?", "") + " " + dr.GetName(f) + " {get;set;}");
                                                tw.Append("\n\t\tpublic " + ConvertToCLR((SqlDataType)Enum.Parse(typeof(SqlDataType), dr.GetDataTypeName(f), true)) + notnullable.ToCustom("", "?") + " " + dr.GetName(f) + " {get;set;}");
                                                binding.AppendFormat("\n\t\t\t\tif (dal[\"{0}\"] != DBNull.Value) rs" + i + ".{0} = ({1})dal[\"{0}\"];", dr.GetName(f), ConvertToCLR((SqlDataType)Enum.Parse(typeof(SqlDataType), dr.GetDataTypeName(f), true)));
                                            }


                                            tw.Append("\n\n\t\tpublic static ResultSet" + i + "[] Init(" + sp.Name + " dal, StateBag sb) \n\t\t{");
                                            tw.Append("\n\t\t\tList<ResultSet" + i + "> rs" + i + "s = new List<ResultSet" + i + ">();");
                                            tw.Append("\n\t\t\twhile (dal.Read(sb))");
                                            tw.Append("\n\t\t\t{");
                                            tw.Append("\n\t\t\t\tResultSet" + i + " rs" + i + " = new ResultSet" + i + "();");

                                            tw.Append(binding.ToString());

                                            tw.Append("\n\t\t\t\t\trs" + i + "s.Add(rs" + i + ");;");
                                            tw.Append("\n\t\t\t}");
                                            tw.Append("\n\t\t\treturn rs" + i + "s.ToArray();");

                                            tw.Append("\n\t\t}");
                                            tw.Append("\n\t}");

                                        } while (dr.NextResult());
                                    }

                                    tw.Append(@"

    public new Boolean Execute(StateBag sb)
    {
        preExecute();
        bool flag = base.Execute(sb);
        if (flag)
        {
            //build all result sets
            ");
                                    for (int sets = 1; i >= sets; sets++)
                                    {
                                        tw.Append("RS" + sets + " = ResultSet" + sets + ".Init(this, sb);\n\t\t\t");
                                    }
                                    tw.Append(@" 

            postExecute();
            if (base.db.cmd.Parameters[""RetVal""].Value != DBNull.Value) Return = (int)base.db.cmd.Parameters[""RetVal""].Value;

        }
        else
        {
            //just instantiate empty result sets 
");
                                    for (int sets = 1; i >= sets; sets++)
                                    {
                                        tw.Append("RS" + sets + " = new ResultSet" + sets + " [] {};\n\t\t\t");
                                    }
                                    tw.Append(@" 
        }
        return flag;
    }");




                                }
                            }

                            for (int sets = i; sets > 0; sets--)
                            {
                                tw.Append("public ResultSet" + sets + "[] RS" + sets + " { get; set; }");
                            }





                            tw.Append("\n} \n");

                        }
                    }

                    gencode.Append(tw.ToString());

                }
                //catch (Exception e) { error += e.Message; context.Response.Write(e.Message); }
            }
            gencode.Append("}");

            context.Response.Write(error);


        }
         
    }
}
