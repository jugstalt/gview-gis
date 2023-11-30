using gView.Framework.Core.Carto;
using System.Collections.Generic;

namespace gView.Framework.Cartography.Rendering.Abstractions
{
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
