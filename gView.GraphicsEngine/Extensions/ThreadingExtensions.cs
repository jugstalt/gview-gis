using gView.GraphicsEngine.Threading;
using System;

namespace gView.GraphicsEngine.Extensions
{
    static public class ThreadingExtensions
    {
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
