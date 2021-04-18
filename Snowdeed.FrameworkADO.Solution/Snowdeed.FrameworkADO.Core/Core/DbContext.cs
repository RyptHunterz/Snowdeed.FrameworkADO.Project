using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;

namespace Snowdeed.FrameworkADO.Core.Core
{
    using Snowdeed.FrameworkADO.Core.Extensions;
    using Snowdeed.FrameworkADO.Core.Interfaces;

    public class DbContext : IDbContext, IDisposable
    {
        protected SqlConnection _connection;
        protected string _database;

        public DbDatabase Database { get; set; }

        #region -- Constructor(s) --
        public DbContext(string connectionString)
        {
            ArrayList connection = new ArrayList(connectionString.Split(';'));
            StringBuilder _createConnectionString = new StringBuilder();

            for (var i = 0; i < connection.Count - 1; i++)
            {
                if (connection[i].ToString().StartsWith("Database="))
                {
                    _database = connection[i].ToString().Split('=')[1];
                }
                else
                {
                    _createConnectionString.Append($"{connection[i]};");
                }
            }

            _connection = new SqlConnection(_createConnectionString.ToString());
            _connection.Open();
        }
        #endregion

        #region -- Public Method(s) --
        public void CreateDatabase()
        {
            using (SqlCommand command = _connection.CreateCommand())
            {
                string query = $"IF NOT EXISTS(SELECT * FROM sys.databases WHERE name='{_database}') BEGIN CREATE DATABASE [{_database}] END";

                command.CommandText = query;
                command.ExecuteNonQuery();
            }
        }

        public void CreateTable()
        {
            using (SqlCommand command = _connection.CreateCommand())
            {
                foreach (PropertyInfo property in GetType().GetProperties())
                {
                    foreach (Type type in property.PropertyType.GetGenericArguments())
                    {
                        string query = $"USE [{_database}]; IF NOT EXISTS (SELECT * FROM sys.tables WHERE [name] = '{type.Name}') BEGIN CREATE TABLE [{type.Name}] (";

                        foreach (PropertyInfo propertyInfo in type.GetProperties())
                        {
                            query += propertyInfo.ConvertToSQLProperty();
                        }
                        query += $") END";

                        command.CommandText = query;
                        command.ExecuteNonQuery();
                    }
                }
            }
        }
        #endregion

        public void Dispose()
        {
            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }
        }
    }
}