namespace Learn.CSharp.Attributes.Return.Framework
{
    public interface ISpecification<T>
    {
        bool SatisfiedBy(T entity);

        ISpecification<T> And(ISpecification<T> next);
        
        ISpecification<T> Or(ISpecification<T> next);
    }
}