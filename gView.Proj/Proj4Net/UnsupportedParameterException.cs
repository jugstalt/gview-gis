using System;

namespace Proj4Net
{
    ///<summary>Signals that a parameter in a CRS specification is not currently supported, or unknown.</summary>
    public class UnsupportedParameterException : Proj4NetException
    {
        public UnsupportedParameterException() { }

        public UnsupportedParameterException(String message)
            : base(message)
        {
        }
    }
}