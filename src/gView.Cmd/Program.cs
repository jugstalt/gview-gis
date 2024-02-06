
using gView.Cmd.Extensions.DependencyInjection;
using gView.Cmd.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;

var servicesProvider = new ServiceCollection()
    .AddCommandCollection()
    .AddCommandLineArguments(config => { config.Arguments = args; })
    .AddWorker()
    .BuildServiceProvider();

var worker = servicesProvider.GetService<WorkerService>();

try
{
    await worker!.Run();

    Console.WriteLine();
    Console.WriteLine("finished");
}
catch(Exception ex)
{
    Console.WriteLine($"Exception: {ex.Message}");
}