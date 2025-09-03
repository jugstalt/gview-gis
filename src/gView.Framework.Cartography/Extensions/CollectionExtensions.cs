#nullable enable

using System;
using System.Collections.Concurrent;
using System.Linq;

namespace gView.Framework.Cartography.Extensions;
internal static class CollectionExtensions
{
    const string ErrorPrefix = "ERROR:";
    const string WarningPrefix = "WARNING:";

    private static void AddMessage(this ConcurrentBag<string> messages, string prefix, string message)
    {
        messages.Add(message?.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) == true
            ? message
            : $"{prefix} {message}");
    }

    static public void AddErrorMessage(this ConcurrentBag<string> messages, string errorMessage)
    {
        messages.AddMessage(ErrorPrefix, errorMessage);
    }

    static public void AddWarningMessage(this ConcurrentBag<string> messages, string warning)
    {
        messages.AddMessage(WarningPrefix, warning);
    }

    static public bool HasErrorMessages(this ConcurrentBag<string>? messages)
    {
        return messages?
            .Where(m => m.StartsWith(ErrorPrefix, StringComparison.OrdinalIgnoreCase))
            .Any() == true;
    }
}
