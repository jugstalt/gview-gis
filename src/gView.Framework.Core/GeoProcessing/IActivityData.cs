using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;

namespace gView.Framework.Core.GeoProcessing
{
    public interface IActivityData
    {
        IDatasetElement Data { get; set; }
        string DisplayName { get; }
        bool ProcessAble(IDatasetElement data);

        QueryMethod QueryMethod { get; set; }
        string FilterClause { get; set; }

        IFeatureCursor GetFeatures(string appendToClause);
    }
}
