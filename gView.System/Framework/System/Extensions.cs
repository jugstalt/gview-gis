using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace gView.Framework.system
{
    static public class Extensions
    {
        //static public T NonblockingResult<T>(this Task task)
        //{
        //    var nonBlockingTask = Task.Run(() => task);
        //    nonBlockingTask.Wait();

        //    var property=task.GetType().GetProperty("Result");
        //    if (property != null)
        //        return (T)property.GetValue(task);

        //    return default(T);
        //}

        static public string ExtractConnectionStringParameter(this string connectionString, string parameterName)
        {
            parameterName = parameterName.ToLower();

            foreach(var p in connectionString.Split(';'))
            {
                if(p.ToLower().StartsWith(parameterName+"="))
                {
                    return p.Substring(parameterName.Length + 1);
                }
            }

            return String.Empty;
        }
    }
}
