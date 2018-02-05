using System;
using System.Threading;

namespace Learn.CSharp.Code.Threading.Threads
{
    /// <summary>
    /// 
    ///   Покажу на примере семафора т.н. "тонкие", они же -- гибридные объекты блокировки.
    /// Один из них -- SemaphoreSlim, то есть это -- семафор c точки зрения своего принципа действия, 
    /// однако он не является объектом уровня ядра, то есть реализован на уровне .NET.
    ///   
    ///   Гибридные блокировки используют SpinLock + Monitor, сначала поток пытается захватить спинлок,
    /// а потом использует мониторную блокировку.
    /// 
    /// </summary>
    public class SemaphoreLock : ILock, ILockCookie
    {
        private readonly SemaphoreSlim _semaphore;

        public SemaphoreLock(int initialCount, int maxCount) => _semaphore = new SemaphoreSlim(initialCount, maxCount);


        public ILockCookie Acquire(int timeoutMs = -1)
        {
            if (timeoutMs != Timeout.Infinite && timeoutMs < 1)
                throw new ArgumentException();

            IsTaken = _semaphore.Wait(timeoutMs);

            return this;
        }

        
        public bool IsTaken { get; private set; }

        public void Release()
        {
            if (!IsTaken) return;
            _semaphore.Release();
        }

        
        public void Dispose() => Release();
    }
}