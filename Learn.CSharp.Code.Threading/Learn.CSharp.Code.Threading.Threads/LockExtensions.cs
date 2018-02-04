using System;

namespace Learn.CSharp.Code.Threading.Threads
{
    public static class LockExtensions
    {
        public static void Execute(this ILock @this, Action code)
        {
            using (var lc = @this.Acquire())
            {
                if (!lc.IsTaken) return;
                code();
            }
        }

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