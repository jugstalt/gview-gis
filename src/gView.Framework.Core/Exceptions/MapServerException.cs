using System;

namespace gView.Framework.Core.Exceptions;

public class MapServerException : Exception
{
    public MapServerException(string message)
        : base(message)
    {
        
    }

    public MapServerException(string message, int statusCode)
        : this(message)
    {
        this.StatusCode = statusCode;
    }

    public int? StatusCode = null;
}
