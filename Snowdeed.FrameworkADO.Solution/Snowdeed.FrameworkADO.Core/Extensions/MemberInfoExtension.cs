using System;
using System.Reflection;

namespace Snowdeed.FrameworkADO.Core.Extensions
{
    using Snowdeed.FrameworkADO.Core.Attributes;

    public static class MemberInfoExtension
    {
        private const string Boolean = "System.Boolean";
        private const string DateTime = "System.DateTime";
        private const string Guid = "System.Guid";
        private const string Int32 = "System.Int32";
        private const string Int64 = "System.Int64";
        private const string String = "System.String";

        public static string ConvertToSQLProperty(this PropertyInfo property)
        {
            bool NotNullValue = Attribute.IsDefined(property, typeof(NotNullAttribute));
            bool IdentityValue = Attribute.IsDefined(property, typeof(IdentityAttribute));
            MaxLenghtAttribute MaxLenghtValue = (MaxLenghtAttribute) Attribute.GetCustomAttribute(property, typeof(MaxLenghtAttribute));

            switch (property.PropertyType.ToString())
            {
                case Boolean:
                    return $"{property.Name} BIT,";
                case DateTime:
                    return $"{property.Name} DATETIME2 {(NotNullValue ? "NOT NULL" : string.Empty )},";
                case Guid:
                    return $"{property.Name} UNIQUEIDENTIFIER {(NotNullValue ? "NOT NULL" : string.Empty)},";
                case Int32:
                    return $"{property.Name} INT {(NotNullValue ? "NOT NULL" : string.Empty)} {(IdentityValue ? "IDENTITY(1,1)" : string.Empty)},";
                case Int64:
                    return $"{property.Name} BIGINT {(NotNullValue ? "NOT NULL" : string.Empty)}";
                case String:
                    return $"{property.Name} NVARCHAR({((MaxLenghtValue != null) ? MaxLenghtValue.MaxLenght : "max")}) {(NotNullValue ? "NOT NULL" : string.Empty)},";
                default:
                    throw new InvalidOperationException("PropertyInfo value is not valid");
            }
        }
    }
}