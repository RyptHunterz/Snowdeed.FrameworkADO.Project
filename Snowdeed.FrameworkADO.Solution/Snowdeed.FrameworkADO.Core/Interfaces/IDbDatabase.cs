using System.Collections.Generic;

namespace Snowdeed.FrameworkADO.Core.Interfaces
{
    public interface IDbDatabase
    {
        T GetByOneStoredProcedure<T>(string storedProcedure, object parameters) where T : new();
        IEnumerable<T> GetByListStoredProcedure<T>(string storedProcedure, object parameters) where T : new();
        bool ExecuteStoredProcedure(string storedProcedure, object parameters);
    }
}