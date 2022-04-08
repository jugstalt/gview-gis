using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace gView.Server.Extensions
{
    static public class IoExtensions
    {
        static public bool TryCreateDirectoryIfNotExistes(this IConfiguration configuration, string configKey)
        {
            var directoryPath = configuration[configKey];

            if (!String.IsNullOrEmpty(directoryPath))
            {
                try
                {
                    var di = new DirectoryInfo(directoryPath);
                    if (!di.Exists)
                    {
                        di.Create();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warnung: Can't create directory { directoryPath }");
                    Console.WriteLine(ex.Message);

                    return false;
                }
            }

            return true;
        }

        static public bool TryDeleteFilesOlderThan(this string directoryPath, DateTime dtUtc)
        {
            if (!String.IsNullOrEmpty(directoryPath))
            {
                try
                {
                    var di = new DirectoryInfo(directoryPath);
                    if (di.Exists)
                    {
                        var fileInfos = di.GetFiles();
                        foreach (var fileInfo in fileInfos)
                        {
                            if (fileInfo.CreationTimeUtc < dtUtc)
                            {
                                fileInfo.FullName.TryDeleteFile();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warnung: Can't delete files form directory { directoryPath }");
                    Console.WriteLine(ex.Message);

                    return false;
                }
            }

            return true;
        }

        static public bool TryDeleteFile(this string filePath)
        {
            try
            {
                var fi = new FileInfo(filePath);
                if (fi.Exists)
                {
                    fi.Delete();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warnung: Can't delete file { filePath }");
                Console.WriteLine(ex.Message);

                return false;
            }

            return true;
        }
    }
}
