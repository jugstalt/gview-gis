using System;

namespace Proj4Net
{
    ///<summary>
    /// Signals that a situation or data state has been encountered which prevents computation from proceeding,
    /// or which would lead to erroneous results.
    /// This is the base class for all exceptions thrown in the Proj4Net API.
    ///</summary>
    public class Proj4NetException : Exception
    {
        public Proj4NetException()
        {
        }

        public Proj4NetException(String message)
            : base(message)
        {
        }
    }
}