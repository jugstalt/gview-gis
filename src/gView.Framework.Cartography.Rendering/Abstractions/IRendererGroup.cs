using gView.Framework.Core.Carto;
using System.Collections.Generic;

namespace gView.Framework.Cartography.Rendering.Abstractions
{
    public interface IRendererGroup : IEnumerable<IRendererGroupItem>
    {
        void Add(IRendererGroupItem rendererItem);
        int IndexOf(IRendererGroupItem rendererItem);
        bool Remove(IRendererGroupItem rendererItem);
        void RemoveAt(int index);
        void Insert(int index, IRendererGroupItem rendererItem);
        int Count { get; }

        void Clear();

        IRendererGroupItem Create(IRenderer renderer);

        IRendererGroupItem this[int index] { get; }
    }
}
