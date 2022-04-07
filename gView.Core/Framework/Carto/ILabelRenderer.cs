using gView.Framework.Data;
using gView.Framework.IO;
using gView.Framework.system;

namespace gView.Framework.Carto
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