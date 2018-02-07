using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Learn.CSharp.Code.Threading.Threads.WebServer;
using Learn.CSharp.Code.Threading.WebServer;


namespace LearnCSharp.Code.Threading.Tasks.OldStyle
{
    public class TaskWebServer : IWebServer
    {
        private readonly WebServerOptions _options;
        private readonly HttpListener _listener = new HttpListener();
        private readonly CancellationTokenSource _cancelSource = new CancellationTokenSource();
        
        public TaskWebServer(WebServerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _listener.Prefixes.Add($"http://{_options.Host}:{_options.Port}/");
        }



        public void Dispose() => Stop();

        public void Start() => Task.Factory.StartNew(Listen);
        

        public void Stop(int timeoutMs = 0)
        {
            if (!_listener.IsListening) return;
            _listener.Stop();
            _cancelSource.CancelAfter(timeoutMs);
        }


        private void Listen()
        {
            _listener.Start();

            while (!_cancelSource.IsCancellationRequested)
            {
                //
                //   Метод ContinueWith создает т.н. "продолжение", continuation, 
                // то есть продолжает выполнение задачи после получения результата.
                // интересно то, что, выполнение метода OnRequest может случиться 
                // уже в абсолютно другом потоке, нежели вызов GetContextAsync.
                // 
                //   Таски не имеют взаимно-однозначного соответствия с потоками и в 
                // одном потоке может выполнться несколько задач, при этом задачи управляются
                // событиями, например, так работают таки ввода-вывода (IOCP)
                // 
                _listener.GetContextAsync().ContinueWith(OnRequest, _cancelSource.Token);
            }
            
            _listener.Stop();
        }

        private async Task OnRequest(Task<HttpListenerContext> task)
        {
            //   ContinueWith не проверяет статус задачи, он просто обрабатывает ее дальше,
            // то есть нам обязательно надо проверить не произошло ли ошибки.
            //   
            //   Ошибки, произошедште при выполнении таска оборачиваются в AggregatedExcetion,
            // писать try-catch в вызывающем коде бессмысленно, потому что таск выполняется асинхронно,
            // с большой вероятностью в другом потоке, блок же try-catch выполняется в вызывающем потоке
            // и закончится к тому времени, как произойдет исключение.
            //   
            //   Кроме ошибки таск может быть отменен, для этого используется объект CancellationToken,
            // его принимают все асинхронные методы BCL, также стоит передавать его во все создаваемые 
            // асинхронные методы.
            //
            //   Стоит пояснить, что CancellationToken на самом деле никак не связан с таском, то есть внутри кода 
            // необходимо проверять свойство CancellationToken.IsCancellationRequested или вызывать метод 
            // ThrowIfCancellationRequested, последний бросает исключение TaskCancelledException, которое автоматически
            // ставит таску свойство IsFaulted = true.
            //
            //   Для создания CancellationToken используется CancellationTokenSource cts, когда на cts вызывается одна 
            // из перегрузок метода Cancel. В этом случае ВСЕ CancellationToken, созданные им получают сигнал отмены.
            // По факту CancellationToken -- это структура, содержащая всего одно поле типа CancellationTokenSource.
            // 
            // Есть еще один нюанс: он не создает новую задачу, он изменяет сам объект.
            //
            if (task.IsCanceled || task.IsFaulted) return;

            //   Представь, что дл каждого асинхронного вызова нам нужен был бы коллбек, был бы ад, как в ноде,
            // await делает то же самое, что и ContinueWith, только в случае его использования компилятор
            // генерирует весь вспомогательный код (проверка статусов таски, генерация класса для хранения состояния и
            // подобное, нижк покажу)
            //
            //   То есть await делает ни что иное, как создает continuation, являясь по большому счету синтаксическим 
            // сахаром для ContinueWith. То есть, он как бы говорит: ты пока поделай что-то еще, а, когда вот эта таска
            // завершится, я тебя оповещу и ты сделай то, что написано после.
            //
            try
            {
                var handled = false;
                
                // Если бы не было await, то нам бы было надо:
                //
                // - Реализовать итератор самостоятельно (то есть написать некий метод, который принимает
                //   таску и следующий хендлер и вызывать его в ContinueWith), либо объявить некую глобальную
                //   переменную handled и позаботиться о синхронизации доступа к ней.
                //
                // - Проверять состояние таски вручную (как выше)
                //
                foreach (var handler in _options.Handlers)
                {
                    // Здесь же мы просто пишем код, как будто он синхронный
                    // Компилятор создает конечный автомат с перемненной состояния типа bool (handled)
                    handled = await handler.HandleRequest(task.Result.Request, task.Result.Response,
                        _cancelSource.Token);
                    if (handled) break;
                }
                
                if (!handled) task.Result.Response.NotFound();
            }
            // Catch развернется в проверку статуса каждой таски
            catch (Exception x)
            {
                task.Result.Response.IntenalServerError();
            }
            
            // По сути функция выглядит так, как будто она возвращает void, то есть мы не возвращаем таску.
            // Про await расскажу в StaticFileHandler
        }
    }
}