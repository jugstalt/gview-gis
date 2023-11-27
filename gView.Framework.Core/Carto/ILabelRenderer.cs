using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.IO;
using gView.Framework.Core.system;

namespace gView.Framework.Core.Carto
{
    public interface ILabelRenderer : IRenderer, IPersistable, IClone, IClone2
    {
        void PrepareQueryFilter(IDisplay display, IFeatureLayer layer, IQueryFilter filter);
        bool CanRender(IFeatureLayer layer, IMap map);
        string Name { get; }

        LabelRenderMode RenderMode { get; }
        int RenderPriority { get; }

        void Draw(IDisplay disp, IFeature feature);
    }
}