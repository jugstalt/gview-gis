using Microsoft.Extensions.Options;

namespace gView.Cmd.Services;
internal class CommandLineArgumentsService
{
    private CommandLineArgumentsServiceOptions _options;

    public CommandLineArgumentsService(IOptions<CommandLineArgumentsServiceOptions> options)
    {
        _options = options.Value;
    }
}
