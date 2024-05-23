using gView.Framework.Common;
using gView.MxlUtil.Lib.Abstraction;
using gView.MxlUtil.Lib.Exceptions;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace gView.Cmd.MxlUtil
{
    class Program
    {
        async static Task<int> Main(string[] args)
        {
            PlugInManager.InitSilent = true;
            IMxlUtility utilityInstance = null;

            try
            {
                string utility = args.FirstOrDefault();

                var assemblyPath = $"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}/gView.MxlUtil.Lib.dll";
                var assembly = Assembly.LoadFrom(assemblyPath);
                utilityInstance =
                    assembly.GetTypes()
                        .Where(t =>
                        {
                            try
                            {
                                return
                                    typeof(IMxlUtility).IsAssignableFrom(t) &&
                                    ((IMxlUtility)Activator.CreateInstance(t)).Name.Equals(utility, StringComparison.InvariantCultureIgnoreCase);
                            }
                            catch { return false; }
                        })
                        .Select(t => Activator.CreateInstance(t) as IMxlUtility)
                        .FirstOrDefault();

                if (utilityInstance == null)
                {
                    var utilityInstances = assembly.GetTypes()
                        .Where(t =>
                        {
                            try
                            {
                                if (typeof(IMxlUtility).IsAssignableFrom(t))
                                {
                                    Activator.CreateInstance(t); // try create
                                    return true;
                                }
                            }
                            catch { }
                            return false;
                        })
                        .Select(t => Activator.CreateInstance(t) as IMxlUtility);

                    Console.WriteLine("Usage: gView.Cmd.MxlUtil.exe <utiltiy-name> [parameters]\n");
                    Console.WriteLine("Registered Utitlites:");

                    foreach (var utitilty in utilityInstances)
                    {
                        Console.WriteLine(utitilty.Description());
                        Console.WriteLine(Environment.NewLine);
                    }

                    return 1;
                }

                await utilityInstance.Run(args);

                return 0;
            }
            catch (IncompleteArgumentsException)
            {
                Console.WriteLine(utilityInstance?.HelpText());

                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Exception:");
                Console.WriteLine(ex.Message);
                //Console.WriteLine(ex.StackTrace);

                return 1;
            }
        }


    }
}
