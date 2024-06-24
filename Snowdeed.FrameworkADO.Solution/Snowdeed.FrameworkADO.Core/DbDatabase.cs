using Microsoft.Data.SqlClient;
using Snowdeed.FrameworkADO.Core.Builders;
using System.Data;
using System.Reflection;

namespace Snowdeed.FrameworkADO.Core;

public interface IDbDatabase
{
    Task<T?> GetByOneStoredProcedureAsync<T>(string storedProcedure, object parameters) where T : new();
    Task<IEnumerable<T>?> GetByListStoredProcedureAsync<T>(string storedProcedure, object parameters) where T : new();
    Task<int> ExecuteStoredProcedureAsync(string storedProcedure, object parameters);
}

public class DbDatabase(SqlConnection connection) : IDbDatabase
{

    #region -- Constructor(s) --
    #endregion

    public async Task<T?> GetByOneStoredProcedureAsync<T>(string storedProcedureName, object parameters) where T : new()
    {
        try
        {
            await using var command = new SqlCommand(storedProcedureName, connection);
            command.CommandType = CommandType.StoredProcedure;

            var spParameters = new CreateParameterBuilder().CreateParameter(parameters);
            command.Parameters.AddRange([.. spParameters.Parameters]);

            await using var reader = await command.ExecuteReaderAsync();

            if (!reader.HasRows) return default;

            await reader.ReadAsync();

            var result = Activator.CreateInstance<T>();

            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties().Where(x => x.CanWrite))
            {
                object? value = reader[propertyInfo.Name] != DBNull.Value ? reader[propertyInfo.Name] : null;
                propertyInfo.SetValue(result, value);
            }

            return result;
        }
        catch (SqlException ex)
        {
            throw new Exception($"Failed to execute '{storedProcedureName}' stored procedure ", ex);
        }
    }

    public async Task<IEnumerable<T>?> GetByListStoredProcedureAsync<T>(string storedProcedureName, object parameters) where T : new()
    {
        try
        {
            await using var command = new SqlCommand(storedProcedureName, connection);
            command.CommandType = CommandType.StoredProcedure;

            var spParameters = new CreateParameterBuilder().CreateParameter(parameters);
            command.Parameters.AddRange([.. spParameters.Parameters]);

            await using var reader = await command.ExecuteReaderAsync();

            if (!reader.HasRows) return default;

            var result = new List<T>();

            while (await reader.ReadAsync())
            {
                var item = Activator.CreateInstance<T>();

                foreach (PropertyInfo propertyInfo in typeof(T).GetProperties().Where(x => x.CanWrite))
                {
                    object? value = reader[propertyInfo.Name] != DBNull.Value ? reader[propertyInfo.Name] : null;
                    propertyInfo.SetValue(item, value);
                }

                result.Add(item);
            }

            return result.AsEnumerable();
        }
        catch (SqlException ex)
        {
            throw new Exception($"Failed to execute '{storedProcedureName}' stored procedure ", ex);
        }
    }

    public async Task<int> ExecuteStoredProcedureAsync(string storedProcedureName, object parameters)
    {
        try
        {
            await using var command = new SqlCommand(storedProcedureName, connection);
            command.CommandType = CommandType.StoredProcedure;

            var spParameters = new CreateParameterBuilder().CreateParameter(parameters);
            command.Parameters.AddRange([.. spParameters.Parameters]);

            return await command.ExecuteNonQueryAsync();
        }
        catch (SqlException ex)
        {
            throw new Exception($"Failed to execute '{storedProcedureName}' stored procedure ", ex);
        }
    }
}