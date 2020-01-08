using NoRe.Database.Core;
using NoRe.Database.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoRe.Database.SqLite
{
    public class SqLiteWrapper : IDatabase
    {
        private static SqLiteConfiguration Configuration { get; set; }
        public SQLiteConnection Connection { get; set; }
        public SQLiteTransaction Transaction { get; set; }

        /// <summary>
        /// Creates a new SqLiteWrapper
        /// Uses specefied values as connection string
        /// Does not read or write a configuration file
        /// Throws an exception if the database is not reachable
        /// </summary>
        /// <param name="databasePath">The path where the database is located</param>
        /// <param name="databaseVersion">The version of the database</param>
        public SqLiteWrapper(string databasePath, string databaseVersion, string password = "", bool doWrite = false)
        {
            Configuration = new SqLiteConfiguration
            {
                DatabaseVersion = databaseVersion,
                DatabasePath = databasePath,
                Pwd = password
            };
            if (doWrite) Configuration.Write();

            Connection = new SQLiteConnection(Configuration.ToString());

            if (!TestConnection(out string error)) throw new Exception(error);
        }

        /// <summary>
        /// Creates a new MySqlWrapper
        /// Creats and loads the connection string from the configuration file
        /// Throws an exception if the database is not reachable
        /// </summary>
        public SqLiteWrapper(string configurationPath = "")
        {
            Configuration = new SqLiteConfiguration();
            if (!string.IsNullOrEmpty(configurationPath)) Configuration = new SqLiteConfiguration(configurationPath);
            Configuration.Read();

            Connection = new SQLiteConnection(Configuration.ToString());

            if (!TestConnection(out string error)) throw new Exception(error);
        }

        public int ExecuteNonQuery(string commandText, params object[] parameters)
        {
            try
            {
                Connection.Open();
                return GetCommand(commandText, parameters).ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Connection.Close();
            }
        }

        public Table ExecuteReader(string commandText, params object[] parameters)
        {
            try
            {
                Connection.Open();

                SQLiteDataReader reader = GetCommand(commandText, parameters).ExecuteReader();

                Table table = new Table
                {
                    DataTable = reader.GetSchemaTable()
                };

                while (reader.Read())
                {
                    Row row = new Row();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row.Columns.Add(new Column(reader.GetName(i), reader.GetValue(i)));
                    }

                    table.Rows.Add(row);
                }

                return table;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Connection.Close();
            }

        }

        public T ExecuteScalar<T>(string commandText, params object[] parameters)
        {
            try
            {
                Connection.Open();
                return (T)GetCommand(commandText, parameters).ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Connection.Close();
            }
        }

        public void ExecuteTransaction(List<Query> queries)
        {
            try
            {
                StartTransaction();

                foreach (Query query in queries)
                {
                    GetCommand(query.CommandText, query.Parameters).ExecuteNonQuery();
                }

                CommitTransaction();
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                throw ex;
            }
            finally
            {
                Connection.Close();
            }
        }

        public void ExecuteTransaction(string queryText, params object[] parameters)
        {
            ExecuteTransaction(new List<Query>
            {
                new Query(queryText, parameters)
            });
        }

        public bool TestConnection(out string error)
        {
            try
            {
                Connection.Open();

                error = "";
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
            finally
            {
                Connection.Close();
            }
        }


        /// <summary>
        /// Rolls back the current transaction and closes the connection
        /// </summary>
        private void RollbackTransaction()
        {
            try
            {
                Transaction.Rollback();
            }
            finally
            {
                Connection.Close();
                Transaction.Dispose();
                Transaction = null;
            }

        }

        /// <summary>
        /// Committs the current transaction and closes the connection
        /// </summary>
        private void CommitTransaction()
        {
            try
            {
                Transaction.Commit();
            }
            finally
            {
                Connection.Close();
                Transaction.Dispose();
                Transaction = null;
            }
        }
        /// <summary>
        /// Starts the current transaction and opens a connection
        /// </summary>
        private void StartTransaction()
        {
            try
            {
                if (Connection.State != System.Data.ConnectionState.Open)
                {
                    Connection.Open();
                }

                Transaction = Connection.BeginTransaction();
            }
            catch (Exception ex)
            {
                if (Transaction != null) Transaction.Dispose();
                Transaction = null;
                throw ex;
            }

        }

        private SQLiteCommand GetCommand(string commandText, params object[] parameters)
        {
            SQLiteCommand command = Connection.CreateCommand();

            command.CommandText = commandText;
            command.Connection = Connection;
            if (Transaction != null) command.Transaction = Transaction;

            for (int i = 0; i < parameters.Length; i++) { command.Parameters.AddWithValue($"@{i}", parameters[i]); }

            command.Prepare();

            return command;
        }

        public void Dispose()
        {
            if (Transaction != null)
            {
                Transaction.Dispose();
            }

            if (Connection != null)
            {
                Connection.Close();
                Connection.Dispose();
            }
        }
    }
}
