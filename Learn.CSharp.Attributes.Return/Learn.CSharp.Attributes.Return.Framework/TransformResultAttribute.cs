namespace Learn.CSharp.Attributes.Return.Framework
{
    using System;    
    
    [AttributeUsage(AttributeTargets.ReturnValue)]
    public class TransformResultAttribute : Attribute
    {
        public TransformResultAttribute(Type transformer) =>
            Transformer = transformer ?? throw new ArgumentNullException(nameof(transformer));
        
        
        public Type Transformer { get; }
    }
}