using gView.Framework.Core.Exceptions;
using System;
using System.Security.Cryptography;

namespace gView.Framework.Core.Extensions;

/// <summary>
/// What a service client gets to see from an exception.
/// MapServerException (and derived exceptions) carry texts written for clients;
/// every other exception may contain internals (paths, connection strings, SQL,
/// stack traces) and is answered with a generic text plus an error id. The full
/// exception goes to the log with the same id.
/// </summary>
public static class ExceptionExtensions
{
    public const string InternalErrorText = "internal server error";

    /// <summary>
    /// A short random id (8 hex chars) that links a generic client error to the log entry.
    /// </summary>
    public static string NewErrorId()
        => Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).ToLowerInvariant();

    public static string InternalErrorMessage(string errorId)
        => String.IsNullOrEmpty(errorId)
            ? InternalErrorText
            : $"{InternalErrorText} (id: {errorId})";

    extension(Exception exception)
    {
        /// <summary>
        /// True, if the message of this exception is meant to be shown to service clients.
        /// </summary>
        public bool IsClientMessage => exception is MapServerException;

        /// <summary>
        /// The message for the client: the exception message for client exceptions,
        /// otherwise the generic text with the error id.
        /// </summary>
        public string ToClientMessage(string errorId)
            => exception.IsClientMessage
                ? exception.Message
                : InternalErrorMessage(errorId);

        /// <summary>
        /// The log entry for the exception: error id, type, message, inner exceptions and stack trace.
        /// </summary>
        public string ToLogMessage(string errorId)
            => String.IsNullOrEmpty(errorId)
                ? exception.ToString()
                : $"error-id {errorId}: {exception}";
    }
}
