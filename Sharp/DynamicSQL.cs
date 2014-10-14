using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Reflection;
using Slapper;

namespace Sharp
{
    public static class DynamicSQL
    {

        public static SqlParameter[] ToSqlParameters(this object source, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            return source.GetType().GetProperties(bindingAttr)
                    .Select(x => new SqlParameter("@" + x.Name, x.GetValue(source, null) ?? DBNull.Value))
                    .ToArray();
        }

        public static dynamic SqlDataReaderToExpando(SqlDataReader reader)
        {
            var expandoObject = new ExpandoObject() as IDictionary<string, object>;

            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (!expandoObject.Keys.Contains(reader.GetName(i)))
                {
                    expandoObject.Add(reader.GetName(i), reader[i]);
                }
            }

            return expandoObject;
        }

        public static IEnumerable<T> Execute<T>(SqlConnection connection, string commandText, System.Data.CommandType commandType = System.Data.CommandType.Text)
        {
            return Execute<T>(connection, commandText, commandType, null);
        }

        public static IEnumerable<T> Execute<T>(SqlConnection connection, string commandText, System.Data.CommandType commandType = System.Data.CommandType.Text, params SqlParameter[] parms)
        {
            return Execute(connection, commandText, commandType, parms).FirstOrDefault().Select(x => (T)AutoMapper.Map<T>(x));
        }

        public static void ExecuteQuery(SqlConnection connection, string commandText, System.Data.CommandType commandType = System.Data.CommandType.Text, params SqlParameter[] parms)
        {
            int i = 0;
            var rows = Execute(connection, commandText, commandType, parms);
            foreach (var r in rows)
            {
                i++;
            }
        }
        public static void ExecuteQuery(SqlConnection connection, string commandText, System.Data.CommandType commandType = System.Data.CommandType.Text)
        {
            ExecuteQuery(connection, commandText, commandType, null);
        }
        public static IEnumerable<IEnumerable<dynamic>> Execute(SqlConnection connection, string commandText, System.Data.CommandType commandType = System.Data.CommandType.Text)
        {
            return Execute(connection, commandText, commandType, null);
        }
        public static IEnumerable<IEnumerable<dynamic>> Execute(SqlConnection connection, string commandText, System.Data.CommandType commandType, params SqlParameter[] parms)
        { 

            {
                using (var command = new SqlCommand(commandText, connection))
                {
                    command.CommandType = commandType;
                    if (parms != null)
                    {
                        command.Parameters.AddRange(parms);
                        foreach (SqlParameter parm in command.Parameters)
                        {
                            if (parm.Value == null)
                                parm.Value = DBNull.Value;
                        }
                    }

                    if (connection.State != System.Data.ConnectionState.Open)
                        connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<IEnumerable<dynamic>> resultSets = new List<IEnumerable<dynamic>>();
                        do
                        {
                           resultSets.Add(GetRows(reader));
                        } while (reader.NextResult());
                        return resultSets;
                    }
                }
            }
        }

        private static IEnumerable<dynamic> GetRows(SqlDataReader reader)
        {
            List<dynamic> rows = new List<dynamic>();
            while (reader.Read())
            {
                rows.Add(SqlDataReaderToExpando(reader));
            }
            return rows;
        }

    }
}