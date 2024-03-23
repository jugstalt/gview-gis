using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.IO;
using gView.Framework.Core.Common;

namespace gView.Framework.Core.Carto
{
    public interface ILabelRenderer : IRenderer, IPersistable
    {
        void PrepareQueryFilter(IDisplay display, IFeatureLayer layer, IQueryFilter filter);

        LabelRenderMode RenderMode { get; }
        int RenderPriority { get; }

        void Draw(IDisplay disp, IFeatureLayer layer, IFeature feature);
    }
}