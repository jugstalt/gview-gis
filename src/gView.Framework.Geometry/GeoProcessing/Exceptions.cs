using System;

namespace gView.Framework.Geometry.GeoProcessing
{
    public class InvalidGeometryException : Exception
    {
        public InvalidGeometryException(string message) : base(message)
        {

        }
    }
}
