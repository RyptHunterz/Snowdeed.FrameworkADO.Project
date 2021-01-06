using System;
using System.Collections.Generic;

namespace Snowdeed.FrameworkADO.Core.Interfaces
{
    public interface IDbSet<T> where T : class
    {
        public IEnumerable<T> GetAll();

        //public T GetById(object Id);

        public int Add(T entity);

        public int Udpate(T entity);

        public int Delete(T entity);

    }
}
