namespace Learn.CSharp.Attributes.Return.Framework.Invocation
{
    using System;

    public class ConsoleSeviceInvocationLogger : IServiceInvocationLogger
    {
        public void LogInvocation(ServiceInvocationContext ctx)
        {
            Console.Write($"[{GetType().Name}]: ");
            Console.Write($"Done action [{ctx.Id}]. ");

            Console.WriteLine(ctx.Error != null
                ? $"Error occured: {ctx.Error}"
                : $"Originally fetched: {ctx.FetchedResult}. Final result: {ctx.Result}");
        }
    }
}