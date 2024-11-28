using System;
using System.Collections.Generic;

namespace gView.Framework.Core.UI
{
    public class TocLegendItems : IDisposable
    {
        public IEnumerable<TocLegendItem> Items { get; set; }

        public void Dispose()
        {
            if (Items != null)
            {
                foreach (var item in Items)
                {
                    item.Dispose();
                }

                Items = null;
            }
        }
    }
}