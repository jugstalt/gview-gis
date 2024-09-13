using gView.Framework.Core.Carto;
using gView.Framework.Core.IO;

namespace gView.Framework.Cartography
{
    public class MapServiceProperties : IMapServiceProperties
    {
        public int MaxImageWidth { get; set; }
        
        public int MaxImageHeight { get; set; }

        public int MaxRecordCount { get; set; }

        public IMapServiceProperties Clone()
        {
            return new MapServiceProperties()
            {
                MaxImageWidth = MaxImageWidth,
                MaxImageHeight = MaxImageHeight,
                MaxRecordCount = MaxRecordCount
            };
        }

        public void Load(IPersistStream stream)
        {
            MaxImageWidth = (int)stream.Load("MapService_MaxImageWidth", (int)0);
            MaxImageHeight = (int)stream.Load("MapService_MaxImageHeight", (int)0);
            MaxRecordCount = (int)stream.Load("MapService_MaxRecordCount", (int)0);
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("MapService_MaxImageWidth", MaxImageWidth);
            stream.Save("MapService_MaxImageHeight", MaxImageHeight);
            stream.Save("MapService_MaxRecordCount", MaxRecordCount);
        }
    }
}
