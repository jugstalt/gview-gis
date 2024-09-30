using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace gView.Carto.Core.Services.Abstraction;

public interface ICartoRestoreService
{
    Task<RestoreResult> SetRestorePoint(ICartoApplicationScopeService appScope, string descritpion);
    IEnumerable<(string filePath, string description, DateTime timeUtc)> GetRestorePoints(string mxlPath, int take = 10);
    RestoreResult RemoveRestorePoints(string mxlPath);
}
