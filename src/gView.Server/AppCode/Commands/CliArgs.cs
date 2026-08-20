using System;

namespace gView.Server.AppCode.Commands;

/// <summary>
/// Small shared "--flag value" parser used by the offline CLI commands
/// (PublishCommand, RemoveCommand, CatalogCommand, GetMetadataCommand,
/// SetMetadataCommand).
/// </summary>
static internal class CliArgs
{
    static public string GetValue(string[] args, string flag)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (String.Equals(args[i], flag, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }

        return String.Empty;
    }

    static public bool HasFlag(string[] args, string flag)
    {
        foreach (var arg in args)
        {
            if (String.Equals(arg, flag, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
