using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Carto;
using System.IO;
using gView.Framework.Symbology;
using System.Drawing;
using gView.Framework.Data;
using System.Xml;
using System.Drawing.Drawing2D;
using gView.Framework.Carto.Rendering;

namespace gView.Interoperability.OGC.SLD
{
    public class StyledLayerDescriptorWriter
    {
        private static IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

        private MemoryStream _ms;
        private StreamWriter _sw;
        private bool _closed = false;
        private string _str = String.Empty;
        private bool _writeDocument = true;

        public StyledLayerDescriptorWriter()
            : this(true)
        {
        }
        public StyledLayerDescriptorWriter(bool WriteDocument)
        {
            _ms = new MemoryStream();
            _sw = new StreamWriter(_ms, Encoding.UTF8);

            _writeDocument = WriteDocument;

            if (_writeDocument)
                WriteBeginDocument();
        }

        public void WriteNamedLayer(string layername, IFeatureRenderer Renderer)
        {
            if (_closed) return;

            if (Renderer is SLDRenderer)
            {
                SLDRenderer sldRenderer = (SLDRenderer)Renderer;
                _sw.WriteLine(sldRenderer.ToXmlString(layername));
            }
            else
            {
                _sw.WriteLine(@"  <NamedLayer>
    <Name>" + layername + @"</Name>
      <UserStyle>
        <Name>" + layername + "_style</Name>");

                _sw.WriteLine(@"<FeatureTypeStyle>
       <Rule>");

                WriteFeatureRenderer(Renderer);

                _sw.WriteLine(@"</Rule>
       </FeatureTypeStyle>");

                _sw.WriteLine(@"</UserStyle>    
  </NamedLayer>");
            }
        }

        private void WriteBeginDocument()
        {
            _sw.WriteLine(@"<StyledLayerDescriptor version=""1.0.0"">");
        }
        private void WriteEndDocument()
        {
            _sw.WriteLine(@"</StyledLayerDescriptor>");
        }

        #region IFeatureRenderer
        private void WriteFeatureRenderer(IFeatureRenderer renderer)
        {
            if (renderer is SimpleRenderer)
            {
                WriteSimpleRenderer((SimpleRenderer)renderer);
            }
            else if (renderer is ValueMapRenderer)
            {
            }
            else if (renderer is UniversalGeometryRenderer)
            {
                WriteUniversalGeometryRenderer((UniversalGeometryRenderer)renderer);
            }
        }

        private void WriteUniversalGeometryRenderer(UniversalGeometryRenderer renderer)
        {
            for (int i = 0; i < renderer.LegendItemCount; i++)
            {
                ILegendItem lItem = renderer.LegendItem(i);

                WriteSymbol(lItem as ISymbol);
            }
        }

        private void WriteSimpleRenderer(SimpleRenderer renderer)
        {
            if (renderer != null && renderer.Symbol != null)
                WriteSymbol(renderer.Symbol);
        }
        #endregion

        #region ISymbology

        public void WriteSymbol(ISymbol symbol)
        {
            if (symbol == null) return;

            if (symbol is IFillSymbol)
                WriteSimpleFillSymbol(symbol as IFillSymbol);
            else if (symbol is ILineSymbol)
                WriteSimpleLineSymbol(symbol as ILineSymbol);
            else if (symbol is IPointSymbol)
                WritePointSymbol(symbol as IPointSymbol);
            else if (symbol is ISymbolCollection)
            {
                ISymbolCollection symCol = (ISymbolCollection)symbol;
                foreach (ISymbolCollectionItem symItem in symCol.Symbols)
                {
                    if (symItem == null || symItem.Visible == false) continue;

                    WriteSymbol(symItem.Symbol);
                }
            }
        }
        private void WriteSimpleFillSymbol(IFillSymbol symbol)
        {
            #region SPEC
            //<PolygonSymbolizer>
            //<Geometry>
            //<ogc:PropertyName>the_area</ogc:PropertyName>
            //</Geometry>
            //<Fill>
            //<CssParameter name="fill">#aaaaff</CssParameter>
            //</Fill>
            //<Stroke>
            //<CssParameter name="stroke">#0000aa</CssParameter>
            //</Stroke>
            //</PolygonSymbolizer>
            #endregion

            if (symbol == null) return;

            _sw.WriteLine("<PolygonSymbolizer>");
            _sw.WriteLine("<Geometry>");
            _sw.WriteLine("</Geometry>");
            if (symbol is IBrushColor &&
                ((IBrushColor)symbol).FillColor.A != 0)
            {
                _sw.WriteLine("<Fill>");
                WriteBrushColor(symbol as IBrushColor);
                _sw.WriteLine("</Fill>");
            }
            if (symbol is IPenColor ||
                symbol is IPenWidth ||
                symbol is IPenDashStyle)
            {
                _sw.WriteLine("<Stroke>");
                WritePenColor(symbol as IPenColor);
                WritePenWidth(symbol as IPenWidth);
                if (symbol is SimpleFillSymbol &&
                    ((SimpleFillSymbol)symbol).OutlineSymbol is SimpleLineSymbol)
                {
                    WriteLineCap(((SimpleLineSymbol)((SimpleFillSymbol)symbol).OutlineSymbol).LineStartCap);
                }
                _sw.WriteLine("</Stroke>");
            }
            _sw.WriteLine("</PolygonSymbolizer>");
        }
        private void WriteSimpleLineSymbol(ILineSymbol symbol)
        {
            #region SPEC
            //<LineSymbolizer>
            //<Geometry>
            //<ogc:PropertyName>centerline</ogc:PropertyName>
            //</Geometry>
            //<Stroke>
            //<CssParameter name="stroke">#0000ff</CssParameter>
            //<CssParameter name="stroke-width">2</CssParameter>
            //</Stroke>
            //</LineSymbolizer>
            #endregion

            if (symbol == null) return;

            _sw.WriteLine("<LineSymbolizer>");
            _sw.WriteLine("<Geometry>");
            _sw.WriteLine("</Geometry>");

            _sw.WriteLine("<Stroke>");
            WritePenColor(symbol as IPenColor);
            WritePenWidth(symbol as IPenWidth);
            WriteDashStyle(symbol as IPenDashStyle);
            if (symbol is SimpleLineSymbol)
            {
                WriteLineCap(((SimpleLineSymbol)symbol).LineStartCap);
            }
            _sw.WriteLine("</Stroke>");

            _sw.WriteLine("</LineSymbolizer>");
        }
        private void WritePointSymbol(IPointSymbol symbol)
        {
            #region SPEC
            //<PointSymbolizer>
            //<Geometry>
            //<ogc:PropertyName>locatedAt</ogc:PropertyName>
            //</Geometry>
            //<Graphic>
            //<Mark>
            //<WellKnownName>star</WellKnownName>
            //<Fill>
            //<CssParameter name="fill">#ff0000</CssParameter>
            //</Fill>
            //</Mark>
            //<Size>8.0</Size>
            //</Graphic>
            //</PointSymbolizer>
            #endregion

            if (symbol == null) return;

            _sw.WriteLine("<PointSymbolizer>");
            _sw.WriteLine("<Geometry>");
            _sw.WriteLine("</Geometry>");

            _sw.WriteLine("<Graphic>");

            if (symbol is SimplePointSymbol)
            {
                _sw.WriteLine("<Mark>");
                _sw.Write("<WellKnownName>");
                _sw.Write(((SimplePointSymbol)symbol).Marker.ToString().ToLower());
                _sw.WriteLine("</WellKnownName>");

                _sw.WriteLine("<Fill>");
                WriteBrushColor(symbol as IBrushColor);
                _sw.WriteLine("</Fill>");

                _sw.WriteLine("<Stroke>");
                WritePenColor(symbol as IPenColor);
                WritePenWidth(symbol as IPenWidth);
                _sw.WriteLine("</Stroke>");

                _sw.WriteLine("</Mark>");
                _sw.Write("<Size>");
                _sw.Write(((SimplePointSymbol)symbol).Size.ToString(_nhi));
                _sw.WriteLine("</Size>");
            }

            _sw.WriteLine("</Graphic>");
            _sw.WriteLine("</PointSymbolizer>");
        }

        private void WriteBrushColor(IBrushColor symbol)
        {
            if (symbol == null) return;
            WriteFillColor(symbol.FillColor);
        }
        private void WriteFillColor(Color col)
        {
            _sw.WriteLine(@"<CssParameter name=""fill"">" + ColorTranslator.ToHtml(col) + "</CssParameter>");
            if (col.A != 255)
            {
                _sw.WriteLine(@"<CssParameter name=""fill-opacity"">" + ((float)(col.A / 255f)).ToString(_nhi) + "</CssParameter>");
            }
        }
        private void WritePenColor(IPenColor symbol)
        {
            if (symbol == null) return;
            _sw.WriteLine(@"<CssParameter name=""stroke"">" + ColorTranslator.ToHtml(symbol.PenColor) + "</CssParameter>");
            if (symbol.PenColor.A != 255)
            {
                _sw.WriteLine(@"<CssParameter name=""stroke-opacity"">" + ((float)(symbol.PenColor.A / 255f)).ToString(_nhi) + "</CssParameter>");
            }
        }
        private void WritePenWidth(IPenWidth symbol)
        {
            if (symbol == null) return;
            _sw.WriteLine(@"<CssParameter name=""stroke-width"">" + symbol.PenWidth.ToString(_nhi) + "</CssParameter>");
        }
        private void WriteLineCap(LineCap style)
        {
            switch (style)
            {
                case LineCap.Flat:
                    _sw.WriteLine(@"<CssParameter name=""stroke-linecap"">butt</CssParameter>");
                    break;
                case LineCap.Round:
                    _sw.WriteLine(@"<CssParameter name=""stroke-linecap"">round</CssParameter>");
                    break;
                case LineCap.Square:
                    _sw.WriteLine(@"<CssParameter name=""stroke-linecap"">square</CssParameter>");
                    break;
            }
        }
        private void WriteDashStyle(IPenDashStyle symbol)
        {
            if (symbol == null) return;

            string sequence = String.Empty;
            switch (symbol.PenDashStyle)
            {
                case DashStyle.Dot:
                    sequence = "2,2,2";
                    break;
                case DashStyle.Dash:
                    sequence = "5,5,5";
                    break;
                case DashStyle.DashDot:
                    sequence = "4,3,1,3,5";
                    break;
                case DashStyle.DashDotDot:
                    sequence = "4,3,1,2,1,3,4";
                    break;
                default:
                    return;
            }
            _sw.WriteLine(@"<CssParameter name=""stroke-dasharray"">" + sequence + "</CssParameter>");
        }
        #endregion

        public override string ToString()
        {
            if (!_closed)
            {
                if (_writeDocument)
                    WriteEndDocument();

                _sw.Flush();
                _ms.Position = 0;
                byte[] bytes = new byte[_ms.Length];
                _ms.Read(bytes, 0, (int)_ms.Length);
                _sw.Close();

                _str = Encoding.UTF8.GetString(bytes).Trim();
            }
            _closed = true;
            return _str;
        }
        public string ToLineString()
        {
            StringBuilder sb = new StringBuilder();
            StringReader sr = new StringReader(this.ToString());
            string line;
            while ((line = sr.ReadLine()) != null)
                sb.Append(line.Trim());

            return sb.ToString();
        }
    }

    public class StyledLayerDescriptorReader
    {
        private static IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

        XmlDocument _doc = null;
        XmlNamespaceManager _ns = null;
        public StyledLayerDescriptorReader(XmlNamespaceManager ns)
        {
            _ns = ns;
        }
        public List<IFeatureLayer> ReadNamedLayers(string sld)
        {
            List<IFeatureLayer> fLayers = new List<IFeatureLayer>();
            try
            {
                _doc = new XmlDocument();
                _doc.LoadXml(sld);

                foreach (XmlNode node in _doc.SelectNodes("StyledLayerDescriptor/NamedLayer"))
                {
                    IFeatureLayer fLayer = ReadLayer(node);
                    if (fLayer == null) continue;

                    fLayers.Add(fLayer);
                }
            }
            catch
            {
            }
            return fLayers;
        }

        private IFeatureLayer ReadLayer(XmlNode node)
        {
            XmlNode nameNode = node.SelectSingleNode("Name");
            if (nameNode == null) return null;
            XmlNode userStyleNode = node.SelectSingleNode("UserStyle");
            if (userStyleNode == null) return null;

            FeatureLayer fLayer = new FeatureLayer();
            fLayer.Title = nameNode.InnerText;

            foreach (XmlNode rule in userStyleNode.SelectNodes("Rule"))
            {
                XmlNode symbolizer = userStyleNode.SelectSingleNode("PolygonSymbolizer");
                if (symbolizer != null)
                {
                }
                symbolizer = userStyleNode.SelectSingleNode("LineSymbolizer");
                if (symbolizer != null)
                {
                }
                symbolizer = userStyleNode.SelectSingleNode("PointSymbolizer");
                if (symbolizer != null)
                {
                }
            }

            return fLayer;
        }

        #region Symbology
        public ISymbol ReadSymbol(XmlNode ruleNode)
        {
            if (ruleNode == null) return null;

            SymbolCollection symColl = new SymbolCollection();
            foreach (XmlNode child in ruleNode.ChildNodes)
            {
                switch (child.Name)
                {
                    case "PolygonSymbolizer":
                        symColl.AddSymbol(ReadPolygonSymbol(child));
                        break;
                    case "LineSymbolizer":
                        symColl.AddSymbol(ReadLineSymbol(child));
                        break;
                    case "PointSymbolizer":
                        symColl.AddSymbol(ReadPointSymbol(child));
                        break;
                }
            }

            if (symColl.Symbols.Count == 0)
                return null;
            else if (symColl.Symbols.Count == 1)
                return symColl.Symbols[0].Symbol;

            return symColl;
        }

        private ISymbol ReadPolygonSymbol(XmlNode node)
        {
            if (node == null) return null;

            SimpleFillSymbol symbol = new SimpleFillSymbol();
            XmlNode fillNode = node.SelectSingleNode("Fill");
            if (fillNode != null)
            {
                symbol.Color = ReadColor(
                    fillNode.SelectSingleNode("CssParameter[@name='fill']"),
                    fillNode.SelectSingleNode("CssParameter[@name='fill-opacity']"));
            }
            else
            {
                symbol.Color = Color.Transparent;
            }

            XmlNode strokeNode = node.SelectSingleNode("Stroke");
            if (strokeNode != null)
            {
                symbol.OutlineSymbol = ReadLineSymbol(node);
            }
            else
            {
                symbol.OutlineSymbol = null;
            }

            return symbol;
        }
        private ISymbol ReadLineSymbol(XmlNode node)
        {
            if (node == null) return null;

            SimpleLineSymbol symbol = new SimpleLineSymbol();
            XmlNode strokeNode = node.SelectSingleNode("Stroke");
            if (strokeNode != null)
            {
                symbol.Color = ReadColor(
                    strokeNode.SelectSingleNode("CssParameter[@name='stroke']"),
                    strokeNode.SelectSingleNode("CssParameter[@name='stroke-opacity']"));
                symbol.Width = ReadFloat(strokeNode.SelectSingleNode("CssParameter[@name='stroke-width']"));

                switch (ReadString(strokeNode.SelectSingleNode("CssParameter[@name='stroke-linecap']")))
                {
                    case "butt":
                        symbol.LineStartCap = symbol.LineEndCap = LineCap.Flat;
                        break;
                    case "square":
                        symbol.LineStartCap = symbol.LineEndCap = LineCap.Square;
                        break;
                    case "round":
                        symbol.LineStartCap = symbol.LineEndCap = LineCap.Round;
                        break;
                }
            }

            return symbol;
        }
        private ISymbol ReadPointSymbol(XmlNode node)
        {
            if (node == null) return null;

            SimplePointSymbol symbol = new SimplePointSymbol();
            symbol.Color = ReadColor(node.SelectSingleNode("Graphic/Mark/Fill/CssParameter[@name='fill']"));
            symbol.OutlineColor = ReadColor(
                node.SelectSingleNode("Graphic/Mark/Stroke/CssParameter[@name='stroke']"),
                node.SelectSingleNode("Graphic/Mark/Stroke/CssParameter[@name='stroke-opacity']"));
            symbol.OutlineWidth = ReadFloat(node.SelectSingleNode("Graphic/Mark/Stroke/CssParameter[@name='stroke-width']"));

            switch (ReadString(node.SelectSingleNode("Graphic/Mark/WellKnownName")).ToLower())
            {
                case "circle":
                    symbol.Marker = SimplePointSymbol.MarkerType.Circle;
                    break;
                case "cross":
                    symbol.Marker = SimplePointSymbol.MarkerType.Cross;
                    break;
                case "triangle":
                    symbol.Marker = SimplePointSymbol.MarkerType.Triangle;
                    break;
                case "square":
                    symbol.Marker = SimplePointSymbol.MarkerType.Square;
                    break;
                case "star":
                    symbol.Marker = SimplePointSymbol.MarkerType.Star;
                    break;
            }

            symbol.Size = ReadFloat(node.SelectSingleNode("Graphic/Size"));

            return symbol;
        }
        private Color ReadColor(XmlNode cssParameter)
        {
            return ReadColor(cssParameter, null);
        }
        private Color ReadColor(XmlNode cssParameter, XmlNode cssParameter2)
        {
            if (cssParameter == null) return Color.Transparent;
            try
            {
                Color col = ColorTranslator.FromHtml(cssParameter.InnerText);
                if (cssParameter2 != null)
                {
                    int alpha = (int)(ReadFloat(cssParameter2) * 255f);
                    col = Color.FromArgb(alpha, col);
                }
                return col;
            }
            catch { return Color.Transparent; }
        }
        private float ReadFloat(XmlNode cssParameter)
        {
            if (cssParameter == null) return 0f;
            try
            {
                float res;
                float.TryParse(cssParameter.InnerText, System.Globalization.NumberStyles.Any, _nhi, out res);
                return res;
            }
            catch { return 0f; }
        }
        private string ReadString(XmlNode cssParameter)
        {
            if (cssParameter == null) return String.Empty;
            return cssParameter.InnerText;
        }
        private DashStyle ReadDashStyle(XmlNode cssParameter)
        {
            if (cssParameter == null) return DashStyle.Solid;

            return DashStyle.Solid;
        }
        #endregion
    }
}
