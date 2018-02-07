namespace LearnCSharp.Code.Threading.Tasks.WebServer
{
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    
    
    public interface IRequestHandler
    {
        Task<bool> HandleRequest(HttpListenerRequest req, HttpListenerResponse res, CancellationToken ct);
    }
}