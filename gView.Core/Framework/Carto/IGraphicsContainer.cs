using System;

namespace gView.Framework.Carto
{
    public interface IGraphicsContainer
    {
        event EventHandler SelectionChanged;

        IGraphicElementList Elements { get; }
        IGraphicElementList SelectedElements { get; }
        GrabberMode EditMode { get; set; }
    }
}