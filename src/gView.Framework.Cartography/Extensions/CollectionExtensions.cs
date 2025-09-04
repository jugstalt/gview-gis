#nullable enable

using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace gView.Framework.Cartography.Extensions;
internal static class CollectionExtensions
{
    private static void AddMessage(this ConcurrentBag<Map.ErrorMessage> messages, ErrorMessageLevel type, string message)
    {
        messages.Add(Map.ErrorMessage.Create(type, message));
    }

    static public void AddErrorMessage(this ConcurrentBag<Map.ErrorMessage> messages, string errorMessage)
    {
        messages.AddMessage(ErrorMessageLevel.Error, errorMessage);
    }

    static public void AddWarningMessage(this ConcurrentBag<Map.ErrorMessage> messages, string warning)
    {
        messages.AddMessage(ErrorMessageLevel.Warning, warning);
    }

    static public bool HasErrorMessages(this ConcurrentBag<Map.ErrorMessage>? messages)
    {
        return messages?
            .Where(m => m.MessageType == ErrorMessageLevel.Error)
            .Any() == true;
    }
}
