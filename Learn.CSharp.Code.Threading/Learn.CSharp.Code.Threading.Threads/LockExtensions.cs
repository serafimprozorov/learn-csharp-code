namespace Learn.CSharp.Code.Threading.Threads
{
    using System;

    
    public static class LockExtensions
    {
        /// <summary>
        /// Выполняет что-то, если блокировку удалось захватить
        /// </summary>
        public static void Execute(this ILock @this, Action code)
        {
            using (var lc = @this.Acquire())
            {
                if (!lc.IsTaken) return;
                code();
            }
        }

        /// <summary>
        /// Выполняет что-то, если к моменту захвата блокировки выполняется условие
        /// </summary>
        public static void ExecuteIf(this ILock @this, Func<bool> condition, Action code)
        {
            using (var lc = @this.Acquire())
            {
                if (!condition() || !lc.IsTaken) return;
                code();
            }
        }
    }
}