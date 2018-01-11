using System.Collections.Generic;
using System.Reflection;

namespace Learn.CSharp.Attributes.Return.Framework.Location
{
    using System;
    using System.Linq;
    
    using static System.StringComparison;
    
    
    public class ServiceLocator
    {
        private const string Service = nameof(Service);

        private readonly IServiceLocationLogger _logger;

        public ServiceLocator(IServiceLocationLogger logger) =>
            _logger = logger ?? throw new ArgumentNullException(nameof(logger)); 
        
        
        private static void TryMatchService(Type type, ServiceLocationContext ctx)
        {
            var name = $"{ctx.EntityName}{Service}";

            if (!type.Name.Equals(name, OrdinalIgnoreCase) || type.GetInterface(name, true) != null)
                return;

            ctx.MatchingType = type;

            foreach (var method in type.GetMethods())
            {
                if (!method.Name.Equals(ctx.ActionName, OrdinalIgnoreCase)) continue;

                var parameters = method.GetParameters().Select(p => p.ParameterType).ToArray();

                if (parameters.Length != ctx.Parameters.Length) continue;

                var i = parameters.Length;

                while (--i >= 0 && parameters[i] != ctx.Parameters[i]) { }

                if (i > 0) continue;

                ctx.Method = method;

                break;
            }
        }
        
        
        // Тут вообще все было сложнее: кровь, кишки, рефлексия, но о них мы поговрим,
        // когда ты дойдешь до них, пока -- так, как есть, очень упрощено, потом допишу, заодно и фреймворк добью))
        public void LocateService(ServiceLocationContext ctx)
        {
            try
            {
                AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Where(asm =>
                        asm.FullName.IndexOf("System.", OrdinalIgnoreCase) == -1 &&
                        asm.FullName.IndexOf("mscorlib", OrdinalIgnoreCase) == -1)
                    .SelectMany(a => a.GetExportedTypes())
                    .DoWhile(t => TryMatchService(t, ctx), ctx, c => c.Method == null)
                    .FailIf(c => c.Method == null, () => new Exception("No type found"));
            }
            catch (Exception x)
            {
                ctx.Error = x;
            }
            finally
            {
                _logger.LogLocation(ctx);
            }
        }
    }
}