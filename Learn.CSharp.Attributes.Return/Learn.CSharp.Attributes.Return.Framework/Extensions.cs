namespace Learn.CSharp.Attributes.Return.Framework
{
    using System;
    using System.Collections.Generic;    
    
    
    public static class Extensions
    {
        public static TF DoWhile<T, TF>(this IEnumerable<T> @this, Action<T> action, TF flag, Func<TF, bool> condition)
            where T : class
        {
            foreach (var i in @this)
            {
                action(i);
                if (!condition(flag)) break;
            }

            return flag;
        }

        public static void FailIf<T>(this T @this, Func<T, bool> condition, Func<Exception> @throw)
        {
            if (!condition(@this)) return;
            throw @throw();
        }
    }
}