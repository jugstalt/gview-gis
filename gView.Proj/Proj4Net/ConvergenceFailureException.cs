using System;

namespace Proj4Net
{
    ///<summary>
    /// Signals that an interative mathematical algorithm has failed to converge. This is usually due to values exceeding the allowable bounds for the computation in which they are being used.
    ///</summary>
    public class ConvergenceFailureException : Proj4NetException
    {
        public ConvergenceFailureException() { }

        public ConvergenceFailureException(String message)
            : base(message)
        {
        }
    }
}