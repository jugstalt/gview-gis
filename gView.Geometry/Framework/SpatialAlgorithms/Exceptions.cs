using System;

namespace gView.Framework.SpatialAlgorithms
{
    public class InvalidGeometryException : Exception
    {
        public InvalidGeometryException(string message) : base(message)
        {

        }
    }
}
