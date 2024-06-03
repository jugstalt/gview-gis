using Microsoft.Extensions.Options;

namespace gView.Cmd.Services;
internal class CommandLineArgumentsService
{
    private readonly Dictionary<string, string> _arguments;

    public CommandLineArgumentsService(IOptions<CommandLineArgumentsServiceOptions> options)
    {
        _arguments = new();

        SetArguments(options.Value.Arguments);
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

    public void SetArguments(string[] arguments)
    {
        _arguments.Clear();

        for (int i = 0; i < arguments.Length; i++)
        {
            if (arguments[i].StartsWith("-"))
            {
                if (i < arguments.Length - 1
                    && arguments[i] != "--help"
                    )
                {
                    _arguments[arguments[i]] = arguments[i + 1];
                    i++;
                }
                else
                {
                    _arguments[arguments[i]] = "";
                }
            }
        }
    }
}
