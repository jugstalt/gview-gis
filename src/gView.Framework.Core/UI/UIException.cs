using System;

namespace gView.Framework.Core.UI
{
    public class UIException : Exception
    {
        public UIException(string msg, Exception inner)
            : base(msg, inner)
        {
        }


        public override string Message
        {
            get
            {
                return base.Message + (InnerException != null ? "\n" + InnerException.Message : string.Empty);
            }
        }
    }


}