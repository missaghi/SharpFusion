using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Text;
using System.IO;
using System.Web;
using Sharp;
using Sharp.EndPoints;

namespace Sharp.Schema
{

    public static class Extension
    {
        public static string SubStringPy(this string str, int start, int end)
        {
            if (str.Length + end > 0 && Math.Abs(end) < (str.Length - start) && start < str.Length)
                return str.Substring(start, str.Length - start + end);
            else
                return "";
        }

        public static bool Like(this string str, string str1)
        {
            if (String.IsNullOrEmpty(str) || String.IsNullOrEmpty(str1 ))
                return (String.IsNullOrEmpty(str) && String.IsNullOrEmpty(str1));
            else
                return str.Equals(str1, StringComparison.InvariantCultureIgnoreCase);
        } 
    }


    class Table
    {
        public string Name { get; set; }
        public List<FK> fks { get; set; }
        public List<Column> Columns { get; set; }

        public Table(String schematable, List<Column> Defaults)
        {
            fks = new List<FK>();
            Columns = new List<Column>();
            Columns.AddRange(Defaults);

            using (StringReader reader = new StringReader(schematable))
            {
                string line = reader.ReadLine();
                int length = line.Contains(" -") ? line.IndexOf(" -") : line.Length;
                this.Name = line.Substring(0, length).Trim();
                while ((line = reader.ReadLine()) != null)
                    Columns.Add(new Column(this, line));
            }
        }
    } 

    class Column
    {
        public Table ParentTable { get; set; }

        public string Name { get; set; }
        public string DataTypeSize { get; set; }
        public string defaultvalue { get; set; }
        public bool noparam_mod { get; set; }
        public bool identity { get; set; }
        public bool unique { get; set; } 
        public bool AllowNull { get; set; }
        public bool pk { get; set; }
        public bool fk { get; set; }
        public bool clu_idx { get; set; }
        public string idx_order { get; set; }
        public bool nc_idx { get; set; }

        public Column(Table parentTable, String fields)
        { 
            this.ParentTable = parentTable;
            fields = string.Join("\r\n", fields.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

            this.AllowNull = true;
            this.noparam_mod = false;
            this.identity = false;
            this.unique = false;
            this.clu_idx = false;
            this.nc_idx = false; 


            using (StringReader reader = new StringReader(fields))
            {
                string line; 
                    this.Name = reader.ReadLine().Trim();
                    this.DataTypeSize = reader.ReadLine().Trim();
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("fk(") && line.EndsWith(")"))
                    {
                        this.fk = true;
                        parentTable.fks.Add(new FK(this, line));
                    }
                    else if (line.StartsWith("clu_idx(") && line.EndsWith(")"))
                    {
                        this.clu_idx = true;
                        this.idx_order = line.SubStringPy(8, -1);
                    }
                    else if (line.StartsWith("nc_idx(") && line.EndsWith(")"))
                    {
                        this.nc_idx = true;
                        this.idx_order = line.SubStringPy(8, -1);
                    }
                    else if (line.Like("pk"))
                    {
                        this.pk = true;
                        parentTable.Columns.ForEach(x => x.pk = false);
                    }
                    else if (line.Like("notnull"))      this.AllowNull = false;
                    else if (line.Like("noparam_mod"))  this.noparam_mod = true;
                    else if (line.Like("ident"))        this.identity = true;
                    else if (line.Like("unique"))       this.unique = true;
                    else if (line.Like("clu_idx"))      this.clu_idx = true;
                    else if (line.Like("nc_idx"))       this.nc_idx = true; 

                }
            }
        }
    }


    class FK
    {
        public Column fkcolumn { get; set; }
        public string RefTable { get; set; }
        public string RefColumn { get; set; }

        public FK(Column column, string line)
        {
            fkcolumn = column;
            line = line.SubStringPy(3, -1);
            RefTable = line;
            RefColumn = "id";
            if (line.Contains(","))
            {
                RefTable = line.Split(new char[] { ',' })[0];
                RefColumn = line.Split(new char[] { ',' })[1];
            } 
        }
    }

    class Parse
    {
        public List<Table> Tables { get; set; }
        public List<Column> DefaultColumns { get; set; }

        public Parse(String schema)
        {
            Tables = new List<Table>();
            DefaultColumns = new List<Column>();

            using (StringReader reader = new StringReader(schema))
            { 
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    StringBuilder sb = new StringBuilder();
                    
                    if (!String.IsNullOrEmpty(line.Trim()))
                        sb.AppendLine(line.Trim());
                    while ((line = reader.ReadLine()) != null && !String.IsNullOrEmpty(line.Trim()))
                    {
                        sb.AppendLine(line.Trim());
                    }

                    if (sb.ToString().Length > 0)
                    {
                        if (sb.ToString().IndexOf("defaults\r\n") == 0)
                        {
                            DefaultColumns = new Table(sb.ToString(), DefaultColumns).Columns;
                        }
                        else
                            Tables.Add(new Table(sb.ToString(), DefaultColumns));
                    }
                }
            }  
        }
    }


    public class SchemaBuilder : EndpointHandler
    {

        public SchemaBuilder() { 
        
        }

        [Sharp.EndPoints.Endpoint("SchemaBuilder")]
        public void BuildSchema() //string[] args)
        {
            string AssemblyDirectory = Utility.AssemblyDirectory;
            string folder = Path.GetFullPath(AssemblyDirectory + "../../");
            string file = "schema.txt"; //args[0]
            string schema;
            try { schema = (File.ReadAllText(folder + file, Encoding.UTF8)); }
            catch (Exception e)
            {
                throw new Exception("Schema file not foound. Put a file called schema.txt in the app root." + folder);
            }
            Parse parse = new Parse(schema);

            string scriptsfolder = folder + "/GenScripts"; // +DateTime.Now.Ticks;

            Directory.CreateDirectory(scriptsfolder);

            StringBuilder TableScript = new StringBuilder();
            StringBuilder ProcScript = new StringBuilder();
            StringBuilder FksScript = new StringBuilder();
             
            foreach (Table tbl in parse.Tables)
            {
#region Tables
                Template tmp = new Template(Resources<SchemaBuilder>.Read["table.sql"]); 
                
                tmp.Append("tablename", tbl.Name);

                List<String> cols = new List<string>();

                //Columns
                foreach (Column col in tbl.Columns)
                    cols.Add(string.Format("[{0}] {1} {2} {3} {4} {5}", col.Name, col.DataTypeSize, col.identity ? "IDENTITY(1,1)" : "", col.AllowNull ? "NULL" : "NOT NULL", col.defaultvalue, col.unique ? "UNIQUE" : "" ));
                tmp.Append("columns", string.Join(",\r\n\t", cols.ToArray()));

                //Primary Keys
                var PrimaryKeys = from pks in tbl.Columns where pks.pk select pks.Name;
                tmp.Set("columnnames", String.Join(", ", PrimaryKeys.ToArray()));

                //clustering
                if (tbl.Columns.Count(x => x.clu_idx) > 0)  tmp.Set("pk_clustering", "NONCLUSTERED  ");  
                TableScript.Append (tmp.ToString());
#endregion

#region ClusteredIndexes
               
                tmp = new Template(Resources<SchemaBuilder>.Read["index.sql"]); 

                var ClusteredIndexes =  from clu in tbl.Columns where clu.clu_idx orderby clu.idx_order select clu.Name;
 
                tmp.Set("table_name", tbl.Name);
                tmp.Set("clustering", "CLUSTERED");
                tmp.Set("columns", String.Join(", ", ClusteredIndexes.ToArray()));

                if (ClusteredIndexes.Count() > 0)
                    TableScript.Append (tmp.ToString());
#endregion

                #region Select by CIndex
                foreach (String colname in ClusteredIndexes)
                {
                    tmp = new Template(Resources<SchemaBuilder>.Read["usp_select_by_fk.sql"]); 
                    tmp.Set("tablename", tbl.Name);
                    tmp.Set("datatypesize", tbl.Columns.Single(x => x.Name.Equals(colname)).DataTypeSize);
                    tmp.Set("fieldname", colname);
                    tmp.Append("columns", String.Join(", ", (from clu in tbl.Columns select "[" + clu.Name + "]").ToArray()));
                    ProcScript.Append(tmp.ToString());
                }
                #endregion

                #region Select by UniqueCol
                var UNIQUECols = from clu in tbl.Columns where clu.unique select clu;
                foreach (Column col in UNIQUECols)
                {
                    tmp = new Template(Resources<SchemaBuilder>.Read["usp_select_by_fk.sql"]); 
                    tmp.Set("tablename", tbl.Name);
                    tmp.Set("datatypesize", col.DataTypeSize);
                    tmp.Set("fieldname", col.Name);
                    tmp.Append("columns", String.Join(", ", (from clu in tbl.Columns select "[" + clu.Name + "]").ToArray()));
                    ProcScript.Append(tmp.ToString());
                }
                #endregion

#region NonClusteredIndexes

                tmp = new Template(Resources<SchemaBuilder>.Read["index.sql"]); 

                var NonClusteredIndexes = from clu in tbl.Columns  where clu.nc_idx orderby clu.idx_order select clu.Name;

                tmp.Set("table_name", tbl.Name);
                tmp.Set("clustering", "NONCLUSTERED");
                tmp.Set("columns", String.Join(", ", NonClusteredIndexes.ToArray()));

                if (NonClusteredIndexes.Count() > 0)
                    TableScript.Append(tmp.ToString());
#endregion

                #region Select by NCIndex
                foreach (String colname in NonClusteredIndexes)
                {
                    tmp = new Template(Resources<SchemaBuilder>.Read["usp_select_by_fk.sql"]);
                    tmp.Set("tablename", tbl.Name); 
                    tmp.Set("datatypesize", tbl.Columns.Single(x => x.Name.Equals(colname)).DataTypeSize); 
                    tmp.Set("fieldname", colname);
                    tmp.Append("columns", String.Join(", ", (from clu in tbl.Columns select clu.Name).ToArray()));
                    ProcScript.Append(tmp.ToString());
                }
#endregion

#region Upsert
                tmp = new Template(Resources<SchemaBuilder>.Read["usp_upsert.sql"]);
                tmp.Append("tablename", tbl.Name);

                //Columns
                foreach (Column col in tbl.Columns)
                {
                    tmp.Append("params", string.Format("@{0} {1},\r\n\t", col.Name, col.DataTypeSize, col.AllowNull ? "NULL" : "" ));
                    if (!col.Name.Like("id") && !col.Name.Like("updated_dt"))
                        tmp.Append("update", string.Format("[{0}] = ISNULL(@{0}, [{0}]),\r\n\t\t", col.Name));
                    if (!parse.DefaultColumns.Contains(col))
                    {
                        tmp.Append("insert", string.Format("[{0}],\r\n\t\t", col.Name));
                        tmp.Append("values", string.Format("@{0},\r\n\t\t", col.Name));
                    }
                } 

                ProcScript.Append (tmp.ToString());
#endregion

#region Delete
                tmp = new Template(Resources<SchemaBuilder>.Read["usp_delete.sql"]);
                tmp.Append("tablename", tbl.Name); 
                ProcScript.Append(tmp.ToString());
#endregion

#region Select
                tmp = new Template(Resources<SchemaBuilder>.Read["usp_select.sql"]);
                tmp.Append("tablename", tbl.Name);
                tmp.Append("columns", String.Join(", ", (from clu in tbl.Columns select "[" + clu.Name + "]").ToArray()));
                ProcScript.Append (tmp.ToString());
#endregion

#region Select by FK

                DelGenParent GenParent = null;
                GenParent = delegate(FK seedFK,  Template temp)
                {
                    foreach (FK pfk in parse.Tables.Single(x => x.Name.Like(seedFK.RefTable)).fks)
                    {
                        Template template = temp.Clone();
                        template.Append("joins", "\r\n\t\tINNER JOIN [dbo].[" + pfk.RefTable + "] REF_" + pfk.RefTable + " ON REF_" + pfk.fkcolumn.ParentTable.Name + ".[" + pfk.fkcolumn.Name + "] = REF_" + pfk.RefTable + ".[" + pfk.RefColumn + "]");
                        template.Set("toptable", pfk.RefTable);
                        template.Set("parenttable", pfk.RefTable);
                        template.Set("fieldname", pfk.RefColumn);
                        template.Set("datatypesize", tbl.Columns.Single(x=>x.Name.Equals(pfk.RefColumn)).DataTypeSize);
                        Column column = parse.Tables.Single(x => x.Name.Like(pfk.RefTable)).Columns.Single(x => x.Name.Like(pfk.RefColumn));
                        template.Set("params", "\r\n\t@" + pfk.RefColumn + " " + column.DataTypeSize);
                        ProcScript.Append(template.ToString());

                        if (parse.Tables.Single(x => x.Name.Like(pfk.RefTable)).fks.Count > 0  && !pfk.RefTable.Like(pfk.fkcolumn.ParentTable.Name)) //prevent recursive self joins
                            GenParent(pfk, template);
                    }
                };

                foreach (FK fk in tbl.fks)
                {
                    tmp = new Template(Resources<SchemaBuilder>.Read["usp_select_by_fk.sql"]);
                    tmp.Append("columns", String.Join(", ", (from clu in tbl.Columns select "[" + clu.Name + "]").ToArray()));
                    tmp.Set("tablename", tbl.Name);
                    tmp.Set("fieldname", fk.fkcolumn.Name);
                    tmp.Set("datatypesize", fk.fkcolumn.DataTypeSize); 

                    ProcScript.Append(tmp.ToString());

                    //parenttables
                    if (parse.Tables.Single(x => x.Name.Like(fk.RefTable)).fks.Count > 0)
                    {
                        tmp = new Template(Resources<SchemaBuilder>.Read["usp_select_by_parent.sql"]);
                        tmp.Set("tablename", tbl.Name);
                        tmp.Append("joins", "\r\n\t\tINNER JOIN [dbo].[" + fk.RefTable + "] REF_" + fk.RefTable + " ON A.[" + fk.fkcolumn.Name + "] = REF_" + fk.RefTable + ".[" + fk.RefColumn + "]");
                        GenParent(fk, tmp);
                    }
                }
#endregion

#region Select by FKs
      
                if (tbl.fks.Count > 1)
                { 
                    tmp = new Template(Resources<SchemaBuilder>.Read["usp_select_by_multiple_fk.sql"]);
                    tmp.Set("params", String.Join(",\r\n\t", (from fks in tbl.fks select "@" + fks.fkcolumn.Name + " " + fks.fkcolumn.DataTypeSize).ToArray()));
                    tmp.Set("columns", String.Join(", ", (from clu in tbl.Columns select  "[" + clu.Name + "]").ToArray()));
                    tmp.Set("tablename", tbl.Name);
                    var wheres = (from clu in tbl.fks select clu.fkcolumn.Name).ToList().ToArray();
                    tmp.Set("fks", String.Join("_and_", wheres.ToArray()));
                    
                    for (int i = 0; i < wheres.Length; i++)
                    {
                        wheres[i] = string.Format("t.{0} = @{0}", wheres[i]);
                    }
                    tmp.Set("fieldnames", String.Join(" AND \r\n\t\t", wheres.ToArray())); 
                    ProcScript.Append (tmp.ToString());

                    foreach (FK fk in tbl.fks)
                    {
                        //parenttables
                        tmp = new Template(Resources<SchemaBuilder>.Read["usp_select_by_parent.sql"]);
                        tmp.Append("additionalparams", String.Join(",\r\n\t", (from fks in tbl.fks where !fks.Equals(fk) select "@" + fks.fkcolumn.Name + " " + fks.fkcolumn.DataTypeSize).ToArray()) + ",");
                        var whereitems = (from clu in tbl.fks where !clu.Equals(fk) select clu.fkcolumn.Name).ToList().ToArray();
                        tmp.Set("fks", "_and_" + String.Join("_and_", whereitems));

                        for (int i = 0; i < whereitems.Length; i++)
                            whereitems[i] = string.Format("A.{0} = @{0}", whereitems[i]);
                        tmp.Append("whereitems", String.Join(" AND \r\n\t\t", whereitems.ToArray()) + " AND "); 
                        tmp.Set("tablename", tbl.Name);
                        if (parse.Tables.Single(x => x.Name.Like(fk.RefTable)).fks.Count > 0)
                        {
                            tmp.Append("joins", "\r\n\t\tINNER JOIN [dbo].[" + fk.RefTable + "] REF_" + fk.RefTable + " ON A.[" + fk.fkcolumn.Name + "] = REF_" + fk.RefTable + ".[" + fk.RefColumn + "]");
                            GenParent(fk, tmp);
                        }
                    }
                }
#endregion

#region FKs
foreach (FK fk in tbl.fks)
{
    tmp = new Template(Resources<SchemaBuilder>.Read["ForeignKey.sql"]);
    tmp.Set("tablename", tbl.Name);
    tmp.Set("fieldname", fk.fkcolumn.Name);
    tmp.Set("fieldfkname", fk.RefColumn);
    tmp.Set("tablefkname", fk.RefTable);
    FksScript.Append(tmp.ToString());
}
#endregion 

            } 

            TableScript.Append(FksScript.ToString());
            File.WriteAllText(scriptsfolder + "/procs.sql", ProcScript.ToString());
            File.WriteAllText(scriptsfolder + "/tables.sql", TableScript.ToString());


            HttpContext.Current.Response.Write("Complete. SQL Scripts written to /GenScripts now go run those on the DB then browse to the the DAL builder");
        }

        delegate void DelGenParent(FK fk,  Template temp);

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            
        }
    }
}