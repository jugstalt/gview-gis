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

    public bool Run()
    {
        return true;
    }
}
