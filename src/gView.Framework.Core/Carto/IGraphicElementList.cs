using System;
using System.Collections.Generic;

namespace gView.Framework.Core.Carto
{
    public interface IGraphicElementList : IEnumerable<IGraphicElement>
    {
        void Add(IGraphicElement element);
        void Remove(IGraphicElement element);
        void Remove(Type type);
        void Clear();
        void Insert(int i, IGraphicElement element);
        bool Contains(IGraphicElement element);
        int Count { get; }
        IGraphicElement this[int i] { get; }

        IGraphicElementList Clone();
        IGraphicElementList Swap();
    }
}