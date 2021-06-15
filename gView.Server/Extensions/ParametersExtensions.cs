using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.Extensions
{
    static public class ParametersExtensions
    {
        static public string GetArgumentValue(this string[] args, string argument, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase)
        {
            if (args == null)
                return null;

            for (int i = 0; i < args.Length - 1; i++)
            {
                if(args[i]!=null && args[i].Equals(argument, stringComparison))
                {
                    return args[i+1];
                }
            }

            return null;
        }
    }
}
