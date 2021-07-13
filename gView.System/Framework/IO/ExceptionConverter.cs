using System;
using System.Text;

namespace gView.Framework.IO
{
    public class ExceptionConverter
    {
        public static string ToString(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Details:\r\n");
            while (ex != null)
            {
                sb.Append("Message:" + ex.Message + "\r\n");
                sb.Append("Source:" + ex.Source + "\r\n");
                sb.Append("Stacktrace:" + ex.StackTrace + "\r\n");

                ex = ex.InnerException;
                if (ex != null)
                {
                    sb.Append("Inner Exception:\r\n");
                }
            }
            return sb.ToString();
        }
    }
}
