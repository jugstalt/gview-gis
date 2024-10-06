using gView.Blazor.Core.Extensions;
using gView.Carto.Core.Services.Abstraction;
using gView.Framework.Core.Carto;
using gView.Framework.Geometry;
using gView.Framework.IO;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Carto.Core.Services;

public class CartoRestoreService : ICartoRestoreService
{
    private readonly CartoRestoreServiceOptions _options;

    public CartoRestoreService(
                IOptions<CartoRestoreServiceOptions> options
        )
    {
        _options = options.Value;
    }

    async public Task<RestoreResult> SetRestorePoint(ICartoApplicationScopeService appScope, string description)
    {
        try
        {
            if (String.IsNullOrEmpty(appScope.Document?.FilePath)
                || appScope.Document.Map is null)
            {
                return RestoreResult.Failed;
            }

            var document = new CartoDocument(appScope, appScope.Document.Map.Name)
            {
                Map = (IMap)appScope.Document.Map.Clone()
            };

            document.Map.Display.ImageWidth = 1024;
            document.Map.Display.ImageHeight = 800;
            document.Map.Display.ZoomTo(new Envelope(-10, -10, 10, 10));

            XmlStream stream = new XmlStream("MapApplication", performEncryption: true);
            stream.Save("MapDocument", document);

            string mxl = stream.ToXmlString();
            string hash = mxl.GenerateSHA1();
            var filePaths = GetRestoreFilePath(appScope.Document.FilePath, hash);

            var filePath = filePaths.restoreFile;
            var descriptionPath = filePaths.descriptionFile;

            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists)
            {
                fileInfo.CreationTimeUtc = DateTime.UtcNow;
                File.SetLastWriteTimeUtc(filePath, DateTime.UtcNow);

                File.Delete(descriptionPath);
                await File.WriteAllTextAsync(descriptionPath, description);

                return RestoreResult.Unchanged;
            }

            if (!fileInfo.Directory!.Exists) fileInfo.Directory.Create();

            await File.WriteAllTextAsync(filePath, mxl);
            await File.WriteAllTextAsync(descriptionPath, description);

            return RestoreResult.Success;
        }
        catch
        {
            return RestoreResult.Failed;
        }
    }

    public IEnumerable<(string filePath, string description, DateTime timeUtc)> GetRestorePoints(
                string mxlFilePath,
                int take = 10
        )
    {
        var rootDirectory = new DirectoryInfo(_options.RestoreRootPath);

        if (String.IsNullOrEmpty(mxlFilePath) || !rootDirectory.Exists) yield break;

        var fileFilterPatterns = GetRestoreFilePath(mxlFilePath, "*");
        Dictionary<string, string> descriptions = new();

        foreach (var descriptionFile in rootDirectory.GetFiles(new FileInfo(fileFilterPatterns.descriptionFile).Name)
                                                     .OrderByDescending(f => f.LastWriteTimeUtc))
        {
            var key = descriptionFile.Name.AsSpan(0, descriptionFile.Name.Length - descriptionFile.Extension.Length);
            descriptions[key.ToString()] = File.ReadAllText(descriptionFile.FullName);

            if (take> 0 && descriptions.Count() >= take) break;
        }

        int count = 0;
        foreach (var restoreFile in rootDirectory.GetFiles(new FileInfo(fileFilterPatterns.restoreFile).Name)
                                                 .OrderByDescending(f => f.LastWriteTimeUtc))
        {
            var key = restoreFile.Name.AsSpan(0, restoreFile.Name.Length - restoreFile.Extension.Length);
            descriptions.TryGetValue(key.ToString(), out string? description);

            yield return (restoreFile.FullName, description ?? "", restoreFile.CreationTimeUtc);

            if (take > 0 && count++ > take) yield break;
        }
    }

    public RestoreResult RemoveRestorePoints(string mxlPath)
    {
        try
        {
            var rootDirectory = new DirectoryInfo(_options.RestoreRootPath);
            var fileFilterPatterns = GetRestoreFilePath(mxlPath, "*");

            foreach (var descriptionFile in rootDirectory.GetFiles(new FileInfo(fileFilterPatterns.descriptionFile).Name).ToArray()) 
            {
                File.Delete(descriptionFile.FullName);
            }

            foreach (var restoreFile in rootDirectory.GetFiles(new FileInfo(fileFilterPatterns.restoreFile).Name).ToArray())
            {
                File.Delete(restoreFile.FullName);
            }

            return RestoreResult.Success;
        } 
        catch 
        { 
            return RestoreResult.Failed; 
        }
    }

    #region Helper

    private (string restoreFile, string descriptionFile) GetRestoreFilePath(string mxlFilePath, string hash)
    {
        FileInfo fi = new FileInfo(mxlFilePath);
        var fileTitle = fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);

        return 
        (
            System.IO.Path.Combine(_options.RestoreRootPath, $"{fileTitle}-{fi.Directory!.FullName.GenerateSHA1()}-{hash}.restore"),
            System.IO.Path.Combine(_options.RestoreRootPath, $"{fileTitle}-{fi.Directory!.FullName.GenerateSHA1()}-{hash}.txt")
        );
    }

    #endregion
}
