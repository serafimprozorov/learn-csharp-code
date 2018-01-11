namespace Learn.CSharp.Attributes.Return.Framework.Location
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;    
    
    
    public class ServiceLocationContext
    {
        public ServiceLocationContext(string entityName, string actionName, Type[] parameters)
        {
            Id = Guid.NewGuid();
            EntityName = entityName;
            ActionName = actionName;
            Parameters = parameters;
        }
        
        
        public Guid Id { get; }        
        
        public string EntityName { get; }
        
        public string ActionName { get; }

        public Type[] Parameters { get; }

        public Exception Error { get; set; }

        public Type MatchingType { get; set; }

        public MethodInfo Method { get; set; }
    }
}