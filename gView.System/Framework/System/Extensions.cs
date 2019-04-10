using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace gView.Framework.system
{
    static public class Extensions
    {
        static public T NonblockingResult<T>(this Task task)
        {
            var nonBlockingTask = Task.Run(() => task);
            nonBlockingTask.Wait();

            var property=task.GetType().GetProperty("Result");
            if (property != null)
                return (T)property.GetValue(task);

            return default(T);
        }
    }
}
