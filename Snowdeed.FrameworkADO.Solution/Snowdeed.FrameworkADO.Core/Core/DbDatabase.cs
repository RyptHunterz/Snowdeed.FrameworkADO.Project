using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace Snowdeed.FrameworkADO.Core.Core
{
    using Snowdeed.FrameworkADO.Core.Interfaces;

    public class DbDatabase : IDbDatabase
    {
        private SqlConnection _connection;
        private string _database;

        #region -- Constructor(s) --
        public DbDatabase(SqlConnection connection, string database)
        {
            _connection = connection;
            _database = database;
        }
        #endregion

        public T GetByOneStoredProcedure<T>(string storedProcedure, object parameters) where T : new()
        {
            using (SqlCommand command = _connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = storedProcedure;

                foreach (PropertyInfo propertyInfo in parameters.GetType().GetProperties())
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = $"@{propertyInfo.Name}";
                    parameter.Value = propertyInfo.GetValue(parameters);
                    command.Parameters.Add(parameter);
                }

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    var result = Activator.CreateInstance<T>();

                    if (reader.HasRows)
                    {
                        reader.Read();

                        foreach (PropertyInfo propertyInfo in typeof(T).GetProperties().Where(x => x.CanWrite))
                        {
                            object value = reader[propertyInfo.Name] != DBNull.Value ? reader[propertyInfo.Name] : null;
                            propertyInfo.SetValue(result, value);
                        }
                    }
                    return result;
                }
            }
        }

        public IEnumerable<T> GetByListStoredProcedure<T>(string storedProcedure, object parameters) where T : new()
        {
            using (SqlCommand command = _connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = storedProcedure;

                foreach (PropertyInfo propertyInfo in parameters.GetType().GetProperties())
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = $"@{propertyInfo.Name}";
                    parameter.Value = propertyInfo.GetValue(parameters);
                    command.Parameters.Add(parameter);
                }

                var result = new List<T>();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var item = Activator.CreateInstance<T>();

                            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties().Where(x => x.CanWrite).ToList())
                            {
                                object value = reader[propertyInfo.Name] != DBNull.Value ? reader[propertyInfo.Name] : null;
                                propertyInfo.SetValue(item, value);
                            }

                            result.Add(item);
                        }
                    }
                    return result;
                }
            }
        }

        public bool ExecuteStoredProcedure(string storedProcedure, object parameters)
        {
            using (SqlCommand command = _connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = storedProcedure;

                foreach (PropertyInfo propertyInfo in parameters.GetType().GetProperties())
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = $"@{propertyInfo.Name}";
                    parameter.Value = propertyInfo.GetValue(parameters);
                    command.Parameters.Add(parameter);
                }

                return command.ExecuteNonQuery() != 0;
            }
        }
    }
}