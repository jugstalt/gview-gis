using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.system
{
    public class IndexList<T> : List<T>
    {
        public IndexList() : base()
        {
        }

        new public void Add(T item)
        {
            int index = base.BinarySearch(item);
            if (index >= 0) return;

            base.Insert(~index, item);
        }

        new public bool Contains(T item)
        {
            return (base.BinarySearch(item) >= 0);
        }

        new public int IndexOf(T item)
        {
            int index = base.BinarySearch(item);
            if (index >= 0) return index;

            return -1;
        }
    }
}
