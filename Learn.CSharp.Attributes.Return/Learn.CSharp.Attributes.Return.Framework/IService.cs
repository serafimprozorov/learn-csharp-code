using System.Collections.Generic;

namespace Learn.CSharp.Attributes.Return.Framework
{
    // Теперь я знаю, зачем in/out, надеюсь, ты -- тоже.
    public interface IService<in TI, T>
    {
        T Get(TI id);
        
        IEnumerable<T> Search(ISpecification<T> spec);

        void Save(T entity);

        void Delete(TI id);
    }
}