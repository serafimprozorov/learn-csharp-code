namespace Learn.CSharp.Code.Threading.Threads
{
    /// <summary>
    /// Обертка над блокировками.
    /// Реализации мною придуманы и написаны в далеких 2011..2012, так что не ручаюсь за оптимальность,
    /// </summary>
    public interface ILock
    {
        /// <summary>
        /// Пытается получить блокировку в течение указанного времени,
        /// </summary>
        /// <param name="timeoutMs">-1 (Timeout.Infinite) для бесконечного ожидания.</param>
        ILockCookie Acquire(int timeoutMs = -1);
    }
}