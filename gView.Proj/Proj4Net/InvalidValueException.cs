using System;

namespace Proj4Net
{
    ///<summary>
    /// Signals that a parameter or computed internal variable has a value which lies outside the  allowable bounds for the computation in which it is being used.
    ///</summary>
    public class InvalidValueException : Proj4NetException
    {
        public InvalidValueException() { }

        public InvalidValueException(String message)
            : base(message)
        {
        }
    }
}