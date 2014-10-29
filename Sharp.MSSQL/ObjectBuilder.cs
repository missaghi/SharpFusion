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
using Sharp;
using Sharp.EndPoints; 


namespace Sharp.ObjectBuilder
{ 
    //todo update .proj file to include new objects / remove deelted ones.


    public partial class ObjectBuilder : EndpointHandler
    {
        Template localTemplate { get; set; }

        [Sharp.EndPoints.Endpoint("ObjectBuilder")]
        public void BuildObjects() 
        {
            HttpContext context = HttpContext.Current;
            context.Response.Clear();
            context.Response.ContentType = "text/plain";
            context.Response.Buffer = false;
            context.Response.Write("Building the POCO ORM files, a new folder called object will be added to your project, add the the files in there. They are partial classes that you can augment in a different folder so that your custom stuff doesn't get overwritten next time you run this.");

            String connstr = ConfigurationManager.ConnectionStrings["connstr"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connstr))
            {
                conn.Open();
                Server sv = new Server(new Microsoft.SqlServer.Management.Common.ServerConnection(conn));

                // sv.SetDefaultInitFields(typeof(StoredProcedureParameter), new String[] { "IsOutputParameter", "Name", "DataType", "Length" });
                sv.SetDefaultInitFields(typeof(StoredProcedure), "IsSystemObject");



                // Parse the coonectionstring to a dictionary
                String dbname = connstr.ToLower().Split(";").Cast<String>().ToDictionary(x => x.Split("=")[0], x => x.Split("=")[1])["initial catalog"];
                Database db = sv.Databases[dbname];

                string folder =  Path.GetFullPath( Utility.AssemblyDirectory + "../../" );//context.Server.MapPath("/") + "../" + ConfigurationManager.AppSettings["ProjectName"] + "/");

                if (!Directory.Exists(folder + "objects/dynamic/backup/"))
                {
                    Directory.CreateDirectory(folder + "objects");
                    Directory.CreateDirectory(folder + "objects/dynamic");
                    Directory.CreateDirectory(folder + "objects/dynamic/backup");
                }


                foreach (String oldfile in Directory.GetFiles(folder + "objects/dynamic/backup/"))
                {
                    File.Delete(oldfile);
                }

                foreach (String oldfile in Directory.GetFiles(folder + "objects/dynamic/"))
                {
                    File.Move(oldfile, oldfile.Replace("objects/dynamic/", "objects/dynamic/backup/").Replace(".cs", ".cs.bak"));
                }

                foreach (Table tbl in db.Tables)
                {
                    
                    String Filename = folder + "objects/dynamic/" + tbl.Name + "-obj.cs";
                    String Backup = folder + "objects/dynamic/backup/" + tbl.Name + "-obj.bak";
                    
                    //TextWriter tw = new StreamWriter(context.Server.MapPath("/objects/" + Query["name"]+ ".cs"));
                    localTemplate = new Template(Resources<ObjectBuilder>.Read["ObjectBuilder.cs"]);
                    localTemplate.Set("namespace",ConfigurationManager.AppSettings["ProjectName"].Else("DAL"));

                    BuildObj(db, tbl, context);

                    File.WriteAllText(Filename, localTemplate.ToString());
                }
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


        public void BuildObj(Database db, Table table, HttpContext context)
        { 
            string tablename = table.Name;

            foreach (Index idx in table.Indexes)
            {
                if (!idx.IndexedColumns.Contains("id"))
                {
                    Template child = new Template(Resources<ObjectBuilder>.Read["ObjectTemplate_fk.cs"]);
                    child.Set("ltablename", tablename);
                    child.Set("tablename", tablename.Capitalize());
                    child.Set("fkey", idx.IndexedColumns[0].Name);
                    child.Set("fkeytype", ConvertToCLR(table.Columns[idx.IndexedColumns[0].Name].DataType.SqlDataType));
                    child.Set("list", localTemplate["list"]);
                    localTemplate.Append("FKs", child.ToString());
                }
            }

            foreach (Column col in db.Tables[tablename].Columns)
            {
                bool nullable = !ConvertToCLR(col.DataType.SqlDataType).LikeOne(new string[] {"String", "Byte[]"}) && !col.Name.Like("id") && !col.Name.Contains("_id");
                localTemplate.Append("properties", string.Format("\n\t\tpublic {0}{2} {1} {{ get; set; }}", ConvertToCLR(col.DataType.SqlDataType), col.Name, nullable.ToCustom("?", "")));
                if (nullable)
                {
                    localTemplate.Append("list", string.Format("\n\t\t\t\t\tif (rs1.{0}.HasValue) _{1}.{0} = rs1.{0}.Value;", col.Name, tablename.Capitalize()));
                    localTemplate.Append("select", string.Format("\n\t\t\t\t\tif (rs1.{0}.HasValue) this.{0} = rs1.{0}.Value;;", col.Name));
                }
                else
                {
                    localTemplate.Append("list", string.Format("\n\t\t\t\t\t_{1}.{0} = rs1.{0};", col.Name, tablename.Capitalize()));
                    localTemplate.Append("select", string.Format("\n\t\t\t\t\tthis.{0} = rs1.{0};", col.Name));
                }
            }

            foreach (StoredProcedureParameter param in db.StoredProcedures["usp_" + tablename + "_ups"].Parameters)
            {
                if (!param.IsOutputParameter) 
                localTemplate.Append("upsert", string.Format("\n\t\t\t\t\tdal.{0} = this.{0};", param.Name.Replace("@", "")));
            }

           

            //getlists
            foreach (Table tbl in db.Tables)
            { 
                foreach (ForeignKey fk in tbl.ForeignKeys)
                {
                    if (fk.Parent.Name.Like(tablename))
                    {
                        //lists
                        Template child = new Template(Resources<ObjectBuilder>.Read["ObjectTemplate_fk.cs"]);
                        child.Set("ltablename", tablename);
                        child.Set("tablename", tablename.Capitalize());
                        child.Set("reftable", fk.ReferencedTable.Capitalize());
                        child.Set("fkey", fk.Columns[0].Name);
                        child.Set("fkeytype", ConvertToCLR(tbl.Columns[fk.Columns[0].Name].DataType.SqlDataType));
                        child.Set("list", localTemplate["list"]);
                        localTemplate.Append("FKs", child.ToString());

                        //parents
                        Template parent = new Template(Resources<ObjectBuilder>.Read["ObjectTemplate_parent.cs"]);
                        parent.Set("ltablename", "parent_" + fk.ReferencedTable.ToLower());
                        parent.Set("tablename", fk.ReferencedTable.Capitalize());
                        parent.Set("fkey", fk.Columns[0].Name);
                        localTemplate.Append("Parents", parent.ToString());
                    }

                    if (fk.ReferencedTable.Like(tablename))
                    {
                        //children
                        Template child = new Template(Resources<ObjectBuilder>.Read["ObjectTemplate_child.cs"]);
                        List<String> columns = new List<string>();
                        foreach (ForeignKeyColumn col in fk.Columns)
                            columns.Add(col.Name);
                        child.Set("ltablename", fk.Parent.Name.ToLower() + "_" + String.Join("_", columns.ToArray()));
                        child.Set("tablename", fk.Parent.Name.ToLower().Capitalize());
                        child.Set("reftable", fk.ReferencedTable.Capitalize());
                        child.Set("refkey", "id");
                        child.Set("fkey", String.Join("_", columns.ToArray()));
                        localTemplate.Append("Children", child.ToString());
                    }

                }
            }


            localTemplate.Set("tablename", tablename.Capitalize());
            localTemplate.Set("ltablename", tablename);
        }

        #region IHttpHandler Members

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            
        }

        #endregion
    }
}

	
 		
 		