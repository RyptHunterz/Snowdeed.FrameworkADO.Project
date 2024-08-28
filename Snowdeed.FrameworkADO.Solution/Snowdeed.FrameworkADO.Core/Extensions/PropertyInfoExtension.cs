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

        if(string.IsNullOrEmpty(propertyAttributes))
        {
            return $"{propertyName} {propertySql}";
        }

        return $"{propertyName} {propertySql} {propertyAttributes}";
    }

    private static string GetPropertyName(PropertyInfo property) => $"{property.Name}";

    private static string GetPropertySql(PropertyInfo property)
    {
        return property.PropertyType switch
        {
            Type t when t.IsEnum => $"INT",
            Type t when t == typeof(Boolean) => $"BIT",
            Type t when t == typeof(DateTime) => $"DATETIME2",
            Type t when t == typeof(Guid) => $"UNIQUEIDENTIFIER DEFAULT NEWSEQUENTIALID()",
            Type t when t == typeof(Int32) => $"INT",
            Type t when t == typeof(Int64) => $"BIGINT",
            Type t when t == typeof(String) => $"NVARCHAR",
            Type t when t == typeof(Double) => $"FLOAT",
            _ => throw new ArgumentOutOfRangeException($"PropertyInfo {property.PropertyType} is not valid"),
        };
    }

    private static List<string> GetPropertyAttribute(PropertyInfo property)
    {
        List<string> attributes = [];

        foreach (Attribute attribute in property.GetCustomAttributes())
        {
            if (attribute.GetType() == typeof(NotNullAttribute))
            {
                attributes.Add("NOT NULL");
            }
            if (attribute.GetType() == typeof(IdentityAttribute))
            {
                attributes.Add("IDENTITY(1,1)");
            }
            if (attribute.GetType() == typeof(PrimaryKeyAttribute))
            {
                attributes.Add("PRIMARY KEY");
            }
            if (attribute.GetType() == typeof(MaxLenghtAttribute))
            {
                if (Attribute.GetCustomAttribute(property, typeof(MaxLenghtAttribute)) is MaxLenghtAttribute MaxLenghtValue)
                {
                    attributes.Add($"({MaxLenghtValue.MaxLenght})" ?? "(MAX)");
                }
                else
                {
                    attributes.Add("(MAX)");
                }
            }
            if (attribute.GetType() == typeof(ForeignKeyAttribute))
            {
                if (Attribute.GetCustomAttribute(property, typeof(ForeignKeyAttribute)) is ForeignKeyAttribute ForeignKeyValue)
                {
                    attributes.Add($"REFERENCES [{ForeignKeyValue.Schema}].[{ForeignKeyValue.Table}]([{ForeignKeyValue.KeyName}])");
                }
            }
        }
        return attributes;
    }

    #region -- Property --
    //private static string BooleanSQL(PropertyInfo property)
    //{
    //    return $"{property.Name} BIT";
    //}

    //private static string DateTimeSQL(PropertyInfo property)
    //{
    //    return $"{property.Name} DATETIME2{property.AddNotNull()}";
    //}

    //private static string UniqueIdentifierSQL(PropertyInfo property)
    //{
    //    return $"{property.Name} UNIQUEIDENTIFIER DEFAULT NEWSEQUENTIALID() {property.AddNotNull()} {property.AddPrimaryKeyValue()} {property.AddForeignKeyAttribute()}";
    //}

    //private static string IntSQL(PropertyInfo property)
    //{
    //    return $"{property.Name} INT {property.AddNotNull()} {property.AddIdentityValue()} {property.AddPrimaryKeyValue()} {property.AddForeignKeyAttribute()}";
    //}

    //private static string BigIntSQL(PropertyInfo property)
    //{
    //    return $"{property.Name} BIGINT {property.AddNotNull()} {property.AddIdentityValue()} {property.AddPrimaryKeyValue()} {property.AddForeignKeyAttribute()}";
    //}

    //private static string NVarcharSQL(PropertyInfo property)
    //{
    //    return $"{property.Name} NVARCHAR({property.AddMaxLenghtAttribute()}) {property.AddNotNull()}";
    //}

    //private static string FloatSQL(PropertyInfo property)
    //{
    //    return $"{property.Name} FLOAT {property.AddNotNull()}";
    //}
    #endregion

    #region -- Attribute --
    //private static string AddNotNull(this PropertyInfo property)
    //{
    //    if (Attribute.IsDefined(property, typeof(NotNullAttribute)))
    //    {
    //        if (Attribute.IsDefined(property, typeof(NullableAttribute)))
    //        {
    //            throw new ArgumentException($"PropertyInfo {property.PropertyType} has a 'NotNull' attribute but its type can be null");
    //        }

    //        return " NOT NULL";
    //    }

    //    return string.Empty;
    //}

    //private static string AddIdentityValue(this PropertyInfo property)
    //{
    //    if (Attribute.IsDefined(property, typeof(IdentityAttribute)))
    //    {
    //        return " IDENTITY(1,1)";
    //    }

    //    return string.Empty;
    //}

    //private static string AddPrimaryKeyValue(this PropertyInfo property)
    //{
    //    if (Attribute.IsDefined(property, typeof(PrimaryKeyAttribute)))
    //    {
    //        return " PRIMARY KEY";
    //    }

    //    return string.Empty;
    //}

    //private static string AddMaxLenghtAttribute(this PropertyInfo property)
    //{
    //    if (Attribute.IsDefined(property, typeof(MaxLenghtAttribute)))
    //    {
    //        if (Attribute.GetCustomAttribute(property, typeof(MaxLenghtAttribute)) is MaxLenghtAttribute MaxLenghtValue)
    //        {
    //            return MaxLenghtValue.MaxLenght.ToString() ?? "MAX";
    //        }
    //    }
    //    return "MAX";
    //}

    //private static string AddForeignKeyAttribute(this PropertyInfo property)
    //{
    //    if (Attribute.IsDefined(property, typeof(Attributes.ForeignKeyAttribute)))
    //    {
    //        if (Attribute.GetCustomAttribute(property, typeof(Attributes.ForeignKeyAttribute)) is Attributes.ForeignKeyAttribute ForeignKeyValue)
    //        {
    //            return $" REFERENCES [{ForeignKeyValue.Schema}].[{ForeignKeyValue.Table}]([{ForeignKeyValue.KeyName}])";
    //        }
    //    }
    //    return string.Empty;
    //}
    #endregion
}