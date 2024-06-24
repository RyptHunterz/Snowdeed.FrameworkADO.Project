using System.Reflection;

namespace Snowdeed.FrameworkADO.Core.Extensions
{
    public static class EnumExtension
    {
        public static string GetEnumValue<T>(this PropertyInfo property, T entity) where T : class
        {
            return ((int)property.GetValue(entity)).ToString();
        }
    }
}