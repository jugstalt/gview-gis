using System;

namespace gView.Framework.UI
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
                return base.Message + (base.InnerException != null ? "\n" + base.InnerException.Message : String.Empty);
            }
        }
    }

    
}