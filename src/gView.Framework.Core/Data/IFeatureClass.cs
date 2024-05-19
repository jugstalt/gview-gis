using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using System.Threading.Tasks;

namespace gView.Framework.Core.Data
{
    public interface IFeatureClass : ITableClass, IGeometryDef
    {
        string ShapeFieldName { get; }
        IEnvelope Envelope { get; }

        Task<int> CountFeatures();

        //IFeature GetFeature(int id, getFeatureQueryType type);
        //IFeatureCursor GetFeatures(List<int> ids, getFeatureQueryType type);
        Task<IFeatureCursor> GetFeatures(IQueryFilter filter);
    }
}