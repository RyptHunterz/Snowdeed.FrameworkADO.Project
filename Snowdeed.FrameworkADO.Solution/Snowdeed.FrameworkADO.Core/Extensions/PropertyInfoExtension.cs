using Snowdeed.FrameworkADO.Core.Attributes;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Snowdeed.FrameworkADO.Core.Extensions;

public static class PropertyInfoExtension
{
    public static string ConvertToSQLProperty(this PropertyInfo property)
    {
        if (property.PropertyType.IsEnum)
        {
            return IntSQL(property);
        }

        return property.PropertyType switch
        {
            Type a when a == typeof(Boolean) => BooleanSQL(property),
            Type a when a == typeof(DateTime) => DateTimeSQL(property),
            Type a when a == typeof(Guid) => UniqueIdentifierSQL(property),
            Type a when a == typeof(Int32) => IntSQL(property),
            Type a when a == typeof(Int64) => BigIntSQL(property),
            Type a when a == typeof(String) => NVarcharSQL(property),
            Type a when a == typeof(Double) => FloatSQL(property),
            _ => throw new ArgumentOutOfRangeException($"PropertyInfo {property.PropertyType} is not valid"),
        };
    }

    #region -- Property --
    private static string BooleanSQL(PropertyInfo property)
    {
        return $"{property.Name} BIT";
    }

    private static string DateTimeSQL(PropertyInfo property)
    {
        return $"{property.Name} DATETIME2{property.AddNotNull()}";
    }

    private static string UniqueIdentifierSQL(PropertyInfo property)
    {
        return $"{property.Name} UNIQUEIDENTIFIER DEFAULT NEWSEQUENTIALID() {property.AddNotNull()} {property.AddPrimaryKeyValue()} {property.AddForeignKeyAttribute()}";
    }

    private static string IntSQL(PropertyInfo property)
    {
        return $"{property.Name} INT {property.AddNotNull()} {property.AddIdentityValue()} {property.AddPrimaryKeyValue()} {property.AddForeignKeyAttribute()}";
    }

    private static string BigIntSQL(PropertyInfo property)
    {
        return $"{property.Name} BIGINT {property.AddNotNull()} {property.AddIdentityValue()} {property.AddPrimaryKeyValue()} {property.AddForeignKeyAttribute()}";
    }

    private static string NVarcharSQL(PropertyInfo property)
    {
        return $"{property.Name} NVARCHAR({property.AddMaxLenghtAttribute()}) {property.AddNotNull()}";
    }

    private static string FloatSQL(PropertyInfo property)
    {
        return $"{property.Name} FLOAT {property.AddNotNull()}";
    }
    #endregion

    #region -- Attribute --
    private static string AddNotNull(this PropertyInfo property)
    {
        if(Attribute.IsDefined(property, typeof(NotNullAttribute)))
        {
            if(Attribute.IsDefined(property, typeof(NullableAttribute)))
            {
                throw new ArgumentException($"PropertyInfo {property.PropertyType} has a 'NotNull' attribute but its type can be null");
            }

            return " NOT NULL";
        }
     
        return string.Empty;
    }

    private static string AddIdentityValue(this PropertyInfo property)
    {
        if (Attribute.IsDefined(property, typeof(IdentityAttribute)))
        {
            return " IDENTITY(1,1)";
        }

        return string.Empty;
    }

    private static string AddPrimaryKeyValue(this PropertyInfo property)
    {
        if (Attribute.IsDefined(property, typeof(PrimaryKeyAttribute)))
        {
            return " PRIMARY KEY";
        }

        return string.Empty;
    }

    private static string AddMaxLenghtAttribute(this PropertyInfo property)
    {
        if (Attribute.IsDefined(property, typeof(MaxLenghtAttribute)))
        {
            if (Attribute.GetCustomAttribute(property, typeof(MaxLenghtAttribute)) is MaxLenghtAttribute MaxLenghtValue)
            {
                return MaxLenghtValue.MaxLenght.ToString() ?? "MAX";
            }
        }
        return "MAX";
    }

    private static string AddForeignKeyAttribute(this PropertyInfo property)
    {
        if (Attribute.IsDefined(property, typeof(Attributes.ForeignKeyAttribute)))
        {
            if (Attribute.GetCustomAttribute(property, typeof(Attributes.ForeignKeyAttribute)) is Attributes.ForeignKeyAttribute ForeignKeyValue)
            {
                return $" REFERENCES [{ForeignKeyValue.Schema}].[{ForeignKeyValue.Table}]([{ForeignKeyValue.KeyName}])";
            }
        }
        return string.Empty;
    }
    #endregion
}