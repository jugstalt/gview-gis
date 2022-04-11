using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace gView.SetupHelper
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("Usage: gView.SetupHelper.exe <root-path> <subpath1> <subpath2> ...");
                    return 1;
                }
                string root = args[0];

                var subDirectories = args.Skip(1);

                if (subDirectories.Count() < 2)
                {
                    return 0;
                }

                var sharedDirInfo = new DirectoryInfo($"{ root }/_shared");
                if (sharedDirInfo.Exists)
                {
                    sharedDirInfo.Delete(true);
                }
                sharedDirInfo.Create();

                SearchIdent(root, subDirectories.ToArray(), sharedDirInfo.FullName, String.Empty);

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
        }

        static int counter = 0;
        static public void SearchIdent(string root, string[] subDirectory, string sharedDirectory, string currentPath)
        {
            while (currentPath.StartsWith("/"))
            {
                currentPath = currentPath.Substring(1);
            }

            foreach (var fileInfo in new DirectoryInfo($"{root}/{subDirectory[0]}/{currentPath}").GetFiles("*.*"))
            {
                List<FileInfo> fileInfos = new List<FileInfo>();
                fileInfos.Add(fileInfo);

                for (int i = 1; i < subDirectory.Length; i++)
                {
                    fileInfos.Add(new FileInfo(Path.Combine($"{root}/{subDirectory[i]}", currentPath, fileInfo.Name)));
                }

                //foreach (var fi in fileInfos)
                //{
                //    Console.WriteLine(fi.FullName+" "+fi.Exists);
                //}

                if (fileInfos.Where(f => f.Exists).Count() == fileInfos.Count)
                {
                    if (fileInfos.Where(f => f.Length != fileInfo.Length).Count() == 0)
                    {
                        Console.WriteLine($"{counter++}:{currentPath}/{fileInfo.Name} => _shared/{currentPath}");

                        string targetPath = Path.Combine(sharedDirectory, currentPath, fileInfo.Name);
                        var targetFileInfo = new FileInfo(targetPath);
                        
                        if (!targetFileInfo.Directory.Exists)
                        {
                            targetFileInfo.Directory.Create();
                        }

                        fileInfo.CopyTo(targetPath);

                        foreach (var fi in fileInfos)
                        {
                            fi.Delete();
                        }
                    }
                }
            }

            foreach (var di in new DirectoryInfo($"{ root }/{ subDirectory[0] }/{ currentPath }").GetDirectories())
            {
                SearchIdent(root, subDirectory, sharedDirectory, $"{ currentPath }/{ di.Name }");
            }
        }
    }
}
