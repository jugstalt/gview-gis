using System;

namespace gView.Framework.Core.Exceptions;

public class MapServerException : Exception
{
    public MapServerException(string messsage)
        : base(messsage)
    {

    }
}
