namespace Learn.CSharp.Attributes.Return.Framework.Invocation
{
    using System;
    using System.Reflection;
    using System.Collections.Generic;

    using Location;
    
    
    public class ServiceInvocationContext
    {
        public ServiceInvocationContext(ServiceLocationContext ctx, object[] arguments)
        {
            Id = Guid.NewGuid();
            Type = ctx.MatchingType;
            Method = ctx.Method;
            Arguments = arguments;
            Warnings = new List<Exception>();
        }
        

        public Guid Id { get; }        
        
        public MethodInfo Method { get; }

        public Type Type { get; }

        public object[] Arguments { get; }
        
        public List<Exception> Warnings { get; }        

        public object FetchedResult { get; set;  }        
        
        public object Result { get; set;  }        
        
        public Exception Error { get; set; }
        
        public bool NotImplemented { get; set; }
    }
}