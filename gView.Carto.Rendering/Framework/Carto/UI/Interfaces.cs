using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Carto.Rendering;
using gView.Framework.Data;

namespace gView.Framework.Carto.Rendering.UI
{
    public interface IPropertyPanel
    {
        object PropertyPanel(IFeatureRenderer renderer, IFeatureLayer fc);
    }

    public interface IPropertyPanel2
    {
        object PropertyPanel(ILabelRenderer renderer, IFeatureLayer fc);
    }
}

namespace gView.Framework.Carto.Rendering
{
    public interface IScaledependent
    {
        double MinimumScale { get; set; }
        double MaximumScale { get; set; }
    }

    public interface IGroupRenderer
    {
        IRendererGroup Renderers { get; }
    }

    public interface IRendererGroup : IEnumerable<IFeatureRenderer>
    {
        void Add(IFeatureRenderer renderer);
        int IndexOf(IFeatureRenderer renderer);
        bool Remove(IFeatureRenderer renderer);
        void RemoveAt(int index);
        void Insert(int index, IFeatureRenderer renderer);
        int Count { get; }

        IFeatureRenderer this[int index] { get; }
    }

    public interface ILabelGroupRenderer
    {
        ILabelRendererGroup Renderers { get; }
    }

    public interface ILabelRendererGroup : IEnumerable<ILabelRenderer>
    {
        void Add(ILabelRenderer renderer);
        int IndexOf(ILabelRenderer renderer);
        bool Remove(ILabelRenderer renderer);
        void RemoveAt(int index);
        void Insert(int index, ILabelRenderer renderer);
        int Count { get; }

        ILabelRenderer this[int index] { get; }
    }
}
