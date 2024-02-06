using gView.Cmd.Core;
using gView.Cmd.Core.Extensions;
using gView.Framework.Common;

namespace gView.Cmd.Services;
internal class WorkerService
{
    private readonly CommandCollectionService _commandCollection;
    private readonly CommandLineArgumentsService _arguments;

    public WorkerService(CommandCollectionService commandCollection,
                         CommandLineArgumentsService arguments)
    {
        _commandCollection = commandCollection;
        _arguments = arguments;
    }

    public Task<bool> Run()
    {
        PlugInManager.InitSilent = true;

        if (!_arguments.HasValue("--command"))
        {
            Console.WriteLine("Usage: gView.Cmd --command [command] [...arguments...]");
            Console.WriteLine("       gView.Cmd --command [command] --help");
            Console.WriteLine();

            Console.WriteLine("Use one of the following commands:");
            foreach (var instance in _commandCollection.Instances)
            {
                Console.WriteLine($"{instance.Name}:\t\t{instance.Description}");
            }

            return Task.FromResult(true);
        }

        var command = _commandCollection.CommandByName(_arguments.GetValue("--command"));
        if (command is null)
        {
            throw new Exception($"Unknown command :{_arguments.GetValue("--command")}");
        }

        var logger = new ConsoleLogger();

        if(_arguments.HasValue("--help"))
        {
            Console.WriteLine($"Help: {command.Name}");
            Console.WriteLine(command.Description);
            command.LogUsage(logger);

            return Task.FromResult(true);
        }

        var commandArguments = new Dictionary<string, object>();
        foreach(var argKey in _arguments.Keys)
        {
            if(argKey.StartsWith("--"))
            {
                continue;
            }
            if(argKey.StartsWith("-"))
            {
                commandArguments[argKey.Substring(1)] = _arguments.GetValue(argKey);
            }
        }

        return command.Run(commandArguments, logger: logger);
    }
}
