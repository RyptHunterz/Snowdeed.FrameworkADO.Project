using Microsoft.Data.SqlClient;
using Snowdeed.FrameworkADO.Core.Attributes;
using Snowdeed.FrameworkADO.Core.Extensions;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

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
    #region -- Private Method(s)
    private static void Log(string? log, bool error)
    {
        Console.ForegroundColor = error ? ConsoleColor.Red : ConsoleColor.White;
        Console.WriteLine(log);
    }
    private static PropertyInfo GetPrimaryProperty => typeof(T).GetProperties().Single(x => Attribute.IsDefined(x, typeof(PrimaryKeyAttribute)));
    #endregion

    #region -- Public Method(s) --
    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken)
    {
        if (connection == null || connection.State != ConnectionState.Open)
            throw new InvalidOperationException("Database connection is not initialized or not open.");

        string query = $"SET DATEFORMAT dmy; USE [{database}]; SELECT * FROM {typeof(T).Name};";
        Log(query, false);

        try
        {
            await using var command = new SqlCommand(query, connection);
            cancellationToken.Register(() => command.Cancel());

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await MapReaderToEntities(reader, cancellationToken);
        }
        catch (SqlException ex)
        {
            throw new Exception($"Failed to execute query : {query}", ex);
        }
    }

    public async Task<T?> GetAsync(object id, CancellationToken cancellationToken)
    {
        if (connection == null || connection.State != ConnectionState.Open)
            throw new InvalidOperationException("Database connection is not initialized or not open.");

        string query = $"SET DATEFORMAT dmy; USE [{database}]; SELECT * FROM {typeof(T).Name} WHERE {GetPrimaryProperty.Name} = @id;";
        Log(query, false);

        try
        {
            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            cancellationToken.Register(() => command.Cancel());

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!reader.HasRows) return default;

            await reader.ReadAsync(cancellationToken);
            return MapReaderToEntity(reader);
        }
        catch (SqlException ex)
        {
            Log(query, true);
            throw new Exception($"Failed to execute query : {query}", ex);
        }
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(expression);

        if (connection == null || connection.State != ConnectionState.Open)
            throw new InvalidOperationException("Database connection is not initialized or not open.");

        string whereClause = expression.ConvertToSql();
        string query = $"SET DATEFORMAT dmy; USE [{database}]; SELECT * FROM {typeof(T).Name} WHERE {whereClause};";
        Log(query, false);

        try
        {
            await using var command = new SqlCommand(query, connection);
            cancellationToken.Register(() => command.Cancel());

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await MapReaderToEntities(reader, cancellationToken);
        }
        catch (SqlException ex)
        {
            Log($"SQL Exception occurred: {ex.Message}", true);
            throw new Exception($"Failed to execute query: {query}. See inner exception for details.", ex);
        }
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken)
    {
        if (connection == null || connection.State != ConnectionState.Open)
            throw new InvalidOperationException("Database connection is not initialized or not open.");

        var queryBuilder = new StringBuilder($"SET DATEFORMAT dmy; USE [{database}]; INSERT INTO [{typeof(T).Name}] (");
        var parameters = new List<SqlParameter>();

        try
        {
            IEnumerable<PropertyInfo> propertyInfos = typeof(T).GetProperties().Where(x => x.CanWrite && x.GetValue(entity, null) != null).ToList();

            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                if (!Attribute.IsDefined(propertyInfo, typeof(IdentityAttribute)) &&
                    !Attribute.IsDefined(propertyInfo, typeof(PrimaryKeyAttribute)))
                {
                    queryBuilder.Append($"{propertyInfo.Name},");
                    var paramName = "@" + propertyInfo.Name;
                    parameters.Add(new SqlParameter(paramName, propertyInfo.GetValue(entity) ?? DBNull.Value));
                }
            }

            queryBuilder.Length--;
            queryBuilder.Append($") OUTPUT INSERTED.{GetPrimaryProperty.Name} VALUES (");

            foreach (var propertyInfo in propertyInfos)
            {
                if (!Attribute.IsDefined(propertyInfo, typeof(IdentityAttribute)) &&
                    !Attribute.IsDefined(propertyInfo, typeof(PrimaryKeyAttribute)))
                {
                    queryBuilder.Append($"@{propertyInfo.Name},");
                }
            }

            queryBuilder.Length--;
            queryBuilder.Append(");");

            var command = new SqlCommand(queryBuilder.ToString(), connection);
            command.Parameters.AddRange(parameters.ToArray());

            cancellationToken.Register(command.Cancel);
            GetPrimaryProperty.SetValue(entity, await command.ExecuteScalarAsync(cancellationToken));

            Log(queryBuilder.ToString(), false);
            return entity;
        }
        catch (SqlException ex)
        {
            Log(queryBuilder.ToString(), true);
            throw new Exception($"Failed to execute query : {queryBuilder.ToString()}", ex);
        }
    }

    public async Task<int> UdpateAsync(object primaryPropertyValue, T entity, CancellationToken cancellationToken)
    {
        if (connection == null || connection.State != ConnectionState.Open)
            throw new InvalidOperationException("Database connection is not initialized or not open.");

        var queryBuilder = new StringBuilder($"SET DATEFORMAT dmy; USE [{database}]; UPDATE {typeof(T).Name} SET ");
        var parameters = new List<SqlParameter>();

        try
        {
            List<PropertyInfo> propertyInfos = typeof(T).GetProperties().Where(x => x.CanWrite && x.GetValue(entity, null) != null).ToList();

            for (int i = 0; i < propertyInfos.Count; i++)
            {
                var propertyInfo = propertyInfos[i];

                if (!Attribute.IsDefined(propertyInfo, typeof(IdentityAttribute)) &&
                    !Attribute.IsDefined(propertyInfo, typeof(PrimaryKeyAttribute)))
                {
                    var parameterName = $"@param{i}";
                    var value = propertyInfo.GetValue(entity);

                    if (propertyInfo.PropertyType.IsEnum && value != null)
                    {
                        value = (int)value;
                    }

                    queryBuilder.Append($"{propertyInfo.Name} = {parameterName}");
                    if (i < propertyInfos.Count - 1)
                    {
                        queryBuilder.Append(", ");
                    }

                    parameters.Add(new SqlParameter(parameterName, value ?? DBNull.Value));
                }
            }

            queryBuilder.Append($" WHERE {GetPrimaryProperty.Name} = @primaryPropertyValue");
            parameters.Add(new SqlParameter("@primaryPropertyValue", primaryPropertyValue));

            Log(queryBuilder.ToString(), false);

            await using var command = new SqlCommand(queryBuilder.ToString(), connection);
            command.Parameters.AddRange(parameters.ToArray());
            cancellationToken.Register(command.Cancel);

            return await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (SqlException ex)
        {
            Log(queryBuilder.ToString(), true);
            throw new Exception($"Failed to execute query: {queryBuilder}", ex);
        }
    }

    public async Task<int> UdpateAsync(T entity, CancellationToken cancellationToken)
    {
        string query = $"SET DATEFORMAT dmy; USE [{database}];";
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
        string query = $"SET DATEFORMAT dmy; USE [{database}];";
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

    private async Task<List<T>> MapReaderToEntities(SqlDataReader reader, CancellationToken cancellationToken)
    {
        var result = new List<T>();

        if (!reader.HasRows) return result;

        var properties = typeof(T).GetProperties().Where(p => p.CanWrite).ToList();

        while (await reader.ReadAsync(cancellationToken))
        {
            var item = Activator.CreateInstance<T>();
            MapProperties(reader, properties, item);
            result.Add(item);
        }

        return result;
    }

    private T? MapReaderToEntity(SqlDataReader reader)
    {
        var properties = typeof(T).GetProperties().Where(p => p.CanWrite).ToList();
        var item = Activator.CreateInstance<T>();
        MapProperties(reader, properties, item);
        return item;
    }

    private void MapProperties(SqlDataReader reader, List<PropertyInfo> properties, T item)
    {
        foreach (var property in properties)
        {
            var columnValue = reader[property.Name] != DBNull.Value ? reader[property.Name] : null;

            if (property.PropertyType.IsEnum)
            {
                if (columnValue == null)
                    throw new NullReferenceException($"Enum property '{property.Name}' cannot be null.");

                property.SetValue(item, Enum.ToObject(property.PropertyType, columnValue));
            }
            else if (property.PropertyType == typeof(DateOnly?) || property.PropertyType == typeof(DateOnly))
            {
                property.SetValue(item, columnValue is DateTime dt ? DateOnly.FromDateTime(dt) : null);
            }
            else if (property.PropertyType == typeof(TimeOnly?) || property.PropertyType == typeof(TimeOnly))
            {
                if (columnValue is DateTime dt)
                    property.SetValue(item, TimeOnly.FromDateTime(dt));
                else if (columnValue is TimeSpan ts)
                    property.SetValue(item, TimeOnly.FromTimeSpan(ts));
                else
                    property.SetValue(item, null);
            }
            else
            {
                property.SetValue(item, columnValue);
            }
        }
    }
}