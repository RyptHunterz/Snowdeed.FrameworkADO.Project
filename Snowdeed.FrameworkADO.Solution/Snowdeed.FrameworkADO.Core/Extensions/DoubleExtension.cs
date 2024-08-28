using System.Reflection;

namespace Snowdeed.FrameworkADO.Core.Extensions;

public static class DoubleExtension
{
    public static bool IsDouble(this Type type)
    {
        return type == typeof(double);
    }

    public static string GetDoubleValue<T>(this PropertyInfo property, T entity) where T : class
    {
        return property.GetValue(entity).ToString().Replace(',', '.');
    }
}
