using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.IO;

namespace gView.Framework.Core.Carto
{
    public interface ILabelRenderer : IRenderer, IPersistable
    {
        void PrepareQueryFilter(IDisplay display, IFeatureLayer layer, IQueryFilter filter);

        LabelRenderMode RenderMode { get; }
        RenderLabelPriority RenderPriority { get; }

        void Draw(IDisplay disp, IFeatureLayer layer, IFeature feature);
    }
}