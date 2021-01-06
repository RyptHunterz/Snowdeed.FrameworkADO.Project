using System;
using System.Reflection;

namespace Snowdeed.FrameworkADO.Core.Extensions
{
    public static class MemberInfoExtension
    {
        private const string Boolean = "System.Boolean";
        private const string DateTime = "System.DateTime";
        private const string DateTimeNull = "System.Nullable`1[System.DateTime]";
        private const string Guid = "System.Guid";
        private const string GuidNull = "System.Nullable`1[System.Guid]";
        private const string Int32 = "System.Int32";
        private const string Int64 = "System.Int64";
        private const string String = "System.String";

        public static string ConvertToSQLProperty(this PropertyInfo property)
        {
            var str = property.PropertyType.ToString();

            switch (str)
            {
                case Boolean:
                    return $"{property.Name} BIT NOT NULL DEFAULT(0),";
                case DateTime:
                    return $"{property.Name} DATETIME NOT NULL,";
                case DateTimeNull:
                    return $"{property.Name} DATETIME NULL,";
                case Guid:
                    return $"{property.Name} UNIQUEIDENTIFIER NOT NULL,";
                case GuidNull:
                    return $"{property.Name} UNIQUEIDENTIFIER NULL,";
                case Int32:
                    return $"{property.Name} INT NOT NULL,";
                case Int64:
                    return $"{property.Name} BIGINT NOT NULL";
                case String:
                    return $"{property.Name} NVARCHAR(max) NOT NULL,";
                default:
                    throw new InvalidOperationException("PropertyInfo value is not valid");
            }
        }
    }
}