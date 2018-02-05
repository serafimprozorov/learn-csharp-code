namespace Learn.CSharp.Code.Threading.Threads.AsyncIO
{
    using System.Net;
    
    
    public interface IAsyncRequestHandler
    {
        /// <summary>
        /// Это очень похоже на middleware из ASP.NET Core.
        /// </summary>
        void HandleRequest(HttpListenerContext context);

        IAsyncRequestHandler Next { get; }
    }
}