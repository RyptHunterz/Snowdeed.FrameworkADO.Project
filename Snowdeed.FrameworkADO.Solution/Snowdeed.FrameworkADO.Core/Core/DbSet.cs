using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace Snowdeed.FrameworkADO.Core.Core
{
    using Snowdeed.FrameworkADO.Core.Attributes;
    using Snowdeed.FrameworkADO.Core.Interfaces;

    public class DbSet<T> : IDbSet<T> where T : class
    {
        private SqlConnection _connection;
        private string _database;

        #region -- Constructor(s) --
        public DbSet(SqlConnection connection, string database)
        {
            _connection = connection;
            _database = database;
        }
        #endregion

        #region -- Sync Method(s) --
        public IEnumerable<T> GetAll()
        {
            using (SqlCommand command = _connection.CreateCommand())
            {
                string query = $"USE [{_database}]; SELECT * FROM {typeof(T).Name};";
                command.CommandText = query;

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

        public int Add(T entity)
        {
            using (SqlCommand command = _connection.CreateCommand())
            {
                string query = $"USE [{_database}]; INSERT INTO [{typeof(T).Name}] (";

                IEnumerable<PropertyInfo> propertyInfos = typeof(T).GetProperties().Where(x => x.CanWrite && x.GetValue(entity, null) != null).ToList();

                foreach (PropertyInfo propertyInfo in propertyInfos)
                {
                    bool IdentityValue = Attribute.IsDefined(propertyInfo, typeof(IdentityAttribute));

                    if (!IdentityValue)
                    {
                        query += $"{propertyInfo.Name}";

                        if (!propertyInfo.Equals(propertyInfos.Last()))
                        {
                            query += ",";
                        }
                    }
                }
                query += ") VALUES (";

                IEnumerable<PropertyInfo> entityPropertyInfos = entity.GetType().GetProperties().Where(x => x.GetValue(entity, null) != null).ToList();

                foreach (PropertyInfo propertyInfo in entityPropertyInfos)
                {
                    bool IdentityValue = Attribute.IsDefined(propertyInfo, typeof(IdentityAttribute));

                    if (!IdentityValue)
                    {
                        query += $"'{propertyInfo.GetValue(entity)}'";

                        if (!propertyInfo.Equals(entityPropertyInfos.Last()))
                        {
                            query += ",";
                        }
                    }
                }

                query += ");";

                command.CommandText = query;
                return command.ExecuteNonQuery();
            }
        }

        public int Udpate(T entity)
        {
            using (SqlCommand command = _connection.CreateCommand())
            {
                string query = $"USE [{_database}]; UPDATE {typeof(T).Name} SET ";

                IEnumerable<PropertyInfo> propertyInfos = typeof(T).GetProperties().Where(x => x.CanWrite && x.GetValue(entity, null) != null).ToList();

                foreach (PropertyInfo propertyInfo in propertyInfos)
                {
                    if (propertyInfo.Name.ToUpper() != "ID")
                    {
                        query += $"{propertyInfo.Name} = '{propertyInfo.GetValue(entity)}'";

                        if (!propertyInfo.Equals(propertyInfos.Last()))
                        {
                            query += ",";
                        }
                    }
                }
                query += $" WHERE ID = '{propertyInfos.Where(x => x.Name.ToUpper() == "ID").FirstOrDefault().GetValue(entity)}'";

                command.CommandText = query;
                return command.ExecuteNonQuery();
            }
        }

        public int Delete(T entity)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}