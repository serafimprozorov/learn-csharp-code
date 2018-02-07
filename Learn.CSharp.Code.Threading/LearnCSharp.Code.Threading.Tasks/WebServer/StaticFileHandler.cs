using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Learn.CSharp.Code.Threading.WebServer;
using static System.StringComparison;

namespace LearnCSharp.Code.Threading.Tasks.WebServer
{
    public class StaticFileHandler : IRequestHandler
    {
        // Делаем буффер большим, но таким, чтобы он не попал в кучу больших объектов
        private const int BufferSize = 1024 * 1024 * 80;
        private const string DefaultMimeType = "application/octet-stream";
        
        private static readonly Dictionary<string, string> Mimes = new Dictionary<string, string>();
        
        private readonly string _root;
        
        
        public StaticFileHandler(string root) => _root = root;
        
        
        public async Task<bool> HandleRequest(HttpListenerRequest req, HttpListenerResponse res, CancellationToken ct)
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
                    // Просто представь вот это на коллбеках, экстраполируй сюда метод OnRequest)))
                    if ((count = await stream.ReadAsync(buffer, 0, buffer.Length, ct)) == 0) break;
                    await WriteToResponse(res, buffer, count);
                }
                while (count != 0);
            }

            return res.Ok();
        }

        // Метод ненужный, но на его примере покажу назначение async
        // Этот метод написан некорректно, присмотрись, что вызывает диссонанс?
        // Ниже -- правильная реализация, посмотри после того, как ответишь на вопрос сама
        private async Task WriteToResponse(HttpListenerResponse res, byte[] buffer, int count) => 
            await res.OutputStream.WriteAsync(buffer, 0, count);
        
        
        
        
        
        
        
        
        
        
        #region Правильная реализация
        
//        private Task WriteToResponse(HttpListenerResponse res, byte[] buffer, int count) => 
//            res.OutputStream.WriteAsync(buffer, 0, count);

        // async на самом деле не играет такой сакральной роли, как await, он просто подсказывает компилчтору,
        // что в методе есть асинхронные вызовы, то есть, если метод просто запускает асинхронный метод (метод, 
        // возвращающий таску), а такое часто бывает, например в MVC-контроллерах, то его не нужно помечать, как 
        // await, потому что никакого "продолжения" у него не будет, следует просто вернуть результат его вызова,
        // то есть просто вернуть таску безо всяких ожиданий ее завершения. 
        // await в это месте в лучшем случае просто не нужен, в худшем (тут #depends) создает оверхед
        // в виде конечного автомата, разделенного состояния и проверок.
        #endregion
        
        
        // Жду твоих вопросов
    }
}