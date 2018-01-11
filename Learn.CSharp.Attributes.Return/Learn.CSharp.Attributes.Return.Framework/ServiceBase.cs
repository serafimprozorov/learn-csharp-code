namespace Learn.CSharp.Attributes.Return.Framework
{
    using System;
    using System.Collections.Generic;
    
    
    public class ServiceBase<TI, T> : IService<TI, T>
    {
        public virtual T Get(TI id) => throw new NotImplementedException();

        public virtual IEnumerable<T> Search(ISpecification<T> spec) => throw new NotImplementedException();
        
        public virtual void Save(T entity) => throw new NotImplementedException();

        public virtual void Delete(TI id) => throw new NotImplementedException();
    }
}