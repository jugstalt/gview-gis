using gView.Framework.Core.Carto;
using System.Collections.Generic;

namespace gView.Framework.Cartography.Rendering.Abstractions
{
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
}
