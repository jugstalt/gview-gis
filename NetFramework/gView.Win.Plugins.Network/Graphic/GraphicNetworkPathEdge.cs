using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Carto;
using gView.Framework.Geometry;
using gView.Framework.Symbology;

namespace gView.Plugins.Network.Graphic
{
    class GraphicNetworkPathEdge : IGraphicElement
    {
        private static SimpleLineSymbol _symbol = new SimpleLineSymbol();
        private IPolyline _polyline;

        public GraphicNetworkPathEdge(IPolyline polyline)
        {
            _polyline = polyline;
            _symbol.Color = System.Drawing.Color.Blue;
            _symbol.Width = 5;
        }

        #region IGraphicElement Member

        public void Draw(IDisplay display)
        {
            if (_polyline != null)
                display.Draw(_symbol, _polyline);
        }

        #endregion
    }
}
