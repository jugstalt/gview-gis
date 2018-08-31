using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Framework.system;
using gView.Framework.UI;

namespace gView.Framework.GeoProcessing
{
    public enum QueryMethod
    {
        All = 0,
        Selected = 1,
        Filter = 2
    };

    public interface IActivityData
    {
        IDatasetElement Data { get; set; }
        string DisplayName { get; }
        bool ProcessAble(IDatasetElement data);

        QueryMethod QueryMethod { get; set; }
        string FilterClause { get; set; }

        IFeatureCursor GetFeatures(string appendToClause);
    }

    public interface IActivity : IErrorMessage ,IProgressReporter
    {
        List<IActivityData> Sources { get; }
        List<IActivityData> Targets { get; }
        string DisplayName { get; }
        string CategoryName { get; }

        List<IActivityData> Process();
    }
}
