using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicBridge.Data
{
    public class DataAccess
    {
        private string _connectionString;
        private DbProviderFactory _providerFactory;
        private BasicLogger _basicLogger = new BasicLogger();

        public DataAccess(string connectionString, DbProviderFactory providerFactory)
        {
            _connectionString = connectionString;
            _providerFactory = providerFactory;
        }

        public List<T> ReadPocos<T>(string selectQuery) where T : new()
        {
            var returnPocos = new List<T>();

            try
            {
                using (var connection = _providerFactory.CreateConnection())
                {
                    connection.ConnectionString = _connectionString;
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = selectQuery;

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader != null && reader.FieldCount > 0)
                            {
                                // change this to an array. No reason why not to.
                                var columnNames = new List<string>();

                                for (var col = 0; col < reader.FieldCount; col++)
                                {
                                    columnNames.Add(reader.GetName(col));
                                }

                                while (reader.Read())
                                {
                                    var poco = new T();
                                    
                                    foreach (var columnName in columnNames)
                                    {
                                        object baseValue = reader.GetValue(reader.GetOrdinal(columnName));

                                        if (baseValue == DBNull.Value)
                                        {
                                            // need to implement null pattern 
                                        }
                                        else
                                        {
                                            // populate poco property with value from cell
                                            poco.GetType().GetProperty(columnName).SetValue(poco, baseValue);
                                        }
                                    }

                                    returnPocos.Add(poco);
                                }
                            }

                            else
                            {
                                return returnPocos;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // log the exception                   
                _basicLogger.LogError("Odbc Command Select Query Failed Due To Error: " + e.Message +
                                     Environment.NewLine + "Select Query was:" + Environment.NewLine + selectQuery);
            }

            return returnPocos;
        }

        public DataTable Read(string selectQuery)
        {
            var table = new DataTable();
            DataColumn column;
            DataRow row;

            try
            {
                using (var connection = new OdbcConnection(_connectionString))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = selectQuery;

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader != null && reader.HasRows)
                            {
                                // perform simple inline data conversion
                                for (var c = 0; c < reader.FieldCount; c++)
                                {
                                    column = new DataColumn();
                                    column.DataType = reader.GetFieldType(c);
                                    column.ColumnName = reader.GetName(c);
                                    table.Columns.Add(column);
                                }
                                
                                while (reader.Read())
                                {
                                    row = table.NewRow();

                                    foreach (DataColumn c in table.Columns)
                                    {
                                        object baseValue = reader.GetValue(reader.GetOrdinal(c.ColumnName));

                                        if(baseValue == DBNull.Value)
                                        {
                                            row[c.ColumnName] = baseValue;
                                        }
                                        else
                                        {
                                            row[c.ColumnName] = Convert.ChangeType(baseValue, c.DataType); 
                                        }                                                                                      
                                    }

                                    table.Rows.Add(row);
                                }
                            }

                            else
                            {
                                return table;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // log the exception                   
                _basicLogger.LogError("Odbc Command Select Query Failed Due To Error: " + e.Message);                        
            }            

            return table;
        }

        public void Write(string sql)
        {            
            try
            {
                using (var connection = new OdbcConnection(_connectionString))
                {
                    connection.Open();

                    using (var command = new OdbcCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                // log the exception
                _basicLogger.LogError("Odbc Command Execute Query Failed Due To Error: " + e.Message +
                                     Environment.NewLine + "Execute Query was:" + Environment.NewLine + sql);
            }            
        }
    }
}
