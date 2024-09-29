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

    async public Task<RestoreResult> SetRestorePoint(ICartoApplicationScopeService appScope)
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
            string filePath = GetRestoreFilePath(appScope.Document.FilePath, hash);

            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists)
            {
                fileInfo.CreationTimeUtc = DateTime.UtcNow;
                File.SetLastWriteTimeUtc(filePath, DateTime.UtcNow);

                return RestoreResult.Unchanged;
            }

            if (!fileInfo.Directory!.Exists) fileInfo.Directory.Create();

            await File.WriteAllTextAsync(filePath, mxl);

            return RestoreResult.Success;
        }
        catch
        {
            return RestoreResult.Failed;
        }
    }

    public IEnumerable<(string filePath, DateTime timeUtc)> GetRestorePoints(
                string mxlFilePath,
                int take = 10
        )
    {
        var rootDirectory = new DirectoryInfo(_options.RestoreRootPath);

        if (String.IsNullOrEmpty(mxlFilePath) || !rootDirectory.Exists) yield break;

        int count = 0;
        foreach (var restoreFile in rootDirectory.GetFiles(new FileInfo(GetRestoreFilePath(mxlFilePath, "*")).Name)
                                                 .OrderByDescending(f => f.LastWriteTimeUtc))
        {
            yield return (restoreFile.FullName, restoreFile.CreationTimeUtc);

            if (take > 0 && count++ > take) yield break;
        }
    }

    public RestoreResult RemoveRestorePoints(string mxlPath)
    {
        try
        {
            foreach (var restorePoint in GetRestorePoints(mxlPath, 0))
            {
                File.Delete(restorePoint.filePath);
            }

            return RestoreResult.Success;
        } 
        catch 
        { 
            return RestoreResult.Failed; 
        }
    }

    #region Helper

    private string GetRestoreFilePath(string mxlFilePath, string hash)
    {
        FileInfo fi = new FileInfo(mxlFilePath);
        var fileTitle = fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);

        return System.IO.Path.Combine(_options.RestoreRootPath, $"{fileTitle}-{fi.Directory!.FullName.GenerateSHA1()}-{hash}.restore");
    }

    #endregion
}
