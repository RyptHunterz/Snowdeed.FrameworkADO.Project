using Microsoft.Data.SqlClient;
using Snowdeed.FrameworkADO.Core.Extensions;
using System.Reflection;
using System.Text;

namespace Snowdeed.FrameworkADO.Core;

public interface IDbContext
{
    Task<int> CreateDatabaseAsync();
    Task<int> CreateTableAsync();
}

public class DbContext : IDbContext
{
    protected readonly SqlConnection connection;
    protected readonly string databaseName;

    #region -- Constructor(s) --
    public DbContext(string? connectionString)
    {
        if (connectionString is null) throw new NullReferenceException(nameof(connectionString));

        List<string> connections = new(connectionString.Split(';'));
        StringBuilder _createConnectionString = new();

        for (var i = 0; i < connections.Count - 1; i++)
        {
            if (!string.IsNullOrEmpty(connections[i]) && !connections[i].StartsWith("Database="))
            {
                _createConnectionString.Append($"{connections[i]};");
            }
        }

        databaseName = connectionString.Split(';').Single(x => x.StartsWith("Database=")).Split('=')[1] ?? GetType().Name;

        connection = new SqlConnection(_createConnectionString.ToString());
        connection.Open();
    }
    #endregion

    #region -- Public Method(s) --
    public async Task<int> CreateDatabaseAsync()
    {
        string query = $"IF NOT EXISTS(SELECT * FROM sys.databases WHERE [name] = '{databaseName}')\nBEGIN\nCREATE DATABASE [{databaseName}]\nEND";

        Console.WriteLine(query);

        await using var command = new SqlCommand(query, connection);
        
        return await command.ExecuteNonQueryAsync();
    }

    public async Task<int> DropDatabaseAsync()
    {
        string query = $"USE [master];\nIF EXISTS(SELECT * FROM sys.databases WHERE [name] = '{databaseName}')\nBEGIN\nALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;\nDROP DATABASE [{databaseName}];\nEND";

        Console.WriteLine(query);

        await using var command = new SqlCommand(query, connection);
        return await command.ExecuteNonQueryAsync();
    }

    public async Task<int> CreateTableAsync()
    {
        StringBuilder query = new($"USE [{databaseName}];\n");

        foreach (PropertyInfo property in GetType().GetProperties())
        {
            foreach (Type type in property.PropertyType.GetGenericArguments())
            {
                query.Append($"IF NOT EXISTS (SELECT * FROM sys.tables WHERE [name] = '{type.Name}')\nBEGIN CREATE TABLE [{type.Name}]\n(\n");

                foreach (PropertyInfo propertyInfo in type.GetProperties())
                {
                    query.Append(propertyInfo.ConvertToSQLProperty());

                    if (type.GetProperties().Last() != propertyInfo)
                    {
                        query.Append(",\n");
                    }
                }
                query.Append("\n);\nEND");
            }
        }

        Console.WriteLine(query.ToString());

        await using var command = new SqlCommand(query.ToString(), connection);
        return await command.ExecuteNonQueryAsync();
    }
    #endregion
}