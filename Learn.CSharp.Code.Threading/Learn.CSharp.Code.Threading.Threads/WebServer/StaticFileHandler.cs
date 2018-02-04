namespace Learn.CSharp.Code.Threading.Threads.Simple
{
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    
    using Threads;

    using static System.StringComparison;
    
    
    public class StaticFileHandler : IRequestHandler
    {
        // Делаем буффер большим, но таким, чтобы он не попал в кучу больших объектов
        private const int BufferSize = 1024 * 1024 * 80;
        private const string DefaultMimeType = "application/octet-stream";
        
        private static readonly Dictionary<string, string> Mimes = new Dictionary<string, string>();
        
        private readonly string _root;


        public StaticFileHandler(string root) => _root = root;


        public bool HandleRequest(HttpListenerRequest req, HttpListenerResponse res)
        {
            if (!req.HttpMethod.Equals("get", OrdinalIgnoreCase)) return false;

            var path = Path.Combine(_root, req.Url.LocalPath);
            
            if (Directory.Exists(path)) return res.Forbidden();
            
            var file = new FileInfo(path);

            if (!file.Exists) return false;
            
         
            using (var stream = File.OpenRead(path))
            {
                res.ContentType = Mimes.TryGetValue(file.Extension, out var mimeType) ? mimeType : DefaultMimeType;
                res.ContentLength64 = file.Length;
                
                var buffer = new byte[BufferSize];
                var count = 0;
                
                do
                {
                    if ((count = stream.Read(buffer, 0, buffer.Length)) == 0) break;
                    res.OutputStream.Write(buffer, 0, count);
                }
                while (count != 0);
            }

            return res.Ok();
        }
    }
}