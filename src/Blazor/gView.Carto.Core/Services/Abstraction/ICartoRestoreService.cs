using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace gView.Carto.Core.Services.Abstraction;

public interface ICartoRestoreService
{
    Task<RestoreResult> SetRestorePoint(ICartoApplicationScopeService appScope);
    IEnumerable<(string filePath, DateTime timeUtc)> GetRestorePoints(string mxlPath, int take = 10);
    RestoreResult RemoveRestorePoints(string mxlPath);
}
