using System.Collections.Generic;

namespace gView.Framework.Common
{
    public class IndexList<T> : List<T>
    {
        public IndexList() : base()
        {
        }

        new public void Add(T item)
        {
            int index = BinarySearch(item);
            if (index >= 0)
            {
                return;
            }

            Insert(~index, item);
        }

        new public bool Contains(T item)
        {
            return BinarySearch(item) >= 0;
        }

        new public int IndexOf(T item)
        {
            int index = BinarySearch(item);
            if (index >= 0)
            {
                return index;
            }

            return -1;
        }
    }
}
