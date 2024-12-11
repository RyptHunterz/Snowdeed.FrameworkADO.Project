using Snowdeed.FrameworkADO.Core.Attributes;
using System.Reflection;

namespace Snowdeed.FrameworkADO.Core.Extensions;

public static class PropertyInfoExtension
{
    public static string ConvertToSQL(this PropertyInfo property)
    {
        string propertyName = GetPropertyName(property);
        string propertySql = GetPropertySql(property);
        string propertyAttributes = string.Join(" ", GetPropertyAttribute(property));

        return string.IsNullOrEmpty(propertyAttributes)
            ? $"{propertyName} {propertySql}"
            : $"{propertyName} {propertySql} {propertyAttributes}";
    }

    private static string GetPropertyName(PropertyInfo property) => property.Name;

    private static string GetPropertySql(PropertyInfo property)
    {
        var underlyingType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

        string sqlType = underlyingType switch
        {
            Type t when t.IsEnum => "INT",
            Type t when t == typeof(bool) => "BIT",
            Type t when t == typeof(DateTime) => "DATETIME2",
            Type t when t == typeof(DateOnly) => "DATE",
            Type t when t == typeof(TimeOnly) => "TIME",
            Type t when t == typeof(Guid) => "UNIQUEIDENTIFIER DEFAULT NEWSEQUENTIALID()",
            Type t when t == typeof(int) => "INT",
            Type t when t == typeof(long) => "BIGINT",
            Type t when t == typeof(string) => "NVARCHAR",
            Type t when t == typeof(double) => "FLOAT",
            Type t when t == typeof(decimal) => "DECIMAL",
            _ => throw new ArgumentOutOfRangeException($"Property type {property.PropertyType} is not supported"),
        };

        var maxLengthAttr = property.GetCustomAttribute<MaxLenghtAttribute>();
        if (maxLengthAttr != null && property.PropertyType == typeof(string))
        {
            sqlType += maxLengthAttr.MaxLenght > 0 ? $"({maxLengthAttr.MaxLenght})" : "(MAX)";
        }

        bool isNullable = Nullable.GetUnderlyingType(property.PropertyType) != null ||
                      !property.PropertyType.IsValueType;
        sqlType += isNullable ? " NULL" : " NOT NULL";

        return sqlType;
    }

    private static List<string> GetPropertyAttribute(PropertyInfo property)
    {
        List<string> attributes = [];

        foreach (Attribute attribute in property.GetCustomAttributes())
        {
            switch (attribute)
            {
                case NotNullAttribute:
                    attributes.Add("NOT NULL");
                    break;

                case IdentityAttribute:
                    attributes.Add("IDENTITY(1,1)");
                    break;

                case PrimaryKeyAttribute:
                    attributes.Add("PRIMARY KEY");
                    break;

                case ForeignKeyAttribute foreignKey:
                    attributes.Add($"REFERENCES [{foreignKey.Schema}].[{foreignKey.Table}]([{foreignKey.KeyName}])");
                    break;
            }
        }

        return attributes;
    }
}