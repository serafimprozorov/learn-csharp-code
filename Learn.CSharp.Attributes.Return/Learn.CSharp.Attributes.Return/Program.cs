using System.Reflection;

namespace Learn.CSharp.Attributes.Return
{
    using System;
    using System.Linq;    
    using Framework.Invocation;
    using Framework.Location;
    
    
    internal class Program
    {
        private readonly ServiceInvoker _invoker;
        private readonly ServiceLocator _locator;

        
        private Program(IServiceLocationLogger locationLogger, IServiceInvocationLogger invocationLogger)
        {
            _locator = new ServiceLocator(locationLogger);
            _invoker = new ServiceInvoker(invocationLogger);
        }
        

        private int Run()
        {
            try
            {
                CallGetUserById();
                CallGetUserProfile();

                return 0;
            }
            catch (Exception x)
            {
                Console.WriteLine(x.ToString());
                return -1;
            }
            // Обалдеть, как можно, я прямо порадовался =))
            finally
            {
                Console.WriteLine("Press <ANYKEY>");
                Console.ReadKey();                
            }
        }


        /// <summary>
        /// Эмулируем вызов экшна Get у контроллера UserController с параметром Id
        /// </summary>
        private void CallGetUserById() => InvokeMethod("User", "Get", new object[] {2});


        /// <summary>
        /// Эмулируем вызов экшна Get у контроллера UserProfileController с параметром Id
        /// </summary>
        private void CallGetUserProfile() => InvokeMethod("UserProfile", "Get", new object[] {2});

        
        
        private void InvokeMethod(string entityName, string actionName, object[] args)
        {
            var locationContext =
                new ServiceLocationContext(entityName, actionName, args.Select(a => a.GetType()).ToArray());
            
            _locator.LocateService(locationContext);

            if (locationContext.Error != null) throw locationContext.Error;

            var invocationContext = new ServiceInvocationContext(locationContext, new object[] {2});
            
            _invoker.InvokeService(invocationContext);
            
            if (invocationContext.NotImplemented) Console.WriteLine("Method not implemnented");
            if (invocationContext.Error != null) throw invocationContext.Error;
            
            Console.WriteLine($"Action {entityName}.{actionName} completed.");
        }



        public static int Main(string[] args) =>
            new Program(new ConsoleServiceLocationLogger(), new ConsoleSeviceInvocationLogger()).Run();
    }
}