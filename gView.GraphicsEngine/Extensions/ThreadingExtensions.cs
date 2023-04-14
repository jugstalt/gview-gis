using gView.GraphicsEngine.Threading;
using System;
using System.Runtime.CompilerServices;

namespace gView.GraphicsEngine.Extensions
{
    static public class ThreadingExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void InterLock(this IThreadLocker locker, Action action)
        {
            if (locker == null)
            {
                action();
            }
            else
            {
                lock (locker)
                {
                    action();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public T GetInterLocked<T>(this IThreadLocker locker, Func<T> func)
        {
            if (locker == null)
            {
                return func();
            }
            else
            {
                lock (locker)
                {
                    return func();
                }
            }
        }
    }
}
