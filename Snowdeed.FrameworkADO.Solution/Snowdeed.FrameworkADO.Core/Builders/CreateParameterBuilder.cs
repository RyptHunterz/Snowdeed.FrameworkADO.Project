using Microsoft.Data.SqlClient;
using System.Reflection;

namespace Snowdeed.FrameworkADO.Core.Builders
{
    public sealed record CreateParameterBuilder
    {
        public List<SqlParameter> Parameters { get; } = [];

        public CreateParameterBuilder CreateParameter(object parameters)
        {
            foreach (PropertyInfo propertyInfo in parameters.GetType().GetProperties())
            {
               Parameters.Add(new SqlParameter($"@{propertyInfo.Name}", propertyInfo.GetValue(parameters)));
            }

            return this;
        }
    }
}