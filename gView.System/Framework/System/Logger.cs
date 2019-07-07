using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.system;
using System.IO;

namespace gView.Framework.system
{
    public class Logger
    {
        private static Object thisLocker = new Object();

        public static void Log(loggingMethod method, string text)
        {
            Log(method, String.Empty, text);
        }
        public static void Log(loggingMethod method, string file_postfix, string text)
        {
            string procName = global::System.Diagnostics.Process.GetCurrentProcess().ProcessName.Replace(".", "_");

            string filename = SystemVariables.MyCommonApplicationData + @"/Logfiles/" + procName + @"/";
            switch (method)
            {
                case loggingMethod.request:
                    filename += procName + ((!String.IsNullOrEmpty(file_postfix)) ? "_" + file_postfix : "") + "_Requests.log";
                    break;
                case loggingMethod.request_detail_pro:
                case loggingMethod.request_detail:
                    filename += procName + ((!String.IsNullOrEmpty(file_postfix)) ? "_" + file_postfix : "") + "_Request_Details.log";
                    text += "\nend\n";
                    break;
                case loggingMethod.error:
                    filename += procName + ((!String.IsNullOrEmpty(file_postfix)) ? "_" + file_postfix : "") + "_Errors.log";
                    break;
            }

            Log(filename, text);
        }

        public static void Log(string filename, string text)
        {
            lock (thisLocker)
            {
                try
                {
                    FileInfo fi = new FileInfo(filename);
                    if (fi.Exists)
                    {
                        if (fi.Length > 10 * 1024 * 1024)
                        {
                            try
                            {
                                if (log_archive)
                                {
                                    string aname = fi.DirectoryName + @"/archive_" + DateTime.Now.ToShortDateString().Replace(":", ".") + "_" + DateTime.Now.ToLongTimeString().Replace(":", ".") + "_";
                                    aname = aname + fi.Name;

                                    fi.MoveTo(aname);
                                }
                                else
                                {
                                    fi.Delete();
                                }
                            }
                            catch { }
                        }
                    }
                    else
                    {
                        if (!fi.Directory.Exists) fi.Directory.Create();
                    }

                    using (StreamWriter sw = new StreamWriter(filename, true))
                    {
                        DateTime time = DateTime.Now;
                        string timeStamp = time.ToShortDateString() + " " + time.ToLongTimeString() + " ";
#if(DEBUG)
                        TimeSpan ts = DateTime.Now - _lastDt;
                        _lastDt = DateTime.Now;
                        timeStamp += "(" + ts.TotalMilliseconds + "ms)\t\t";
#endif
                        //if (text.IndexOf(@"\n") != -1) text = text.Replace(@"\n", "\n" + timeStamp);
                        //if (text.IndexOf("><") != -1) text = text.Replace("><", ">\n" + timeStamp + "<");
                        if (text.IndexOf("><") != -1) text = text.Replace("><", ">\n" + "<");

                        sw.WriteLine(timeStamp + text);
                        sw.Close();
                    }
                }
                catch /*(Exception ex)*/
                {
                    // Nur zum Testen, k�nnte Endlosschleife erzeugen...
                    //Logger.Log(loggingMethod.error, ex.Message);
                }
            }
        }

//#if(DEBUG)
        private static DateTime _lastDt = DateTime.Now;
        public static void LogDebug(string text)
        {
            string procName = global::System.Diagnostics.Process.GetCurrentProcess().ProcessName.Replace(".", "_");
            string filename = SystemVariables.MyCommonApplicationData + @"/Logfiles/" + procName + @"/debug.log";

            Log(filename, text);
        }
//#endif

        private static bool log_archive
        {
            get
            {
                return false;
                //if (System.Configuration.ConfigurationSettings.AppSettings["log_archive"] == null) return false;
                //bool result = false;
                //bool.TryParse(System.Configuration.ConfigurationSettings.AppSettings["log_archive"], out result);
                //return result;
            }
        }
    }
}
