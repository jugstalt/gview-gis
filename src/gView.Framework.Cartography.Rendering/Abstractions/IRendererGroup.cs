using gView.Framework.Core.Carto;
using System.Collections.Generic;

namespace gView.Framework.Cartography.Rendering.Abstractions
{
    public interface IRendererGroup : IEnumerable<IRendererGroupItem>
    {
        void Add(IRendererGroupItem renderer);
        int IndexOf(IRendererGroupItem renderer);
        bool Remove(IRendererGroupItem renderer);
        void RemoveAt(int index);
        void Insert(int index, IRendererGroupItem renderer);
        int Count { get; }

        IRendererGroupItem Create(IRenderer renderer);

        IRendererGroupItem this[int index] { get; }
    }
}
