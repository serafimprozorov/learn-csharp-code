namespace Learn.CSharp.Code.Threading.Threads.WebServer
{
    using System.Net;

    
    public interface IRequestHandler
    {
        bool HandleRequest(HttpListenerRequest req, HttpListenerResponse res);
    }
}