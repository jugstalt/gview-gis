using System.Threading.Tasks;
using gView.Cmd.Core;
using gView.Cmd.Core.Extensions;
using gView.CopyFeatureclass.Lib;

namespace gView.Cmd.CopyFeatureclass;

class Program
{
    async static Task<int> Main(string[] args)
    {
        var parameters = args.ParseCommandLineArguments();
        var command = new CopyFeatureClassCommand();
        var logger = new ConsoleLogger();

        if (!parameters.VerifyParameters(command.ParameterDescriptions, logger))
        {
            command.LogUsage(logger);
            return 1;
        }

        return 0;
    }
}
