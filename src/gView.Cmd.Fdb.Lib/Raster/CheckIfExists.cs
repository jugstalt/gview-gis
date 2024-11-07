using gView.DataSources.Fdb.ImageDataset;
using gView.DataSources.Fdb.MSAccess;
using gView.Framework.Common;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.FDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Cmd.Fdb.Lib.Raster;
internal class CheckIfExists
{
    private ICancelTracker _cancelTracker;
    public delegate void ReportActionEvent(CheckIfExists sender, string action);
    public event ReportActionEvent? ReportAction;

    public CheckIfExists(ICancelTracker? cancelTracker)
    {
        _cancelTracker = cancelTracker ?? new CancelTracker();
    }

    async public Task<bool> Run(IFeatureDataset ds)
    {
        AccessFDB? fdb = ds.Database as AccessFDB;

        if (fdb == null)
        {
            return false;
        }

        FDBImageDataset import = new FDBImageDataset(ds.Database as IImageDB, ds.DatasetName, _cancelTracker);
        import.ReportAction += (sender, message) =>
        {
            ReportAction?.Invoke(this, message);
        };

        return await import.CheckIfExists();
    }
}
