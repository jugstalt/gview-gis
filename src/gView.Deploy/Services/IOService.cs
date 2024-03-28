using gView.Deploy.Extensions;
using gView.Deploy.Models;
using System.IO.Compression;

namespace gView.Deploy.Services;

internal class IOService
{
    public bool CopyIfNotExists(string sourceFile, string targetFile)
    {
        var sourceFileInfo = new FileInfo(sourceFile);
        var targetFileInfo = new FileInfo(targetFile);

        if (!sourceFileInfo.Exists)
        {
            return false;
        }

        if (targetFileInfo.Exists)
        {
            return false;
        }

        if (targetFileInfo.Directory?.Exists == false)
        {
            targetFileInfo.Directory.Create();
        }

        sourceFileInfo.CopyTo(targetFileInfo.FullName);

        return true;
    }

    public bool CopyIfNotExists(ZipArchive zipArchive, string path, string targetFile)
    {
        var targetFileInfo = new FileInfo(targetFile);
        if (targetFileInfo.Exists)
        {
            return false;
        }

        var entry = zipArchive.GetEntry(path);
        if (entry == null)
        {
            return false;
        }

        if (!targetFileInfo.Directory.Exists)
        {
            targetFileInfo.Directory.Create();
        }

        entry.ExtractToFile(targetFile);

        return true;
    }

    public void CopyFolderRecursive(string sourcePath, string targetPath)
    {
        int counter = 0;
        if (!Directory.Exists(targetPath))
        {
            Directory.CreateDirectory(targetPath);
        }

        Directory.CreateDirectory(targetPath);

        foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
        }

        foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
        {
            counter++;

            ClearCurrentConsoleLine();
            var message = $"Copy {newPath}";
            Console.Write(message.Substring(0, Math.Min(message.Length, Console.WindowWidth)));

            File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
        }

        ClearCurrentConsoleLine();
        Console.WriteLine($"...succeeded {counter} items created");
    }

    public void CopyFiles(string sourcePath, string targetPath, string filter = "*.*")
    {
        int counter = 0;

        foreach (var fi in new DirectoryInfo(sourcePath).GetFiles(filter))
        {
            counter++;
            ClearCurrentConsoleLine();
            var message = $"Copy {fi.FullName}";
            Console.Write(message.Substring(0, Math.Min(message.Length, Console.WindowWidth)));

            fi.CopyTo(Path.Combine(targetPath, fi.Name));
        }

        ClearCurrentConsoleLine();
        Console.WriteLine($"...succeeded {counter} items created");
    }

    public void OverrideFolderRecursive(string sourcePath,
                                        string targetPath,
                                        DeployVersionModel versionModel)
    {
        int counter = 0;
        if (!Directory.Exists(targetPath))
        {
            Directory.CreateDirectory(targetPath);
        }

        Directory.CreateDirectory(targetPath);

        foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
        }

        foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
        {
            counter++;

            var message = $"Copy {newPath}";
            Console.WriteLine(message);

            FileInfo soureFileInfo = new FileInfo(newPath);

            if (soureFileInfo.Extension.ToLower() == ".json" || soureFileInfo.Extension.ToLower() == ".config")
            {
                var targetFileInfo = new FileInfo(newPath.Replace(sourcePath, targetPath));
                if (targetFileInfo.Exists)
                {
                    targetFileInfo.Delete();
                }

                var config = versionModel.ReplaceModelProperties(File.ReadAllText(soureFileInfo.FullName));

                File.WriteAllText(targetFileInfo.FullName, config);
            }
            else
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        Console.WriteLine($"...succeeded {counter} items created/overridden");
    }

    public void ExtractZipFolderRecursive(ZipArchive zipArchive, string relativeSourcePath, string targetPath)
    {
        if (!relativeSourcePath.EndsWith("/"))
        {
            relativeSourcePath = $"{relativeSourcePath}/";
        }

        int counter = 0;
        var rootEntry = zipArchive.GetEntry(relativeSourcePath);

        if (rootEntry != null)
        {
            var entries = zipArchive.Entries.Where(e => e.FullName.StartsWith(rootEntry.FullName));
            int entriesLength = entries.Count();

            foreach (var entry in entries)
            {
                counter++;

                if (entry.IsDirectory())
                {
                    //Console.WriteLine($"Got ZipArchiveDirectory: {entry.FullName} - {entry.Name}");
                    DirectoryInfo di = new DirectoryInfo($"{targetPath}/{entry.FullName.Substring(rootEntry.FullName.Length)}");
                    di.Create();
                }
                else
                {
                    FileInfo fi = new FileInfo($"{targetPath}/{entry.FullName.Substring(rootEntry.FullName.Length)}");
                    if (!fi.Directory?.Exists == true)
                    {
                        fi.Directory?.Create();
                    }

                    ClearCurrentConsoleLine();
                    var message = $"{(int)(counter * 100.0 / entriesLength)}% {counter}/{entriesLength} Extract {entry.FullName}";
                    Console.Write(message.Substring(0, Math.Min(message.Length, Console.WindowWidth)));

                    entry.ExtractToFile($"{targetPath}/{entry.FullName.Substring(rootEntry.FullName.Length)}");
                }
            }
        }

        ClearCurrentConsoleLine();
        Console.WriteLine($"...succeeded {counter} items created");
    }

    public void ExtractFiles(ZipArchive zipArchive, string relativeSourcePath, string targetPath)
    {
        int counter = 0;

        if (!relativeSourcePath.EndsWith("/"))
        {
            relativeSourcePath = $"{relativeSourcePath}/";
        }

        var targetDirectory = new DirectoryInfo(targetPath);
        if (!targetDirectory.Exists)
        {
            targetDirectory.Create();
        }

        foreach (var entry in zipArchive.Entries
            .Where(e => e.Length > 0 && e.FullName.StartsWith($"{relativeSourcePath}"))
            .Where(e => e.FullName == $"{relativeSourcePath}{e.Name}"))
        {
            counter++;

            ClearCurrentConsoleLine();
            var message = $"Extract {entry.FullName}";
            Console.Write(message.Substring(0, Math.Min(message.Length, Console.WindowWidth)));

            entry.ExtractToFile(Path.Combine(targetDirectory.FullName, entry.Name));
        }

        ClearCurrentConsoleLine();
        Console.WriteLine($"...succeeded {counter} items created");
    }

    #region Helper

    private void ClearCurrentConsoleLine()
    {
        int currentLineCursor = Console.CursorTop;
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, currentLineCursor);
    }

    #endregion
}
