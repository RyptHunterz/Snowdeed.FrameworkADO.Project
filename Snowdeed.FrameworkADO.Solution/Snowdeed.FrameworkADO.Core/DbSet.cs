using Microsoft.Data.SqlClient;
using Snowdeed.FrameworkADO.Core.Attributes;
using Snowdeed.FrameworkADO.Core.Extensions;
using System.Reflection;

namespace Snowdeed.FrameworkADO.Core;

public interface IDbSet<T> where T : class
{
    Task<IEnumerable<T>?> GetAllAsync();
    Task<T?> GetAsync(object id);
    Task<T> AddAsync(T entity);
    Task<int> UdpateAsync(object id, T entity);
    Task<int> UdpateAsync(T entity);
    Task<int> DeleteAsync(object id);
}

public class DbSet<T>(SqlConnection connection, string database) : IDbSet<T> where T : class
{
    private string query = string.Empty;

    #region -- Private Method(s)
    private static PropertyInfo GetPrimaryProperty => typeof(T).GetProperties().Single(x => Attribute.IsDefined(x, typeof(PrimaryKeyAttribute)));
    #endregion

    #region -- Public Method(s) --
    public async Task<IEnumerable<T>?> GetAllAsync()
    {
        try
        {
            string query = $"USE [{database}]; SELECT * FROM {typeof(T).Name};";

            Console.WriteLine(query);

            await using var command = new SqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            if (!reader.HasRows) return default;

            var result = new List<T>();

            while (await reader.ReadAsync())
            {
                var item = Activator.CreateInstance<T>();

                foreach (PropertyInfo propertyInfo in typeof(T).GetProperties().Where(x => x.CanWrite).ToList())
                {
                    object? value = reader[propertyInfo.Name] != DBNull.Value ? reader[propertyInfo.Name] : null;

                    if (propertyInfo.PropertyType.IsEnum)
                    {
                        propertyInfo.SetValue(item, Enum.ToObject(propertyInfo.PropertyType, value ?? new NullReferenceException("Enum is null !")));
                    }
                    else
                    {
                        propertyInfo.SetValue(item, value);
                    }
                }
                result.Add(item);
            }

            return result;
        }
        catch (SqlException ex)
        {
            throw new Exception($"Failed to execute query", ex);
        }
    }

    public async Task<T?> GetAsync(object id)
    {
        try
        {
            query = $"USE [{database}]; SELECT * FROM {typeof(T).Name} WHERE {GetPrimaryProperty.Name} = '{id}';";

            await using var command = new SqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            if (!reader.HasRows) return default;

            await reader.ReadAsync();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(query);

            var result = Activator.CreateInstance<T>();

            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties().Where(x => x.CanWrite).ToList())
            {
                object? value = reader[propertyInfo.Name] != DBNull.Value ? reader[propertyInfo.Name] : null;
                if (propertyInfo.PropertyType.IsEnum)
                {
                    propertyInfo.SetValue(result, Enum.ToObject(propertyInfo.PropertyType, value ?? new NullReferenceException("Enum is null !")));
                }
                else
                {
                    propertyInfo.SetValue(result, value);
                }
            }

            return result;
        }
        catch (SqlException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(query);

            throw new Exception($"Failed to execute query", ex);
        }
    }

    public async Task<T> AddAsync(T entity)
    {
        try
        {
            string query = $"SET DATEFORMAT dmy; USE [{database}]; INSERT INTO [{typeof(T).Name}] (";

            IEnumerable<PropertyInfo> propertyInfos = typeof(T).GetProperties().Where(x => x.CanWrite && x.GetValue(entity, null) != null).ToList();

            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                if (!Attribute.IsDefined(propertyInfo, typeof(IdentityAttribute)) && 
                    !Attribute.IsDefined(propertyInfo, typeof(PrimaryKeyAttribute)))
                {
                    query += $"{propertyInfo.Name}";

                    if (!propertyInfo.Equals(propertyInfos.Last()))
                    {
                        query += ",";
                    }
                }
            }

            query += $") OUTPUT INSERTED.{GetPrimaryProperty.Name} VALUES (";

            IEnumerable<PropertyInfo> entityPropertyInfos = entity.GetType().GetProperties().Where(x => x.GetValue(entity, null) != null).ToList();

            foreach (PropertyInfo propertyInfo in entityPropertyInfos)
            {
                if (!Attribute.IsDefined(propertyInfo, typeof(IdentityAttribute)) && 
                    !Attribute.IsDefined(propertyInfo, typeof(PrimaryKeyAttribute)))
                {
                    if (propertyInfo.PropertyType.IsEnum)
                    {
                        query += propertyInfo.GetEnumValue(entity);
                    }
                    else if (propertyInfo.PropertyType.IsDouble())
                    {
                        query += propertyInfo.GetDoubleValue(entity);
                    }
                    else
                    {
                        query += $"'{propertyInfo.GetValue(entity)}'";
                    }

                    if (!propertyInfo.Equals(entityPropertyInfos.Last()))
                    {
                        query += ",";
                    }
                }
            }

            query += ");";

            await using var command = new SqlCommand(query, connection);
            GetPrimaryProperty.SetValue(entity, await command.ExecuteScalarAsync());

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(query);

            return entity;
        }
        catch (SqlException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(query);

            throw new Exception($"Failed to execute query", ex);
        }
    }

    public async Task<int> UdpateAsync(object objectValue, T entity)
    {
        try
        {
            string query = $"USE [{database}]; UPDATE {typeof(T).Name} SET ";

            IEnumerable<PropertyInfo> propertyInfos = typeof(T).GetProperties().Where(x => x.CanWrite && x.GetValue(entity, null) != null).ToList();

            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                if (!Attribute.IsDefined(propertyInfo, typeof(IdentityAttribute)) &&
                   !Attribute.IsDefined(propertyInfo, typeof(PrimaryKeyAttribute)))
                {
                    if (propertyInfo.PropertyType.IsEnum)
                    {
                        query += $"{propertyInfo.Name} = '{propertyInfo.GetEnumValue(entity)}'";
                    }
                    else
                    {
                        query += $"{propertyInfo.Name} = '{propertyInfo.GetValue(entity)}'";
                    }

                    if (!propertyInfo.Equals(propertyInfos.Last()))
                    {
                        query += ",";
                    }
                }
            }

            query += $" WHERE {GetPrimaryProperty.Name} = '{objectValue}'";

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(query);

            await using var command = new SqlCommand(query, connection);
            return await command.ExecuteNonQueryAsync();
        }
        catch (SqlException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(query);

            throw new Exception($"Failed to execute query", ex);
        }
    }

    public async Task<int> UdpateAsync(T entity)
    {
        try
        {
            string query = $"USE [{database}]; UPDATE {typeof(T).Name} SET ";

            IEnumerable<PropertyInfo> propertyInfos = typeof(T).GetProperties().Where(x => x.CanWrite && x.GetValue(entity, null) != null).ToList();

            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                if (!Attribute.IsDefined(propertyInfo, typeof(IdentityAttribute)) &&
                    !Attribute.IsDefined(propertyInfo, typeof(PrimaryKeyAttribute)))
                {
                    if (propertyInfo.PropertyType.IsEnum)
                    {
                        query += $"{propertyInfo.Name} = '{propertyInfo.GetEnumValue(entity)}'";
                    }
                    else
                    {
                        query += $"{propertyInfo.Name} = '{propertyInfo.GetValue(entity)}'";
                    }

                    if (!propertyInfo.Equals(propertyInfos.Last()))
                    {
                        query += ",";
                    }
                }
            }

            query += $" WHERE {GetPrimaryProperty.Name} = '{GetPrimaryProperty.GetValue(entity)}'";

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(query);

            await using var command = new SqlCommand(query, connection);
            return await command.ExecuteNonQueryAsync();
        }
        catch (SqlException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(query);

            throw new Exception($"Failed to execute query", ex);
        }
    }

    public async Task<int> DeleteAsync(object id)
    {
        try
        {
            string query = $"USE [{database}]; DELETE FROM {typeof(T).Name} WHERE {GetPrimaryProperty.Name} = '{id}'";

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(query);

            await using var command = new SqlCommand(query, connection);
            return await command.ExecuteNonQueryAsync();

        }
        catch (SqlException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(query);

            throw new Exception($"Failed to execute query", ex);
        }
    }
    #endregion
}