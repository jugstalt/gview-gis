using Microsoft.Extensions.Options;

namespace gView.Cmd.Services;
internal class CommandLineArgumentsService
{
    private readonly Dictionary<string, string> _arguments;

    public CommandLineArgumentsService(IOptions<CommandLineArgumentsServiceOptions> options)
    {
        _arguments = new();

        for (int i = 0; i < options.Value.Arguments.Length; i++)
        {
            if (options.Value.Arguments[i].StartsWith("-"))
            {
                if (i < options.Value.Arguments.Length - 1 
                    && options.Value.Arguments[i] != "--help"
                    )
                {
                    _arguments[options.Value.Arguments[i]] = options.Value.Arguments[i + 1];
                    i++;
                }
                else
                {
                    _arguments[options.Value.Arguments[i]] = "";
                }
            }
        }
    }

    public string[] Keys => _arguments.Keys.ToArray();

    public string GetValue(string argument)
    {
        if (HasValue(argument))
        {
            return _arguments[argument];
        }

        return String.Empty;
    }

    public bool HasValue(string argument) => _arguments.ContainsKey(argument);
}
