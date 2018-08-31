using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Framework.system
{
    public class ExceptionQueue : Exception
    {
        private List<Exception> _exceptions = new List<Exception>();

        public void Add(Exception ex)
        {
            _exceptions.Add(ex);
        }

        public override string Message
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("Exception Queue: " + _exceptions.Count + " Exceptions:\r\n");
                for (int i = 0; i < _exceptions.Count; i++)
                {
                    Exception ex = _exceptions[i];
                    if (ex == null)
                        continue;

                    sb.Append("Exption Type: " + ex.GetType().ToString() + "\r\n");
                    AppendExceptionMessage(ex, sb);

                    while (ex.InnerException != null)
                    {
                        sb.Append(">> Inner Exception:\r\n");
                        sb.Append("Exption Type: " + ex.GetType().ToString() + "\r\n");
                        AppendExceptionMessage(ex, sb);
                        ex = ex.InnerException;
                    }
                }

                return sb.ToString();
            }
        }

        private void AppendExceptionMessage(Exception ex, StringBuilder sb)
        {
            sb.Append("Message: " + ex.Message + "\r\n");
            sb.Append("Source: " + ex.Source + "\r\n");
            sb.Append("Stacktrace: " + ex.StackTrace + "\r\n");
        }
    }
}
