using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.AppCode
{
    public class Logger
    {
        static public void Log(loggingMethod loggingMethod, string message)
        {
            Console.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " " + loggingMethod.ToString() + ": " + message);
        }
    }
}
