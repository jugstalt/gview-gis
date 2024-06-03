
using gView.Cmd.Extensions.DependencyInjection;
using gView.Cmd.Services;
using Microsoft.Extensions.DependencyInjection;

var servicesProvider = new ServiceCollection()
    .AddCommandCollection()
    .AddCommandLineArguments(config => { config.Arguments = args; })
    .AddWorker()
    .BuildServiceProvider();

var worker = servicesProvider.GetService<WorkerService>();

// Initialize Skia
gView.GraphicsEngine.Current.Engine = new gView.GraphicsEngine.Skia.SkiaGraphicsEngine(96.0f);
gView.GraphicsEngine.Current.Encoder = new gView.GraphicsEngine.Skia.SkiaBitmapEncoding();

bool interactive = args.Length == 1 && args[0] == "-i";

if (interactive)
{
    var cmdLineArgService = servicesProvider.GetService<CommandLineArgumentsService>();
    cmdLineArgService!.SetArguments([]);

    Console.WriteLine("Interactive mode: type command with arguments");
    await worker!.Run(true);

    Console.WriteLine("Use:");
    Console.WriteLine("quit ... quit program");
    Console.WriteLine("help ... show help");
    Console.WriteLine("clear ... clear console window"); 

    while (true)
    {
        try
        {
            Console.Write("Command:>");

            var cmdLine = /*gView.Cmd.CommandLinePro.ReadLineWithAdvancedAutoComplete();*/ Console.ReadLine();

            if ("quit".Equals(cmdLine, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }
            else if("help".Equals(cmdLine, StringComparison.OrdinalIgnoreCase))
            {
                cmdLineArgService!.SetArguments([]);
                await worker!.Run(true);
                continue;
            }
            else if("clear".Equals(cmdLine, StringComparison.OrdinalIgnoreCase))
            {
                Console.Clear();
                continue;
            }
            else if(string.IsNullOrWhiteSpace(cmdLine))
            {
                continue;
            }

            cmdLineArgService!.SetArguments(["--command", ..cmdLine.Split(' ')]);

            await worker!.Run(true);

            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
    }
}
else
{
    try
    {
        await worker!.Run();

        Console.WriteLine();
        Console.WriteLine("finished");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Exception: {ex.Message}");
    }
}