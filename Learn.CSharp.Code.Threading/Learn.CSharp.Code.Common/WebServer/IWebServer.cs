namespace Learn.CSharp.Code.Threading.Threads.WebServer
{
    using System;
    
    
    public interface IWebServer : IDisposable
    {
        void Start();

        void Stop(int timeoutMs = 0);
    }
}