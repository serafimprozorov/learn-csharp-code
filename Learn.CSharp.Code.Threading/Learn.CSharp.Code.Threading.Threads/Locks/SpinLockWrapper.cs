namespace Learn.CSharp.Code.Threading.Threads
{ 
    using System.Threading;
    
    
    /// <summary>
    /// 
    ///   Обертка над SpinLock.
    /// 
    ///   Отличается от Montor тем, что не отдает квант времени другому потоку,
    /// а действительно ждет освобождения блокировки, выполняя в потоке некоторое количество операций NOP (ассемблерный 
    /// мнемокод команды 90 (no operation, пропустить такт)). Очевидно, что это удобно тем, что нам не надо переключать 
    /// контекст.
    /// 
    ///   Эту блокировку стоит использовать, когда велик шанс того, что за то время, пока поток "спинится", блокировка 
    /// освободится и он сможет ее получить. В противном случае вычислительные ресурсы будут использоваться 
    /// неоптимально: поток активен, а делать ничего не может. В следствие последнего, часто используют гибридные
    /// блокировки (spin + monitor, spin + mutex);
    /// 
    ///   Спинлок нереентернабелен, если попытаться захватить второй раз произойдет исключение. Эта ситуация 
    /// обрабатывается в Acquire.
    /// 
    ///   Как его использовать и почему -- покажу в примере SimpleWebServer.
    ///  
    /// </summary>
    public class SpinLockWrapper : ILock, ILockCookie
    {
        private readonly SpinLock _spinLock = new SpinLock();

        private bool _taken;


        public ILockCookie Acquire(int timeoutMs = 0)
        {
            if (_taken) return this;

            _spinLock.TryEnter(timeoutMs, ref _taken);

            return this;
        }


        /// <summary>
        /// Здесь стоит проверить, что блокировка захвачена именно текущим потоком
        /// Для спинлока это важно
        /// </summary>
        public bool IsTaken => _taken && _spinLock.IsHeldByCurrentThread;
        
        public void Release()
        {
            if (!IsTaken) return;
            _taken = false;
            _spinLock.Exit();
        }

        
        public void Dispose() => Release();
    }
}