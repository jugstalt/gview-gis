
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