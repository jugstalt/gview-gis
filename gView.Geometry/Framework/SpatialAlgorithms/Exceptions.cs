using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.SpatialAlgorithms
{
    public class InvalidGeometryException : Exception 
    {
        public InvalidGeometryException(string message) : base(message)
        {

        }
    }
}
