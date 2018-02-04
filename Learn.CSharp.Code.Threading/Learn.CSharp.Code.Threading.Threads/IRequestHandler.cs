using System.Net;

namespace Learn.CSharp.Code.Threading.Threads
{
    public interface IRequestHandler
    {
        bool HandleRequest(HttpListenerRequest req, HttpListenerResponse res);
    }
}