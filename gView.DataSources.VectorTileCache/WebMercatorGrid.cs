using gView.Framework.Geometry;
using gView.Framework.Geometry.Tiling;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.DataSources.VectorTileCache
{
    class WebMercatorGrid : Grid
    {
        const double WmDpi = 25.4D / 0.28D;  // wmts 0.28mm -> 1 Pixel in WebMercator

        public WebMercatorGrid()
            : base(new Point(-20037508.3427892, 20037508.3427892), 256, 256, WmDpi, GridOrientation.UpperLeft) 
        {
            double scale = 559082264.02871776;

            for (int i = 0, to = 21; i < to; i++)
            {
                base.AddLevel(i, scale / (WmDpi / 0.0254));

                scale /= 2.0;
            }

            this.Extent = new Envelope(-20037508.3427892, -20037508.3427892, 20037508.3427892, 20037508.3427892);
            
        }

        public IEnvelope Extent { get; }
    }
}
