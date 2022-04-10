using gView.Framework.Data;
using gView.Framework.Data.Cursors;

namespace gView.Framework.GeoProcessing
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
