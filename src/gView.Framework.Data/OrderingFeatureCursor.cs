#nullable enable

using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Framework.Data;
internal class OrderingFeatureCursor : IFeatureCursor
{
    private IFeatureCursor? _cursor;
    private List<IFeature>? _features;
    private Order[] _orderByItems;

    private OrderingFeatureCursor(IFeatureCursor cursor, Order[] orderByItems)
    {
        _cursor = cursor;
        _orderByItems = orderByItems ?? [];
    }

    #region IFeatureCursor

    public void Dispose()
    {
        _cursor?.Dispose();
        _cursor = null;
    }

    private int _position = 0;
    async public Task<IFeature?> NextFeature()
    {
        if (_cursor == null)
        {
            return null;
        }

        if (_features == null)  // collect and sort items
        {
            _features = new List<IFeature>();

            IFeature? feature;
            while ((feature = await _cursor.NextFeature()) != null)
            {
                _features.Add(feature);
            }

            _features.Sort(new OrderFeatures(_orderByItems));
        }

        if (_position >= _features.Count)
        {
            return null;
        }

        return _features[_position++];
    }

    #endregion

    internal static IFeatureCursor Create(IFeatureCursor cursor, Order[] orderByItems)
    {
        return new OrderingFeatureCursor(cursor, orderByItems);
    }

    #region Classes

    private class OrderFeatures : IComparer<IFeature>
    {
        private readonly Order[] _orderByItems;

        public OrderFeatures(Order[] orderByItems)
        {
            _orderByItems = orderByItems;
        }

        public int Compare(IFeature? x, IFeature? y)
        {
            if (x == null)
            {
                return y == null ? 0 : 1;
            }
            if (y == null)
            {
                return -1;
            }

            foreach (var orderByItem in _orderByItems)
            {
                var valX = x[orderByItem.Field];
                var valY = y[orderByItem.Field];

                if (valX is IComparable comparableX && valY is IComparable)
                {
                    var order = comparableX.CompareTo(valY);

                    if (order != 0)
                    {
                        return order * (orderByItem.Descending ? -1 : 1);
                    }
                }
            }

            return 0;
        }
    }

    internal class Order
    {
        public required string Field { get; set; }
        public bool Descending { get; set; }
    }

    #endregion
}
