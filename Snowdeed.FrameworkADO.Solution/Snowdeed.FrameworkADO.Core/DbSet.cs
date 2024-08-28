using Microsoft.Data.SqlClient;
using Snowdeed.FrameworkADO.Core.Attributes;
using Snowdeed.FrameworkADO.Core.Extensions;
using System.Linq.Expressions;
using System.Reflection;

namespace Snowdeed.FrameworkADO.Core;

public interface IDbSet<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken);
    Task<T?> GetAsync(object id, CancellationToken cancellationToken);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken);
    Task<int> UdpateAsync(object id, T entity, CancellationToken cancellationToken);
    Task<int> UdpateAsync(T entity, CancellationToken cancellationToken);
    Task<int> DeleteAsync(object id, CancellationToken cancellationToken);
}

public class DbSet<T>(SqlConnection connection, string database) : IDbSet<T> where T : class
{
    private string? query = $"SET DATEFORMAT dmy; USE [{database}];";

    #region -- Private Method(s)
    private static void Log(string? log, bool error)
    {
        Console.ForegroundColor = error ? ConsoleColor.Red : ConsoleColor.White;
        Console.WriteLine(log);
    }
    private static PropertyInfo GetPrimaryProperty => typeof(T).GetProperties().Single(x => Attribute.IsDefined(x, typeof(PrimaryKeyAttribute)));
    private async Task<IEnumerable<T>> GetAll(string? whereQuery, CancellationToken cancellationToken)
    {
        try
        {
            query += $"SELECT * FROM {typeof(T).Name} {whereQuery};";

            Log(query, false);

            await using var command = new SqlCommand(query, connection);
            cancellationToken.Register(command.Cancel);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var result = new List<T>();

            if (reader.HasRows)
            {
                while (await reader.ReadAsync(cancellationToken))
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
            }
            return result;
        }
        catch (SqlException ex)
        {
            throw new Exception($"Failed to execute query : {query}", ex);
        }
    }
    #endregion

    #region -- Public Method(s) --
    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await GetAll(null, cancellationToken);
    }

    public async Task<T?> GetAsync(object id, CancellationToken cancellationToken)
    {
        try
        {
            query = $"SELECT * FROM {typeof(T).Name} WHERE {GetPrimaryProperty.Name} = '{id}';";

            await using var command = new SqlCommand(query, connection);
            cancellationToken.Register(command.Cancel);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!reader.HasRows) return default;

            await reader.ReadAsync(cancellationToken);

            Log(query, false);

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
            Log(query, true);
            throw new Exception($"Failed to execute query : {query}", ex);
        }
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken)
    {
        var whereQuery = $"WHERE {expression.ConvertToSql()}";
        return await GetAll(whereQuery, cancellationToken);
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken)
    {
        try
        {
            query += $"INSERT INTO [{typeof(T).Name}] (";

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
            cancellationToken.Register(command.Cancel);
            GetPrimaryProperty.SetValue(entity, await command.ExecuteScalarAsync(cancellationToken));

            Log(query, false);

            return entity;
        }
        catch (SqlException ex)
        {
            Log(query, true);

            throw new Exception($"Failed to execute query : {query}", ex);
        }
    }

    public async Task<int> UdpateAsync(object objectValue, T entity, CancellationToken cancellationToken)
    {
        try
        {
            query += $"UPDATE {typeof(T).Name} SET ";

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

            Log(query, false);

            await using var command = new SqlCommand(query, connection);
            cancellationToken.Register(command.Cancel);
            return await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (SqlException ex)
        {
            Log(query, true);

            throw new Exception($"Failed to execute query : {query}", ex);
        }
    }

    public async Task<int> UdpateAsync(T entity, CancellationToken cancellationToken)
    {
        try
        {
            query += $"UPDATE {typeof(T).Name} SET ";

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

            Log(query, false);

            await using var command = new SqlCommand(query, connection);
            cancellationToken.Register(command.Cancel);
            return await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (SqlException ex)
        {
            Log(query, true);

            throw new Exception($"Failed to execute query : {query}", ex);
        }
    }

    public async Task<int> DeleteAsync(object id, CancellationToken cancellationToken)
    {
        try
        {
            query += $"DELETE FROM {typeof(T).Name} WHERE {GetPrimaryProperty.Name} = '{id}'";

            Log(query, false);

            await using var command = new SqlCommand(query, connection);
            cancellationToken.Register(command.Cancel);
            return await command.ExecuteNonQueryAsync(cancellationToken);

        }
        catch (SqlException ex)
        {
            Log(query, true);
            throw new Exception($"Failed to execute query : {query}", ex);
        }
    }
    #endregion


}