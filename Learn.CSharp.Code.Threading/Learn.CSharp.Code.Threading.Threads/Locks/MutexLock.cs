using System;
using System.Threading;

namespace Learn.CSharp.Code.Threading.Threads
{
    /// <summary>
    /// 
    ///   Это -- мьютекс, объект блокировки ядра или, как принято называть, дескриптор ожидания (Wait Handle).
    /// Является достаточно медленным, т.к. для его использования необходимо переключения в режим ядра, однако,
    /// в отличие от объектов блокировки .NET могут быть использованы одновременно несколькими процессами.
    /// 
    ///   Для всех объектов блокировки ядра существуют их "тонкие" версии, они не могут быть использованы несколькими 
    /// процессами сразу, однако являются гибридными и достаточно быстры. Для мьютекса такой реализации нет,
    /// т.к. таким мьютексом по сути является монитор.
    ///   
    /// </summary>

    public class MutexLock : ILock, ILockCookie
    {
        private readonly Mutex _mutex;

        public MutexLock(Mutex mutex = null) => _mutex = mutex ?? new Mutex();
        
        
        public ILockCookie Acquire(int timeoutMs = -1)
        {
            if (timeoutMs != Timeout.Infinite && timeoutMs < 1)
                throw new ArgumentException();

            IsTaken = _mutex.WaitOne(timeoutMs);

            return this;
        }

        
        public bool IsTaken { get; private set; }

        public void Release()
        {
            if (!IsTaken) return;
            _mutex.ReleaseMutex();
        }


        public void Dispose() => Release();
    }
}