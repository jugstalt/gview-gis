using gView.Framework.Data;

namespace gView.Interoperability.Server
{
    internal class MapThemeRasterClass : gView.Framework.XML.AXLRasterClass
    {
        public MapThemeRasterClass(IDataset dataset, string id)
            : base(dataset, id)
        {
        }
    }
}