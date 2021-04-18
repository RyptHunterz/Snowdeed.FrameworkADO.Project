using System.Collections.Generic;

namespace Snowdeed.FrameworkADO.Core.Interfaces
{
    public interface IDbSet<T> where T : class
    {
        IEnumerable<T> GetAll();

        //public T GetById(object Id);

        bool Add(T entity);

        bool Udpate(T entity);

        bool Delete(T entity);
    }
}