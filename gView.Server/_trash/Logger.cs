//using gView.Framework.system;
//using gView.MapServer;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace gView.Server.AppCode
//{
//    public class Logger
//    {
//        async static public Task LogAsync(IServiceRequestContext context, loggingMethod loggingMethod, string message)
//        {
//            await LogAsync(ToMapName(context), loggingMethod, message);
//        }

//        async static public Task LogAsync(string mapName, loggingMethod loggingMethod, string message)
//        {
//            try
//            {
//                mapName = mapName?.ToLower() ?? String.Empty;

//                if (!String.IsNullOrWhiteSpace(Globals.LoggingRootPath))
//                {
//                    string fileName = LogFilename(mapName, loggingMethod, true);

//                    await File.AppendAllLinesAsync(fileName,
//                        new string[]
//                        {
//                            DateTime.UtcNow.ToShortDateString() + " " + DateTime.UtcNow.ToLongTimeString() + " (UTC) " +message,
//                            "-"
//                        });
//                }
//                else
//                {
//                    Console.WriteLine(DateTime.UtcNow.ToShortDateString() + " " + DateTime.UtcNow.ToLongTimeString() + " (UTC) " + loggingMethod.ToString() + ": " + message);
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("Logging error: " + ex.Message);
//            }
//        }

//        public static string LogFilename(string mapName, loggingMethod loggingMethod, bool createDir = false)
//        {
//            if (!String.IsNullOrWhiteSpace(Globals.LoggingRootPath))
//            {
//                mapName = mapName?.ToLower() ?? String.Empty;

//                var dir = new DirectoryInfo(Globals.LoggingRootPath + "/" + mapName);
//                if (createDir && !dir.Exists)
//                    dir.Create();

//                var mapService = String.IsNullOrWhiteSpace(mapName) ? null : InternetMapServer.MapServices.Where(s => s.Fullname?.ToLower() == mapName).FirstOrDefault();

//                string fileName = loggingMethod.ToString() +
//                    ((mapService != null && mapService.RunningSinceUtc.HasValue) ? "-" + mapService.RunningSinceUtc.Value.Ticks.ToString().PadLeft(21, '0') : "") +
//                    ".log";

//                return dir + "/" + fileName;
//            }

//            return String.Empty;
//        }

//        public static bool LogFileExists(string mapName, loggingMethod loggingMethod)
//        {
//            string fileName = LogFilename(mapName, loggingMethod);
//            if(!String.IsNullOrEmpty(fileName))
//            {
//                FileInfo fi = new FileInfo(fileName);
//                return fi.Exists;
//            }

//            return false;
//        }

//        public static (IEnumerable<string> errors, long ticks) ErrorLogs(string mapName, loggingMethod loggingMethod, long last = 0)
//        {
//            if(!String.IsNullOrWhiteSpace(Globals.LoggingRootPath))
//            {
//                var dir = new DirectoryInfo(Globals.LoggingRootPath + "/" + mapName);
//                if(dir.Exists)
//                {
//                    var fileNames = dir.GetFiles("*.log").Select(f => f.Name.Substring(0, f.Name.Length - f.Extension.Length)).OrderByDescending(f => f);
//                    foreach(var fileName in fileNames)
//                    {
//                        try
//                        {
//                            var ticks = long.Parse(fileName.Split('-')[1]);
//                            if (last == 0 || last > ticks)
//                            {
//                                List<string> errors = new List<string>();

//                                using (StreamReader fileStream = File.OpenText(dir.FullName + "/" + fileName + ".log"))
//                                {
//                                    string line;
//                                    StringBuilder message = new StringBuilder();
//                                    while ((line = fileStream.ReadLine()) != null)
//                                    {
//                                        if (line.StartsWith("-"))
//                                        {
//                                            errors.Add(message.ToString().Trim());
//                                            message.Clear();
//                                        }
//                                        else
//                                        {
//                                            message.Append(line + "\n");
//                                        }
//                                    }
//                                }

//                                errors.Reverse();
//                                return (errors, ticks);
//                            }
//                        }
//                        catch { }
//                    }
//                }
//            }

//            return (new string[0], 0);
//        }

//        #region Helper

//        private static string ToMapName(IServiceRequestContext context)
//        {
//            string mapName = String.Empty;
//            if (context != null && context.ServiceRequest != null)
//            {
//                if (!String.IsNullOrWhiteSpace(context.ServiceRequest.Folder))
//                    mapName = context.ServiceRequest.Folder + "/" + context.ServiceRequest.Service;
//                else
//                    mapName = context.ServiceRequest.Service;
//            }

//            return mapName;
//        }

//        #endregion
//    }
//}
