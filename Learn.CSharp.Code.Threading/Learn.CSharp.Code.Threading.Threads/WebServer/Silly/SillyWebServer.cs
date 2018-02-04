﻿namespace Learn.CSharp.Code.Threading.Threads.Simple
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    
    using WebServer;
    
        
    /// <summary>
    /// Когда-то до начала времен никаких тасков не существовало и все использовали 
    /// потоки, то есть создавался прямо почти настоящий поток, почти настоящий,
    /// потому что это и правда -- настоящий поток со своими ресурсами, но при этом несколько потоков
    /// .NET могут выполняться в одном потоке ОС, поэтому -- "почти".
    /// Я буду показывать многопоточность, наверное по шагам, здесь сделаем все вообще в лоб.
    /// </summary>
    public class SillyWebServer : IWebServer
    {
        private const int MaxHandlersCount = 100;
        
        private readonly WebServerOptions _options;
        private readonly Thread _thread;
        private readonly HttpListener _listener;
         
        /// <summary>
        /// См. ILock и его реализации.
        /// Здесь мы используем монитор, потому что коллекция может быть заблокирована на достаточно длительное время,
        /// пока из нее не удалятся завершенные потоки (см. ниже).
        /// </summary>
        private readonly ILock _lock;

        /// <summary>
        /// Сюда будем класть потоки, которые обрабатывают запросы.
        /// Это плохо, от этого бывает OutOfMemoryException,
        /// Да, я сейчас покажу, как добиться утечки памяти в .NET))
        /// Хотя мы это героически решим)))
        /// </summary>
        private readonly List<Thread> _handleThreads = new List<Thread>();

        
        private bool _disposed;

        
        
        public SillyWebServer(WebServerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _listener = new HttpListener();
            _thread = new Thread(Listen);
            _lock = new MonitorLock((_handleThreads as ICollection).SyncRoot);
            
            _listener.Prefixes.Add($"http://{_options.Host}:{_options.Port}/");
        }


        /// <summary>
        ///   Очевидно, что наш веб-сервер должен быть многопоточным, здесь стоит оговориться о том, как работают 
        /// TCP-сокеты (именно они лежат в основе всей сетевой разработки). В цикле сокет слушает какой-то порт и 
        /// пытается принять соединение, ждет его, на это время поток, который слушает сокет блокиуется, когда 
        /// появляется входящее соединение, создается новый сокет для поддержания соединения с клиентом, 
        /// а сокет-слушатель освобождается и может дальше заниматься прослушиванием.
        /// 
        ///   Однако слушатель не будет ждать соединения, пока мы ему не скажем об этом, и, если мы выполняем код 
        /// обработки последовательно в том же потоке, что и прослушивание мы не можем попросить сокет слушать порт,
        /// потому что тогда поток заблокируется и мы не сможем обработать соединение... на этом месте не могу не 
        /// вспомнить вынесшую уймы мозгов фразу: "Питер больше, чем в два раза меньше, чем Москва")), я с помощью нее 
        /// объяснял новисам, что такое вложенный запрос)))) 
        ///  
        ///   Из всего нам становится понятно, что, когда пришло соединение мы должны обработать его в другом потоке.
        /// </summary>
        public void Start()
        {
            /*
             *   Следует помнить, что остановленный (принудительно или нет -- не важно) поток нельзя запустить заново,
             * то есть мы не можем переиспользовать уже остановленный поток
             *
             *   Стоит также рассказать о методах Suspend и Resume и о том, почему они являются нерекомендуемыми:
             * Эти методы приостанавливают и возрождают поток соответственно, однако имеют несколько проблем
             *
             *   Первая проблемма в том, что Suspend на самом деле не останавливает поток сразу, а только сообщает
             * планировщику о том, что его надо приостановить. Планировщик дожидается, когда поток дойдет до т.н.
             * безопасной точки, где можно прервать выполнение кода и только тогда отнимает у потока квант времени и
             * переводит его в состояние Suspended, то есть рантайм и только рантайм решает, когда приостановится поток
             * на самом деле, таким образом, если в потоке выполняются какие-либо "критичные" с точки зрения рантайма
             * операции, то есть шанс, что поток не приостановится, а вызов Suspend еще и заблокирует поток из которого
             * он был выполнен.
             *
             *   Вторая заключается в том, что вызывающий не знает, на каком моменте остановится поток и какой код
             * выполнится, а какой -- нет. Давай рассмотрим в контексте веб-сервера. Пусть у нас есть 1000 одновременных
             * соединений и они обрабатываются 1000 потоками. Зачем-то мы вызвали на них Suspend (а я, кстати, так
             * на своей первой работе и делал)))), при этом, никаких критичных действий (например, ввода-вывода) они 
             * сейчас не выполняют, все, суспендим. А засуспендил его горе-человек-админ, который потом забыл его
             * запустить заново, и в принципе, ничего страшного, мы перезапустим, только вот у машины только что исчезло
             * 1000 портов. Планировщик просто убил потоки, которые приостановлены, так и не выполнив код очистки.
             */
                        
            // Запускаем поток обработки соединений
            _thread.Start();
        }


        public void Stop(int timeoutMs = Timeout.Infinite)
        {
            if (!_listener.IsListening) return;
            
            if (timeoutMs != Timeout.Infinite && timeoutMs <= 0)
                throw new ArgumentException();
            
            // Останавливаем прослушивание порта
            _listener.Stop();

            // Ждем завершения потока этот вызов блокирует поток и ждет указанное время или
            // пока поток _thread не завершится
            _thread.Join(timeoutMs);

            // если поток не завершен через указанное время, то, возможно, он заблокирован
            // GetContext -- блокирующий вызов и нам надо остановить его принудительно.
            if (_thread.IsAlive) _thread.Abort();
            
            // Ждем завершения потоков обработки, мы хотим чтобы они завершились нормально.
            // Для этого блокируем очередь.
            _lock.Execute(() =>
            {
                _handleThreads.ForEach(t =>
                {
                    // Также ждем завершения всех потоков
                    // И, если надо, останавливаем их принудительно.
                    t.Join(timeoutMs);
                    if (t.IsAlive) t.Abort();
                });
            });
        }


        public void Dispose() => Stop();


        private void CloseConnections() => _listener.Close();

        
        private void Listen()
        {
            _listener.Start();

            try
            {
                // Крутимся, пока слушатель активен
                while (_listener.IsListening)
                {
                    // Оборачиваем в try, чтобы одним неудачным соединенем не убить приложение, мы же не node.js
                    try
                    {
                        // Ждем соединения, поток заблокирован
                        var context = _listener.GetContext();
                        
                        // В этот момент слушатель снова может принимать соединения,
                        // Но ему мешает код ниже, следовательно код ниже должен выполняться,
                        // как можно, быстрее.
                        
                        // Просто берем и создаем здесь поток
                        // Этот код здесь, потому что мы должны помнить, как делать нельзя.
//                        var handleThread = new Thread(() => HandleConnection(context.Request, context.Response));
//                        
//                        _handleThreads.Add(handleThread);
//                        
//                        handleThread.Start();    

                        // Фиксируем, что создали его и забываем и  в этом есть фундаментальная ошибка. В какой-то 
                        // момент потоков окажется очень много, после чего:
                        // 
                        // - коллекция попадет в кучу больших объектов, надо всего лишь 10625 запросов, а это ой, 
                        //   как мало, это число легко посчитать: 85000 (размер объекта для попадания в LOH) делим на 8 
                        // (размер указателя). Более того, поскольку массив нельзя увеличить на месте, он будет попадать
                        // в кучу больших объектов каждый раз при изменении размера, да в конце концов он будет 
                        // собираться GC, но есть нюанс: куча больших объектов не дефрагментируется. То есть, возможна
                        // ситуация, когда у тебя есть 256 мегабайт памяти, а выделить 85001 байт ты не можешь, потому 
                        // что в обычную кучу столько не положишь, а самый длинный кусок в куче больших -- 85000 байт,
                        // ты ведь помнишь функции GetMemAvail и GetMaxAvail из турбопаскаля? Вот они об этом.
                        //
                        // - каждый мертвый поток также содержит в себе состояние, а это уже несколько больше,
                        // на вскидку, около 64 байт, то есть, памяти в этом случае ушло уже 680000 байт. Вся эта память
                        // радостно оккупировала к этому времени уже второе поколение, то есть, не будет собрана 
                        // никогда. 
                        // 
                        // - мы не знаем, как конкретно релизованы потоки в данной версии фреймворка (да, именно, тот 
                        // самый случай, когда #depends), то есть очень может быть, что ресурсы потока освобождаются 
                        // только при сборке объекта сборщиком мусора, а это -- около 1MB на поток, то есть вот тебе
                        // и -10 метров памяти;
                        // 
                        // Все это некритично в большинстве случаев, но мы пишем сетевое ПО,следовательно, по достижению 
                        // какого-то размера нам надо искать потоки, которые завершены и удалять их из коллекции, да, 
                        // не разобравшись и сделав самым легким способом в лоб, мы создали себе проблемы на потом.
                        //
                        // Код придется переписать так:

                        _lock.Execute(() =>
                        {
                            // Да, проверяем здесь, что сервер запущен.
                            if (!_listener.IsListening) return;
                           
                            // Удаляем потоки, которые завершили работу,
                            if (_handleThreads.Count >= MaxHandlersCount) _handleThreads.RemoveAll(t => !t.IsAlive);  
                            
                            
                            var handleThread = new Thread(() => HandleConnection(context.Request, context.Response));
                        
                            _handleThreads.Add(handleThread);
                            
                            handleThread.Start();
                        });
                    }
                    catch (Exception)
                    {
                        // Здесь предполагается логирование.
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // Здесь не делаем ничего и это, представь себе, в нашем случае -- норм, хотя,
                // в реальном приложении мы захотим добавить сюда сообщение о том, что что-то пошло не совсем так&
            }
            
            // Закрываем соединения.
            CloseConnections();
        }


        
        private void HandleConnection(HttpListenerRequest req, HttpListenerResponse res)
        {
            try
            {
                if (_options.Handlers.Any(h => h.HandleRequest(req, res))) return;
                res.NotFound();
            }
            catch (Exception x)
            {
                // Логирование
                res.IntenalServerError();
            }
        }
    }
}

/*
 * В этом примере показан многопоточный сервер и блокировки (мониторы);
 * Что здесь хорошо: сервер многопотчный
 * Плохо: мы бесконтрольно создаем новые потоки на каждое соединение, это не очень хорошо,
 * в следующем примере рассмотрим другой вариант -- с очередью соединений.
 */