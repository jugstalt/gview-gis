using gView.Framework.Common;
using gView.Server.Extensions;
using System;
using System.IO;

namespace gView.Server.AppCode
{
    public class Setup
    {
        public bool TrySetup(string[] args)
        {
            try
            {
                FileInfo fi = new FileInfo("_config/mapserver.json");
                if (!fi.Exists)
                {
                    fi = new FileInfo("_config/_mapserver.json");
                    if (fi.Exists)
                    {
                        string configContent = String.Empty;

                        Console.WriteLine("#############################################################################################################");
                        Console.WriteLine("Setup:");
                        Console.WriteLine("First start: creating simple _config/mapserver.json. You can modify this file for your production settings...");
                        Console.WriteLine("#############################################################################################################");

                        if (SystemInfo.IsWindows)
                        {
                            configContent = WindowsSetup(fi.FullName, args);
                        }
                        else if (SystemInfo.IsLinux)
                        {
                            configContent = LinuxSetup(fi.FullName, args);
                        }

                        if (!String.IsNullOrEmpty(configContent))
                        {
                            Console.WriteLine(configContent);

                            File.WriteAllText($"{fi.Directory.FullName}/mapserver.json", configContent);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Warning: can't intialize configuration for first start:");
                Console.WriteLine(ex.Message);

                return false;
            }

            return true;
        }

        #region Windows

        private string WindowsSetup(string configTemplateFile, string[] args)
        {
            var fi = new FileInfo(configTemplateFile);

            string host = "http://localhost:5000";
            if (!String.IsNullOrEmpty(args.GetArgumentValue("-expose-https")))
            {
                host = $"https://localhost:{args.GetArgumentValue("-expose-https")}";
            }
            else if (!String.IsNullOrEmpty(args.GetArgumentValue("-expose-http")))
            {
                host = $"http://localhost:{args.GetArgumentValue("-expose-http")}";
            }

            var configText = File.ReadAllText(fi.FullName);

            var parentDirectory = fi.Directory.Parent.Parent;
            if(parentDirectory.Name.Equals(SystemInfo.Version.ToString())) 
            {
                parentDirectory = parentDirectory.Parent;
            }

            configText = configText.Replace("{repository-path}", Path.Combine(parentDirectory.FullName, "gview-repository").Replace("\\", "\\\\"))
                                   .Replace("{server-url}", host);

            return configText;
        }

        #endregion

        #region Linux

        private const string EnvKey_ServerRespositoryPath = "GV_REPOSITORY_PATH";
        private const string EnvKey_ServerOnlineResourceUrl = "GV_ONLINERESOURCE_URL";

        private string LinuxSetup(string configTemplateFile, string[] args)
        {
            string repositoryPath = GetEnvironmentVariable(EnvKey_ServerRespositoryPath) ?? "/etc/gview/gview-repository";
            string onlineResource = GetEnvironmentVariable(EnvKey_ServerOnlineResourceUrl) ?? "http://localhost:5555";

            var fi = new FileInfo(configTemplateFile);

            var configText = File.ReadAllText(fi.FullName);
            configText = configText.Replace("{repository-path}", repositoryPath)
                                   .Replace("{server-url}", onlineResource);

            return configText;
        }

        #endregion

        #region Helper

        private string GetEnvironmentVariable(string name)
        {
            var environmentVariables = Environment.GetEnvironmentVariables();

            if (environmentVariables.Contains(name) && !String.IsNullOrWhiteSpace(environmentVariables[name]?.ToString()))
            {
                return environmentVariables[name]?.ToString();
            }

            return null;
        }

        #endregion
    }
}
