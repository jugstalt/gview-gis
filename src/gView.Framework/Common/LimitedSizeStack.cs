#nullable enable

using System.Collections.Generic;

namespace gView.Framework.Common
{
    public class LimitedSizeStack<T>
    {
        private readonly LinkedList<T> _list = new LinkedList<T>();
        private readonly int _maxSize;
        private readonly object _lock = new object();

        public LimitedSizeStack(int maxSize)
        {
            _maxSize = maxSize;
        }

        public void Push(T item)
        {
            lock (_lock)
            {
                if (_list.Count == _maxSize)
                {
                    _list.RemoveLast();
                }

                _list.AddFirst(item);
            }
        }

        public T? Pop()
        {
            lock (_lock)
            {
                if (_list.Count == 0) return default(T);

                var value = _list.First!.Value;
                _list.RemoveFirst();

                return value ?? default(T?);
            }
        }

        public bool TryPop(out T? item)
        {
            lock (_lock)
            {
                if (_list.Count == 0)
                {
                    item = default(T?);
                    return false;
                }

                item = _list.First!.Value;
                _list.RemoveFirst();

                return true;
            }
        }

        public T? Peek()
        {
            lock (_lock)
            {
                if (_list.Count == 0) return default(T);

                return _list.First!.Value;
            }
        }

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _list.Count;
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _list.Clear();
            }
        }
    }
}
