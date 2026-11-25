using gView.Framework.Core.Exceptions;

namespace gView.Interoperability.Extensions.Cql2;

/// <summary>
/// An invalid CQL2 filter: a client error (400). The APIs turn it into their own error format.
/// </summary>
public class CqlException : MapServerException
{
    public CqlException(string message)
        : base(message, 400)
    {
    }
}
