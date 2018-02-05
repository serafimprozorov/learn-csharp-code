using Learn.CSharp.Code.Threading.Threads.WebServer;

namespace Learn.CSharp.Code.Threading.Threads.AsyncIO
{
    using System;
    using System.Net;
    using System.Threading;
    
    using static System.Threading.ThreadState;
    
        
    /// <summary>
    /// И так, мы знаем, что не использовать асинхронный ввод-вывод -- моветон 
    /// Исправимся. Главное отличие от многопоточного программирования в том, что асинхронный код 
    /// не принято синхронизировать, он работает на событиях и коллбеках.
    /// Синхронизация и блокировки нужны крайне редко.
    /// Здесь мы не будем использовать блокировки, не будем использовать очередьа будем использовать события.
    /// </summary>
    public class AsyncWebServer : IWebServer
    {
        private readonly WebServerOptions _options;
        private readonly Thread _thread;
        private readonly HttpListener _listener;
        
        //private bool _disposed;
        
        
        public AsyncWebServer(WebServerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _listener = new HttpListener();
            _thread = new Thread(Listen);
            
            _listener.Prefixes.Add($"http://{_options.Host}:{_options.Port}/");
        }


        public void Start()
        {
            _listener.Start();
            _thread.Start();
        }


        public void Stop(int timeoutMs)
        {
            _listener.Stop();
            _thread.Join(timeoutMs); // Ждем завершения потока прослушивания
        }

        public void Dispose()
        {
            if (!_listener.IsListening) return;
           
            _listener.Stop();

            if (!_thread.IsAlive) return; 
            
            _thread.Abort();
        }


        private void OnRequest(IAsyncResult iar)
        {
            // Ждем сигнального состояния события, ниже будет понятно зачем.
            // _waitHandle.Wait();
            
            // Увеличиваем счетчик выполняющихся потоков
            // Если он превышает максимальное количество, то сбрасываем событие
//            if (Interlocked.Increment(ref _currentConcurrency) >= _options.Concurrency)
//                _waitHandle.Reset();

//            HttpListenerContext context = null;

//            try
//            {
//                context = ((HttpListener) iar.AsyncState).EndGetContext(iar);
//                
//                foreach (var handler in _handlers)
//            }
//            catch (Exception)
//            {
//                context?.Response.IntenalServerError();
//            }
//            finally
//            {
//                // Уменьшаем счетчик и, если надо, устанавливаем событие в сигнальное состояние, теперь
//                // в очередь могут встать новые соединения, при этом у нас по факту нет блокировок и мы не
//                // тратим потоки из пула беспорядочно;
//                if (Interlocked.Decrement(ref _currentConcurrency) < _options.Concurrency)
//                    _waitHandle.Set();
//            }
        }
        
        private void Listen()
        {
            _listener.Start();

            try
            {
                while (_listener.IsListening)
                {
                    // Оборачиваем в try, чтобы одним неудачным соединенем не убить приложение.
                    try
                    {
                        //   Мы как бы говорим: вызови как вон тот метод ввода-вывода в потоке из пула и вызови вон ту 
                        // функцию, когда закончишь (в ноль, как в node)
                        // 
                        //   Мы не блокируем поток, мы не ждем окончания установки соединения, мы сразу же начинаем 
                        // слушать сокет дальше. Используется коллбек.
                        _listener.BeginGetContext(OnRequest, _listener);
                    }
                    catch (Exception)
                    {
                        // Здесь тоже предполагается логирование.
                    }
                }
            }
            catch (ThreadAbortException)
            {
                
            }
            
            
            // CloseConnections();
        }
    }
}

/*
 *   В этом примере показан многопоточный сервер, дескрипторы ожидания и асинхронное программирование, как таковое
 *
 * Хорошо: сервер многопотчный, использует IOCP
 * Плохо: код не очобо легко читаем, сама по себе абстракция потока достаточно тяжела и относительно устарела, по
 *        большому счету, использовать поток на каждую асинхронную операцию тоже довольно затратно, посему,
 *        в следующем примере используем таски.
 */







