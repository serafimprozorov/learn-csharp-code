namespace Learn.CSharp.Code.Threading.Threads
{
    using System;
    using System.Threading;

    
    /// <summary>
    /// 
    ///   Обертка над мониторной блокировкой.
    /// 
    ///   По сути является подобием CRITICAL_SECTION из WinApi
    /// Текущий поток захватывает блокировку и, пока он ее не отпустит,
    /// ее не захватит никто.
    ///
    ///   При попытке захватить блокировку поток ждет указанное количество времени, 
    /// после чего отдает свой квант другому потоку (следующему в очереди), что это значит:
    ///  - сохранение стека потока;
    ///  - сохранение регистров и кеша процессора;
    ///  - передача управления другому потоку;
    ///   
    ///   Собственно, именно это называется переключением контекста и это -- не самая быстрая операция.
    /// 
    ///   Блокировка может быть реентернабельная и нереентернабельная, в первом случае поток может захватить ее дважды,
    /// надо понимать, что блокировку придется снимать столько раз, сколько ее захватили. 
    ///  
    /// </summary>
    public sealed class MonitorLock : ILock, ILockCookie
    {
        /// <summary>
        /// Собственно, объект.
        /// </summary>
        private readonly object _monitor;


        public MonitorLock(object monitor) => _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));


        public ILockCookie Acquire(int timeoutMs = 0)
        {
            if (timeoutMs != Timeout.Infinite && timeoutMs <= 0)
                throw new ArgumentException("Timeout must be positive number", nameof(timeoutMs));

            // Просто захватываем блокировку.
            IsTaken = Monitor.TryEnter(_monitor, timeoutMs);

            return this;
        }


        public bool IsTaken { get; private set; }

        public void Release()
        {
            if (!IsTaken) return;
            // Снимаем блокировку
            Monitor.Exit(_monitor);
            
            // И оповещаем потоки в очереди, что блокировка освобождена,
            // это позволяет планировщику работать более рационально
            Monitor.PulseAll(_monitor);
        }


        public void Dispose() => Release();
    }
}