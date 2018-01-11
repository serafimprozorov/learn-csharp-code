namespace Learn.CSharp.Attributes.Return.Framework.Invocation
{
    using System;
    using System.Linq;
    
    using static System.Reflection.BindingFlags;
    
    using Function = System.Func<object, object>;
    
    
    // Вот примерно такой класс у нас есть, который вызыввает сервисы,
    // Я тогда примерно такое писал.
    public class ServiceInvoker
    {
        private const string Execute = nameof(Execute);

        private readonly IServiceInvocationLogger _logger;
    
        public ServiceInvoker(IServiceInvocationLogger logger) =>
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));


        
        private static Function CreateTransformator(Type transformerType, ServiceInvocationContext ctx)
        {
            // Предположим, что мы все сделали верно и у нас есть кеш трансформаторов.
            var method = transformerType.GetMethod(Execute);

            if (method == null) throw new MissingMethodException(transformerType.Name, Execute);

            if (!ctx.Method.ReturnType.IsAssignableFrom(method.ReturnType))
                throw new Exception("Inconsistent argument type");                
                
            var arg = method.GetParameters().SingleOrDefault();

            if (arg == null) throw new Exception("Inconsistent parameters count");
                
            if (!arg.ParameterType.IsAssignableFrom(ctx.Method.ReturnType))
                throw new Exception("Inconsistent argument type");
               
            object instance = null;

            if (!method.IsStatic)
            {
                instance = Activator.CreateInstance(transformerType);
            }

            return obj => method.Invoke(instance, new[] {obj});
        }

        
        public void InvokeService(ServiceInvocationContext ctx)
        {
            try
            {
                var instance = Activator.CreateInstance(ctx.Type);
                
                // ... 
                // ... Здесь всякая валидация, авторизация, аутентификация и прочее, сделанное тоже через атрибуты
                // ... Поясню, почему здесь лучше применять атрибут к возвращаемому значению:
                // ... То, что мы что-то хотим сделать с возвращаемым в конкретном случае значением
                // ... относится к нему и только к нему, то есть мы не хотим логировать все результаты, мы хотим
                // ... залогировать (или сделать что-то еще) конкретный результат.
                // ... в совсем идеальном случае у нас должен быть атрибут, который умеет применить атрибут
                // ... в зависимости от значения. Это я тоже покажу, даже, наверное, сегодня (11.01.2018), но
                // ... сначала склонируй репу.
                // ...

                ctx.FetchedResult = ctx.Method.Invoke(instance, ctx.Arguments);

                if (ctx.Method.ReturnParameter == null) return;
                
                var transformators = ctx.Method.ReturnParameter
                    .GetCustomAttributes(true).OfType<TransformResultAttribute>()
                    .Select(a => CreateTransformator(a.Transformer, ctx));

                // Вот это вот то, о чем я говорил, атрибут применится не к типу, а миенно к значению,
                // эта строка не сработает:
                // var ret = ctx.Method.ReturnType
                //     .GetCustomAttributes(true)
                //     .OfType<TransformResultAttribute>()
                //     .Select(a => CreateTransformator(a.Transformer, ctx));                    

                ctx.Result = transformators.Aggregate(ctx.FetchedResult, (current, t) => t.Invoke(current));

                // ...
                // ... Тут опять же всякое, которое должно выполняться после выполнения метода.
                // ...
            }
            catch (NotImplementedException)
            {
                ctx.NotImplemented = true;
            }
            catch (Exception x)
            {
                ctx.Result = null;
                ctx.Error = x;
            }
            finally
            {
                _logger.LogInvocation(ctx);
            }
        }
    }
}