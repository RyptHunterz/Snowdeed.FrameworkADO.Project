using Snowdeed.FrameworkADO.Core;
using Snowdeed.FrameworkADO.TestAPI.Entities;

namespace Snowdeed.FrameworkADO.TestAPI.Core
{
    public class TestDbContext(string connectionString) : DbContext(connectionString)
    {
        private DbSet<Employee>? employee;
        public DbSet<Employee> Employee => employee ??= new DbSet<Employee>(connection, databaseName);
    }
}
