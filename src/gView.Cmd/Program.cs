
using gView.Cmd.Extensions.DependencyInjection;
using gView.Cmd.Services;
using Microsoft.Extensions.DependencyInjection;

var servicesProvider = new ServiceCollection()
    .AddCommandCollection()
    .AddCommandLineArguments(config => { config.Arguments = args; })
    .AddWorker()
    .BuildServiceProvider();

var worker = servicesProvider.GetService<WorkerService>();

worker!.Run();

Console.WriteLine();
Console.WriteLine("finished");