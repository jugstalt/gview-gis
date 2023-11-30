using gView.Framework.Core.system;
using gView.Framework.Core.UI;
using System.Collections.Generic;

namespace gView.Framework.Core.GeoProcessing
{
    public interface IActivity : IErrorMessage, IProgressReporter
    {
        List<IActivityData> Sources { get; }
        List<IActivityData> Targets { get; }
        string DisplayName { get; }
        string CategoryName { get; }

        List<IActivityData> Process();
    }
}
