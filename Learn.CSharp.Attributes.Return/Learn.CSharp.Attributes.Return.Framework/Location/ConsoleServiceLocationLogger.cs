namespace Learn.CSharp.Attributes.Return.Framework.Location
{
    using System;
    
    
    public class ConsoleServiceLocationLogger : IServiceLocationLogger
    {
        public void LogLocation(ServiceLocationContext context)
        {
            Console.Write($"[{GetType().Name}]: ");
            Console.WriteLine(
                context.Error?.ToString() ?? $"Matched: {context.MatchingType.Name}.{context.Method.Name}");
        }
    }
}