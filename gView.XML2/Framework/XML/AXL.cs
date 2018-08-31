using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using gView.Framework.Carto;
using gView.Framework.Symbology;
using gView.Framework.Data;
using gView.Framework.Geometry;
using System.IO;
using gView.Framework.Xml;
using System.Globalization;
using gView.Framework.Carto.Rendering;

namespace gView.Framework.XML
{
    public class ObjectFromAXLFactory
    {
        private static IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

        public static IQueryFilter Query(XmlNode query, ITableClass tc)
        {
            return Query(query, tc, null);
        }
        public static IQueryFilter Query(XmlNode query, ITableClass tc, IFeatureClass rootFeateatureClass)
        {
            if (tc == null || query == null) return null;

            IQueryFilter filter = (query.Name == "QUERY") ? new QueryFilter() : new SpatialFilter();
            if (query.Attributes["where"] != null)
                filter.WhereClause = query.Attributes["where"].Value;
            if (query.Attributes["subfields"] != null)
            {
                foreach (string field in query.Attributes["subfields"].Value.Split(' '))
                {
                    if (field == "#ALL#")
                        filter.AddField("*");
                    else if (field == "#SHAPE#" && tc is IFeatureClass)
                        filter.AddField(((IFeatureClass)tc).ShapeFieldName);
                    else if (field == "#ID#")
                        filter.AddField(tc.IDFieldName);
                    else
                        filter.AddField(field);
                }
            }
            else
            {
                filter.SubFields = "*";
            }

            XmlNode featureCoordsys = query.SelectSingleNode("FEATURECOORDSYS");
            if (featureCoordsys != null)
            {
                filter.FeatureSpatialReference = ArcXMLGeometry.AXL2SpatialReference(featureCoordsys);
            }

            XmlNode spatialFilter = query.SelectSingleNode("SPATIALFILTER");
            if (query.Name == "SPATIALQUERY" && spatialFilter != null)
            {
                ((ISpatialFilter)filter).SpatialRelation =
                    (spatialFilter.Attributes["relation"].Value == "envelope_intersection") ? spatialRelation.SpatialRelationEnvelopeIntersects : spatialRelation.SpatialRelationIntersects;
                ((ISpatialFilter)filter).Geometry = Geometry(spatialFilter.InnerXml);

                XmlNode filterCoordsys = query.SelectSingleNode("FILTERCOORDSYS");
                if (featureCoordsys != null)
                {
                    ((ISpatialFilter)filter).FilterSpatialReference = ArcXMLGeometry.AXL2SpatialReference(filterCoordsys);
                }
            }

            XmlNode bufferNode = query.SelectSingleNode("BUFFER[@distance]");
            if (bufferNode != null)
            {
                BufferQueryFilter bFilter = new BufferQueryFilter(/*filter*/);
                bFilter.SubFields = "*";
                bFilter.BufferDistance = Convert.ToDouble(bufferNode.Attributes["distance"].Value.Replace(",", "."), _nhi);
                bFilter.RootFilter = filter;
                bFilter.RootFeatureClass = rootFeateatureClass;
                bFilter.FeatureSpatialReference = filter.FeatureSpatialReference;

                filter = bFilter;
            }


            return filter;
        }

        static public IGeometry Geometry(string axl)
        {
            return ArcXMLGeometry.AXL2Geometry(axl);
        }

        public static SimpleRenderer SimpleRenderer(XmlNode axl)
        {
            SimpleRenderer renderer = new SimpleRenderer();

            foreach (XmlNode child in axl.ChildNodes)
            {
                ISymbol symbol = SimpleSymbol(child);
                if (symbol != null)
                    renderer.Symbol = symbol;
            }

            renderer.SymbolRotation = SymbolRotationFromAXLAttribtures(axl);
            return renderer;
        }
        public static IFeatureRenderer ValueMapRenderer(XmlNode axl)
        {
            if (axl == null || axl.Attributes["lookupfield"] == null) return null;
            if (axl.SelectSingleNode("RANGE") != null)
                return QuantityRenderer(axl);

            ValueMapRenderer renderer = new ValueMapRenderer();

            renderer.ValueField = axl.Attributes["lookupfield"].Value;
            foreach (XmlNode child in axl.ChildNodes)
            {
                if (child.Name == "EXACT" && child.Attributes["value"] != null)
                {
                    ISymbol symbol = (child.ChildNodes.Count > 0) ? SimpleSymbol(child.ChildNodes[0]) : null;
                    if (symbol is ILegendItem && child.Attributes["label"] != null)
                        ((LegendItem)symbol).LegendLabel = child.Attributes["label"].Value;
                    renderer[child.Attributes["value"].Value] = symbol;
                }
                else if (child.Name == "OTHER")
                {
                    ISymbol symbol = (child.ChildNodes.Count > 0) ? SimpleSymbol(child.ChildNodes[0]) : null;
                    if (symbol is ILegendItem && child.Attributes["label"] != null)
                        ((LegendItem)symbol).LegendLabel = child.Attributes["label"].Value;
                    renderer.DefaultSymbol = symbol;
                }
            }

            renderer.SymbolRotation = SymbolRotationFromAXLAttribtures(axl);

            return renderer;
        }
        private static QuantityRenderer QuantityRenderer(XmlNode axl)
        {
            try
            {
                QuantityRenderer renderer = new QuantityRenderer();
                renderer.ValueField = axl.Attributes["lookupfield"].Value;

                foreach (XmlNode child in axl.ChildNodes)
                {
                    if (child.Name == "RANGE" && child.Attributes["lower"] != null && child.Attributes["upper"] != null)
                    {
                        ISymbol symbol = (child.ChildNodes.Count > 0) ? SimpleSymbol(child.ChildNodes[0]) : null;
                        if (symbol is ILegendItem && child.Attributes["label"] != null)
                            ((LegendItem)symbol).LegendLabel = child.Attributes["label"].Value;

                        renderer.AddClass(new QuantityRenderer.QuantityClass(
                            double.Parse(child.Attributes["lower"].Value.Replace(",", "."), _nhi),
                            double.Parse(child.Attributes["upper"].Value.Replace(",", "."), _nhi),
                            symbol));
                    }
                    else if (child.Name == "OTHER")
                    {
                        ISymbol symbol = (child.ChildNodes.Count > 0) ? SimpleSymbol(child.ChildNodes[0]) : null;
                        if (symbol is ILegendItem && child.Attributes["label"] != null)
                            ((LegendItem)symbol).LegendLabel = child.Attributes["label"].Value;
                        renderer.DefaultSymbol = symbol;
                    }
                }

                return renderer;
            }
            catch
            {
                return null;
            }
        }

        public static SimpleLabelRenderer SimpleLabelRenderer(XmlNode axl, IFeatureClass fc)
        {
            if (axl == null || fc == null) return null;
            if (axl.Attributes["field"] == null) return null;

            string fieldname = axl.Attributes["field"].Value;
            bool found = false;
            foreach (IField field in fc.Fields.ToEnumerable())
            {
                if (field.name == fieldname)
                {
                    found = true;
                    break;
                }
            }

            if (fc.FindField(fieldname) == null) return null;
            if (!found) return null;

            XmlNode symbol = axl.SelectSingleNode("TEXTSYMBOL");
            if (symbol == null) symbol = axl.SelectSingleNode("TEXTMARKERSYMBOL");
            if (symbol == null) return null;

            SimpleLabelRenderer renderer = new SimpleLabelRenderer();
            bool hasAlignment = false;
            renderer.TextSymbol = SimpleTextSymbol(symbol, out hasAlignment);
            if (renderer.TextSymbol == null) return null;

            renderer.FieldName = fieldname;

            if (!hasAlignment)
            {
                renderer.TextSymbol.TextSymbolAlignment = TextSymbolAlignment.rightAlignOver;
                if (fc.GeometryType == geometryType.Polyline)
                    renderer.TextSymbol.TextSymbolAlignment = TextSymbolAlignment.Over;
                else if (fc.GeometryType == geometryType.Polygon)
                    renderer.TextSymbol.TextSymbolAlignment = TextSymbolAlignment.Center;
            }

            if (axl.Attributes["howmanylabels"] != null)
            {
                switch (axl.Attributes["howmanylabels"].Value.ToLower())
                {
                    case "one_label_per_name":
                        renderer.HowManyLabels = gView.Framework.Carto.Rendering.SimpleLabelRenderer.howManyLabels.one_per_name;
                        break;
                    case "one_label_per_shape":
                        renderer.HowManyLabels = gView.Framework.Carto.Rendering.SimpleLabelRenderer.howManyLabels.one_per_feature;
                        break;
                    case "one_label_per_part":
                        renderer.HowManyLabels = gView.Framework.Carto.Rendering.SimpleLabelRenderer.howManyLabels.one_per_part;
                        break;
                }
            }
            if (axl.Attributes["labelweight"] != null)
            {
                switch (axl.Attributes["labelweight"].Value.ToLower())
                {
                    case "no_weight":
                        renderer.LabelPriority = gView.Framework.Carto.Rendering.SimpleLabelRenderer.labelPriority.low;
                        break;
                    case "med_weight":
                        renderer.LabelPriority = gView.Framework.Carto.Rendering.SimpleLabelRenderer.labelPriority.normal;
                        break;
                    case "high_weight":
                        renderer.LabelPriority = gView.Framework.Carto.Rendering.SimpleLabelRenderer.labelPriority.high;
                        break;
                    case "always":
                        renderer.LabelPriority = gView.Framework.Carto.Rendering.SimpleLabelRenderer.labelPriority.always;
                        break;
                }
            }
            if (axl.Attributes["cartomethod"] != null)
            {
                switch (axl.Attributes["cartomethod"].Value.ToLower())
                {
                    case "horizontal":
                        renderer.CartoLineLabelling = gView.Framework.Carto.Rendering.SimpleLabelRenderer.CartographicLineLabeling.Horizontal;
                        break;
                    case "perpendicular":
                        renderer.CartoLineLabelling = gView.Framework.Carto.Rendering.SimpleLabelRenderer.CartographicLineLabeling.Perpendicular;
                        break;
                    case "curvedtext":
                        renderer.CartoLineLabelling = gView.Framework.Carto.Rendering.SimpleLabelRenderer.CartographicLineLabeling.CurvedText;
                        break;
                    default:
                        renderer.CartoLineLabelling = gView.Framework.Carto.Rendering.SimpleLabelRenderer.CartographicLineLabeling.Parallel;
                        break;
                }
            }
            if (axl.Attributes["expression"] != null)
            {
                renderer.LabelExpression = axl.Attributes["expression"].Value;
                renderer.UseExpression = true;
            }

            renderer.SymbolRotation = SymbolRotationFromAXLAttribtures(axl);

            return renderer;
        }
        public static IFeatureRenderer GroupRenderer(XmlNode axl)
        {
            FeatureGroupRenderer renderer = new FeatureGroupRenderer();

            foreach (XmlNode child in axl.ChildNodes)
            {
                if (child.Name == "SIMPLERENDERER")
                {
                    IFeatureRenderer r = ObjectFromAXLFactory.SimpleRenderer(child);
                    if (r != null)
                        renderer.Renderers.Add(r);
                }
                else if (child.Name == "VALUEMAPRENDERER")
                {
                    IFeatureRenderer r = ObjectFromAXLFactory.ValueMapRenderer(child);
                    if (r != null)
                        renderer.Renderers.Add(r);
                }
                else if (child.Name == "GROUPRENDERER")
                {
                    IFeatureRenderer r = ObjectFromAXLFactory.GroupRenderer(child);
                    if (r != null)
                        renderer.Renderers.Add(r);
                }
                else if (child.Name == "SCALEDEPENDENTRENDERER")
                {
                    IFeatureRenderer r = ObjectFromAXLFactory.ScaleDependentRenderer(child);
                    if (r != null)
                        renderer.Renderers.Add(r);
                }
            }

            if (renderer.Renderers.Count == 1)
                return renderer.Renderers[0];

            return renderer;
        }

        public static ScaleDependentRenderer ScaleDependentRenderer(XmlNode axl)
        {
            try
            {
                ScaleDependentRenderer renderer = new ScaleDependentRenderer();

                double minScale = axl.Attributes["lower"] != null ? Scale(axl.Attributes["lower"].Value) : 0D;
                double maxScale = axl.Attributes["upper"] != null ? Scale(axl.Attributes["upper"].Value) : 0D;

                object rendererObj = GroupRenderer(axl);
                if (rendererObj == null)
                    return null;

                if (rendererObj is IGroupRenderer)
                {
                    foreach (IFeatureRenderer childRenderer in ((IGroupRenderer)rendererObj).Renderers)
                    {
                        renderer.Renderers.Add(childRenderer);
                    }
                }
                else if (rendererObj is IFeatureRenderer)
                {
                    renderer.Renderers.Add((IFeatureRenderer)rendererObj);
                }

                foreach (IScaledependent sdRenderer in renderer.Renderers)
                {
                    sdRenderer.MinimumScale = minScale;
                    sdRenderer.MaximumScale = maxScale;
                }

                return renderer.Renderers.Count > 0 ? renderer : null;
            }
            catch
            {
                return null;
            }
        }

        public static ScaleDependentLabelRenderer ScaleDependentLabelRenderer(XmlNode axl, IFeatureClass fc)
        {
            try
            {
                ScaleDependentLabelRenderer renderer = new ScaleDependentLabelRenderer();

                double minScale = axl.Attributes["lower"] != null ? Scale(axl.Attributes["lower"].Value) : 0D;
                double maxScale = axl.Attributes["upper"] != null ? Scale(axl.Attributes["upper"].Value) : 0D;

                foreach (XmlNode labelRendererNode in axl.SelectNodes("SIMPLELABELRENDERER"))
                {
                    SimpleLabelRenderer labelRenderer = SimpleLabelRenderer(labelRendererNode, fc);
                    if (labelRenderer == null)
                        continue;

                    renderer.Renderers.Add(labelRenderer);
                }

                foreach (IScaledependent sdRenderer in renderer.Renderers)
                {
                    sdRenderer.MinimumScale = minScale;
                    sdRenderer.MaximumScale = maxScale;
                }

                return renderer;
            }
            catch
            {
                return null;
            }
        }

        public static string ConvertToAXL(object gViewObject)
        {
            if (gViewObject is IToArcXml)
                return ((IToArcXml)gViewObject).ArcXml;

            if (gViewObject is SimpleRenderer)
            {
                return SimpleRenderer2AXL(gViewObject as SimpleRenderer);
            }
            if (gViewObject is SimpleLabelRenderer)
            {
                return SimpleLabelRenderer2AXL(gViewObject as SimpleLabelRenderer);
            }
            if (gViewObject is ValueMapRenderer)
            {
                return ValueMapRenderer2AXL(gViewObject as ValueMapRenderer);
            }
            if (gViewObject is QuantityRenderer)
            {
                return QuantityRenderer2AXL(gViewObject as QuantityRenderer);
            }
            if (gViewObject is FeatureGroupRenderer)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<GROUPRENDERER>");
                foreach (IFeatureRenderer renderer in ((FeatureGroupRenderer)gViewObject).Renderers)
                {
                    sb.Append(ConvertToAXL(renderer));
                }
                sb.Append("</GROUPRENDERER>");
                return sb.ToString();
            }
            if (gViewObject is ISymbol)
            {
                return Symbol2AXL(gViewObject as ISymbol, null);
            }

            return "";
        }
        public static string SimpleRenderer2AXL(SimpleRenderer renderer)
        {
            if (renderer == null) return "";

            string symbol = Symbol2AXL(renderer.Symbol, renderer.SymbolRotation);
            if (symbol.Length > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<SIMPLERENDERER");
                sb.Append(SymbolRotation2AXLParameters(renderer.SymbolRotation));
                sb.Append(">\n");
                sb.Append(symbol);
                sb.Append("\n</SIMPLERENDERER>\n");
                return sb.ToString();
            }
            return "";
        }
        public static string SimpleLabelRenderer2AXL(SimpleLabelRenderer renderer)
        {
            if (renderer == null) return "";

            string symbol = Symbol2AXL(renderer.TextSymbol, renderer.SymbolRotation);
            if (symbol.Length > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<SIMPLELABELRENDERER");
                sb.Append(" field=\"" + renderer.FieldName + "\"");
                switch (renderer.HowManyLabels)
                {
                    case gView.Framework.Carto.Rendering.SimpleLabelRenderer.howManyLabels.one_per_name:
                        sb.Append(@" howmanylabels=""one_label_per_name""");
                        break;
                    case gView.Framework.Carto.Rendering.SimpleLabelRenderer.howManyLabels.one_per_feature:
                        sb.Append(@" howmanylabels=""one_label_per_shape""");
                        break;
                    case gView.Framework.Carto.Rendering.SimpleLabelRenderer.howManyLabels.one_per_part:
                        sb.Append(@" howmanylabels=""one_label_per_part""");
                        break;
                }
                switch (renderer.LabelPriority)
                {
                    case gView.Framework.Carto.Rendering.SimpleLabelRenderer.labelPriority.low:
                        sb.Append(@" labelweight =""no_weight""");
                        break;
                    case gView.Framework.Carto.Rendering.SimpleLabelRenderer.labelPriority.normal:
                        sb.Append(@" labelweight =""med_weight""");
                        break;
                    case gView.Framework.Carto.Rendering.SimpleLabelRenderer.labelPriority.high:
                        sb.Append(@" labelweight =""high_weight""");
                        break;
                    case gView.Framework.Carto.Rendering.SimpleLabelRenderer.labelPriority.always:
                        sb.Append(@" labelweight =""always""");
                        break;
                }
                switch (renderer.CartoLineLabelling)
                {
                    case gView.Framework.Carto.Rendering.SimpleLabelRenderer.CartographicLineLabeling.Horizontal:
                        sb.Append(@" cartomethod=""horizontal""");
                        break;
                    case gView.Framework.Carto.Rendering.SimpleLabelRenderer.CartographicLineLabeling.Perpendicular:
                        sb.Append(@" cartomethod=""perpendicular""");
                        break;
                    case gView.Framework.Carto.Rendering.SimpleLabelRenderer.CartographicLineLabeling.CurvedText:
                        sb.Append(@" cartomethod=""curvedtext""");
                        break;
                    default:
                        sb.Append(@" cartomethod=""parallel""");
                        break;
                }
                if (renderer.LabelExpression != null && renderer.LabelExpression != String.Empty)
                {
                    sb.Append(@" expression=""" + renderer.LabelExpression.Replace("&", "&amp;").Replace("\"", "&quot;").Replace("<", "&lt;").Replace(">", "&gt;") + "\"");
                }
                sb.Append(SymbolRotation2AXLParameters(renderer.SymbolRotation));
                sb.Append(">\n");
                sb.Append(symbol);
                sb.Append("\n</SIMPLELABELRENDERER>\n");
                return sb.ToString();
            }
            return "";

        }
        public static string ValueMapRenderer2AXL(ValueMapRenderer renderer)
        {
            if (renderer == null) return "";

            StringBuilder sb = new StringBuilder();
            sb.Append("<VALUEMAPRENDERER");
            sb.Append(" lookupfield=\"" + renderer.ValueField + "\"");
            sb.Append(SymbolRotation2AXLParameters(renderer.SymbolRotation));
            sb.Append(">\n");

            foreach (string key in renderer.Keys)
            {
                string symbol = Symbol2AXL(renderer[key], renderer.SymbolRotation);
                if (symbol != "")
                {
                    sb.Append("<EXACT");
                    sb.Append(" value=\"" + CheckEsc(key) + "\"");
                    if (renderer[key] is ILegendItem)
                        sb.Append(" label=\"" + CheckEsc((((ILegendItem)renderer[key]).LegendLabel)) + "\"");
                    sb.Append(">\n");
                    sb.Append(symbol);
                    sb.Append("\n</EXACT>\n");
                }
            }
            string defaultSymbol = Symbol2AXL(renderer.DefaultSymbol, renderer.SymbolRotation);
            if (defaultSymbol != "")
            {
                sb.Append("<OTHER>");
                sb.Append(defaultSymbol);
                sb.Append("</OTHER>");
            }

            sb.Append("\n</VALUEMAPRENDERER>\n");
            return sb.ToString();
        }

        public static string QuantityRenderer2AXL(QuantityRenderer renderer)
        {
            if (renderer == null) return String.Empty;

            StringBuilder sb = new StringBuilder();
            sb.Append("<VALUEMAPRENDERER");
            sb.Append(" lookupfield=\"" + renderer.ValueField + "\"");
            sb.Append(SymbolRotation2AXLParameters(renderer.SymbolRotation));
            sb.Append(">\n");

            foreach (QuantityRenderer.QuantityClass qClass in renderer.QuantityClasses)
            {
                string symbol = Symbol2AXL(qClass.Symbol, renderer.SymbolRotation);
                if (String.IsNullOrEmpty(symbol))
                {
                    sb.Append("<RANGE");
                    sb.Append(" lower=\"" + qClass.Min.ToString(_nhi) + "\" upper=\"" + qClass.Max.ToString(_nhi) + "\"");
                    if (qClass.Symbol is ILegendItem)
                        sb.Append(" label=\"" + CheckEsc((((ILegendItem)qClass.Symbol).LegendLabel)) + "\"");
                    sb.Append(">\n");
                    sb.Append(symbol);
                    sb.Append("\n</RANGE>\n");
                }
            }
            string defaultSymbol = Symbol2AXL(renderer.DefaultSymbol, renderer.SymbolRotation);
            if (String.IsNullOrEmpty(defaultSymbol))
            {
                sb.Append("<OTHER>");
                sb.Append(defaultSymbol);
                sb.Append("</OTHER>");
            }

            sb.Append("\n</VALUEMAPRENDERER>\n");
            return sb.ToString();
        }

        public static string SimpleFillSymbol2AXL(SimpleFillSymbol symbol, SymbolRotation rotation)
        {
            if (symbol == null) return "";

            StringBuilder sb = new StringBuilder();
            sb.Append("<SIMPLEPOLYGONSYMBOL");
            sb.Append(" boundary=\"" + ((symbol.OutlineSymbol != null) ? "true" : "false") + "\"");
            System.Drawing.Color col;
            if (symbol.OutlineSymbol != null)
            {
                col = symbol.OutlineColor;
                sb.Append(" boundarycolor=\"" + Color2AXL(col) + "\"");
                if (col.A != 255)
                    sb.Append(" boundarytransparency=\"" + ColorTransparency2AXL(col) + "\"");
                if (symbol.OutlineDashStyle != System.Drawing.Drawing2D.DashStyle.Solid)
                    sb.Append(" boundarytype=\"" + DashStyle2AXL(symbol.OutlineDashStyle) + "\"");
                if (symbol.OutlineWidth != 1)
                    sb.Append(" boundarywidth=\"" + symbol.OutlineWidth.ToString() + "\"");
            }
            col = symbol.Color;
            sb.Append(" fillcolor=\"" + Color2AXL(col) + "\"");
            if (col.A != 255)
                sb.Append(" filltransparency=\"" + ColorTransparency2AXL(col) + "\"");
            sb.Append("/>");

            return sb.ToString();
        }
        public static string HatchSymbol2AXL(HatchSymbol symbol, SymbolRotation rotation)
        {
            if (symbol == null) return "";

            StringBuilder sb = new StringBuilder();
            sb.Append("<SIMPLEPOLYGONSYMBOL");
            sb.Append(" boundary=\"" + ((symbol.OutlineSymbol != null) ? "true" : "false") + "\"");
            System.Drawing.Color col;
            if (symbol.OutlineSymbol != null)
            {
                col = symbol.OutlineColor;
                sb.Append(" boundarycolor=\"" + Color2AXL(col) + "\"");
                if (col.A != 255)
                    sb.Append(" boundarytransparency=\"" + ColorTransparency2AXL(col) + "\"");
                if (symbol.OutlineDashStyle != System.Drawing.Drawing2D.DashStyle.Solid)
                    sb.Append(" boundarytype=\"" + DashStyle2AXL(symbol.OutlineDashStyle) + "\"");
                if (symbol.OutlineWidth != 1)
                    sb.Append(" boundarywidth=\"" + symbol.OutlineWidth.ToString() + "\"");
            }
            col = symbol.ForeColor;
            sb.Append(" fillcolor=\"" + Color2AXL(col) + "\"");
            if (col.A != 255)
                sb.Append(" filltransparency=\"" + ColorTransparency2AXL(col) + "\"");

            switch (symbol.HatchStyle)
            {
                case System.Drawing.Drawing2D.HatchStyle.BackwardDiagonal:
                    sb.Append(" filltype=\"bdiagonal\"");
                    break;
                case System.Drawing.Drawing2D.HatchStyle.ForwardDiagonal:
                    sb.Append(" filltype=\"fdiagonal\"");
                    break;
                case System.Drawing.Drawing2D.HatchStyle.Cross:
                    sb.Append(" filltype=\"cross\"");
                    break;
                case System.Drawing.Drawing2D.HatchStyle.DiagonalCross:
                    sb.Append(" filltype=\"diagcross\"");
                    break;
                case System.Drawing.Drawing2D.HatchStyle.Horizontal:
                    sb.Append(" filltype=\"horizontal\"");
                    break;
                case System.Drawing.Drawing2D.HatchStyle.Vertical:
                    sb.Append(" filltype=\"vertical\"");
                    break;
                case System.Drawing.Drawing2D.HatchStyle.Percent05:
                case System.Drawing.Drawing2D.HatchStyle.Percent10:
                case System.Drawing.Drawing2D.HatchStyle.Percent20:
                case System.Drawing.Drawing2D.HatchStyle.Percent25:
                    sb.Append(" filltype=\"lightgray\"");
                    break;
                case System.Drawing.Drawing2D.HatchStyle.Percent30:
                case System.Drawing.Drawing2D.HatchStyle.Percent40:
                case System.Drawing.Drawing2D.HatchStyle.Percent50:
                case System.Drawing.Drawing2D.HatchStyle.Percent60:
                    sb.Append(" filltype=\"gray\"");
                    break;
                case System.Drawing.Drawing2D.HatchStyle.Percent70:
                case System.Drawing.Drawing2D.HatchStyle.Percent75:
                case System.Drawing.Drawing2D.HatchStyle.Percent80:
                case System.Drawing.Drawing2D.HatchStyle.Percent90:
                    sb.Append(" filltype=\"darkgray\"");
                    break;
                default:
                    sb.Append(" filltype=\"bdiagonal\"");
                    break;
            }
            sb.Append("/>");

            return sb.ToString();
        }
        public static string SimpleLineSymbol2AXL(SimpleLineSymbol symbol, SymbolRotation rotation)
        {
            if (symbol == null) return "";

            StringBuilder sb = new StringBuilder();
            sb.Append("<SIMPLELINESYMBOL");
            sb.Append(" color=\"" + Color2AXL(symbol.Color) + "\"");
            if (symbol.Color.A != 255)
                sb.Append(" transparency=\"" + ColorTransparency2AXL(symbol.Color) + "\"");
            if (symbol.Width != 1)
                sb.Append(" width=\"" + symbol.Width.ToString() + "\"");
            if (symbol.DashStyle != System.Drawing.Drawing2D.DashStyle.Solid)
                sb.Append(" type=\"" + DashStyle2AXL(symbol.DashStyle) + "\"");
            if (symbol.Smoothingmode == SymbolSmoothing.AntiAlias)
                sb.Append(" antialiasing=\"true\"");
            if (symbol.LineStartCap != System.Drawing.Drawing2D.LineCap.Flat ||
                symbol.LineEndCap != System.Drawing.Drawing2D.LineCap.Flat)
            {
                if (symbol.LineStartCap == symbol.LineEndCap)
                {
                    sb.Append(" captype=\"");
                    switch (symbol.LineStartCap)
                    {
                        case System.Drawing.Drawing2D.LineCap.Flat:
                            sb.Append("butt");
                            break;
                        default:
                            sb.Append(symbol.LineStartCap.ToString().ToLower());
                            break;
                    }
                    sb.Append("\"");
                }
                else
                {
                    sb.Append(" startcaptype=\"");
                    switch (symbol.LineStartCap)
                    {
                        case System.Drawing.Drawing2D.LineCap.Flat:
                            sb.Append("butt");
                            break;
                        default:
                            sb.Append(symbol.LineStartCap.ToString().ToLower());
                            break;
                    }
                    sb.Append("\"");

                    sb.Append(" endcaptype=\"");
                    switch (symbol.LineEndCap)
                    {
                        case System.Drawing.Drawing2D.LineCap.Flat:
                            sb.Append("butt");
                            break;
                        default:
                            sb.Append(symbol.LineEndCap.ToString().ToLower());
                            break;
                    }
                    sb.Append("\"");
                }
            }
            sb.Append("/>");

            return sb.ToString();
        }
        public static string SimplePointSymbol2AXL(SimplePointSymbol symbol, SymbolRotation rotation)
        {
            if (symbol == null) return "";

            StringBuilder sb = new StringBuilder();
            sb.Append("<SIMPLEMARKERSYMBOL");
            sb.Append(" color=\"" + Color2AXL(symbol.Color) + "\"");
            sb.Append(" transparency=\"" + ColorTransparency2AXL(symbol.Color) + "\"");
            sb.Append(" outline=\"" + Color2AXL(symbol.OutlineColor) + "\"");
            if (symbol.Marker != SimplePointSymbol.MarkerType.Circle)
                sb.Append(" type=\"" + MarkerType2AXL(symbol.Marker) + "\"");
            sb.Append(" width=\"" + symbol.Size.ToString() + "\"");
            sb.Append(" symbolwidth=\"" + symbol.SymbolWidth.ToString() + "\"");
            sb.Append(" gv_outlinetransparency=\"" + ColorTransparency2AXL(symbol.OutlineColor) + "\"");
            sb.Append(" gv_outlinewidth=\"" + symbol.PenWidth.ToString() + "\"");
            sb.Append("/>");

            return sb.ToString();
        }
        public static string SimpleTextSymbol2AXL(SimpleTextSymbol symbol, SymbolRotation rotation)
        {
            if (symbol == null) return "";

            StringBuilder sb = new StringBuilder();
            sb.Append("<TEXTMARKERSYMBOL");
            if (symbol.Angle != 0f)
                sb.Append(" angle=\"" + symbol.Angle + "\"");
            if (symbol.Font != null)
            {
                sb.Append(" font=\"" + symbol.Font.Name + "\"");
                sb.Append(" fontsize=\"" + symbol.Font.Size + "\"");
                sb.Append(" fontstyle=\"" + FontStyle2AXL(symbol.Font.Style) + "\"");
            }
            sb.Append(" fontcolor=\"" + Color2AXL(symbol.Color) + "\"");
            sb.Append(" halignment=\"" + HTextSymbolAlignment2AXL(symbol.TextSymbolAlignment) + "\"");
            sb.Append(" valignment=\"" + VTextSymbolAlignment2AXL(symbol.TextSymbolAlignment) + "\"");
            if (symbol.Color.A != 255)
                sb.Append(" transparency=\"" + ColorTransparency2AXL(symbol.Color) + "\"");

            if (symbol.Smoothingmode == SymbolSmoothing.AntiAlias)
                sb.Append(" antialiasing=\"true\"");

            if (symbol is GlowingTextSymbol)
            {
                sb.Append(" glowing=\"" + Color2AXL(((GlowingTextSymbol)symbol).GlowingColor) + "\"");
                sb.Append(" glowingwidth=\"" + ((GlowingTextSymbol)symbol).GlowingWidth + "\"");
                if (((GlowingTextSymbol)symbol).GlowingSmoothingmode == SymbolSmoothing.AntiAlias)
                    sb.Append(" glowing_antialiasing=\"true\"");
            }

            sb.Append("/>");

            return sb.ToString();
        }
        public static string TrueTypeMarkerSymol2AXL(TrueTypeMarkerSymbol symbol, SymbolRotation rotation)
        {
            if (symbol == null) return "";

            StringBuilder sb = new StringBuilder();
            sb.Append("<TRUETYPEMARKERSYMBOL");
            sb.Append(" character=\"" + symbol.Charakter + "\"");
            if (symbol.Angle != 0f)
                sb.Append(" angle=\"" + symbol.Angle + "\"");
            if (symbol.Font != null)
            {
                sb.Append(" font=\"" + symbol.Font.Name + "\"");
                sb.Append(" fontsize=\"" + symbol.Font.Size + "\"");
                sb.Append(" fontstyle=\"" + FontStyle2AXL(symbol.Font.Style) + "\"");
            }

            sb.Append(" fontcolor=\"" + Color2AXL(symbol.Color) + "\"");
            if (symbol.Color.A != 255)
                sb.Append(" transparency=\"" + ColorTransparency2AXL(symbol.Color) + "\"");

            sb.Append(" hotspot=\"" + symbol.HorizontalOffset.ToString(_nhi) + "," + symbol.VerticalOffset.ToString(_nhi) + "\"");
            // Die Symbolrotation hängt bei gView am Renderer
            //sb.Append(SymbolRotation2AXLParameters(rotation));
            sb.Append("/>");

            return sb.ToString();
        }

        private static string SymbolRotation2AXLParameters(SymbolRotation rotation)
        {
            if (rotation == null || rotation.RotationFieldName == null ||
                rotation.RotationFieldName == String.Empty) return "";

            StringBuilder sb = new StringBuilder();
            sb.Append(" anglefield=\"" + rotation.RotationFieldName + "\"");
            switch (rotation.RotationType)
            {
                case RotationType.aritmetic:
                    sb.Append(" rotatemethod=\"arithmetic\"");
                    break;
                case RotationType.geographic:
                    sb.Append(" rotatemethod=\"geographic\"");
                    break;
            }
            switch (rotation.RotationUnit)
            {
                case RotationUnit.deg:
                    sb.Append(" rotationunit=\"deg\"");
                    break;
                case RotationUnit.gon:
                    sb.Append(" rotationunit=\"grad\"");
                    break;
                case RotationUnit.rad:
                    sb.Append(" rotationunit=\"rad\"");
                    break;
            }

            return sb.ToString();
        }
        private static SymbolRotation SymbolRotationFromAXLAttribtures(XmlNode axl)
        {
            if (axl == null) return null;
            if (axl.Attributes["anglefield"] == null ||
                axl.Attributes["anglefield"].Value == String.Empty) return null;

            SymbolRotation rotation = new SymbolRotation();
            rotation.RotationFieldName = axl.Attributes["anglefield"].Value;

            if (axl.Attributes["rotatemethod"] != null)
            {
                switch (axl.Attributes["rotatemethod"].Value.ToLower())
                {
                    case "arithmetic":
                        rotation.RotationType = RotationType.aritmetic;
                        break;
                    case "geographic":
                        rotation.RotationType = RotationType.geographic;
                        break;
                }
            }

            if (axl.Attributes["rotationunit"] != null)
            {
                switch (axl.Attributes["rotationunit"].Value.ToLower())
                {
                    case "deg":
                        rotation.RotationUnit = RotationUnit.deg;
                        break;
                    case "grad":
                        rotation.RotationUnit = RotationUnit.gon;
                        break;
                    case "rad":
                        rotation.RotationUnit = RotationUnit.rad;
                        break;
                }
            }

            return rotation;
        }

        private static string Color2AXL(System.Drawing.Color col)
        {
            return col.R + "," + col.G + "," + col.B;
        }
        private static string ColorTransparency2AXL(System.Drawing.Color col)
        {
            return ((float)col.A / 255f).ToString();
        }
        private static string DashStyle2AXL(System.Drawing.Drawing2D.DashStyle dashstyle)
        {
            switch (dashstyle)
            {
                case System.Drawing.Drawing2D.DashStyle.Dash:
                    return "dash";
                case System.Drawing.Drawing2D.DashStyle.DashDot:
                    return "dash_dot";
                case System.Drawing.Drawing2D.DashStyle.DashDotDot:
                    return "dash_dot_dot";
                case System.Drawing.Drawing2D.DashStyle.Dot:
                    return "dot";
            }
            return "solid";
        }
        private static string MarkerType2AXL(SimplePointSymbol.MarkerType type)
        {
            switch (type)
            {
                case SimplePointSymbol.MarkerType.Cross:
                    return "cross";
                case SimplePointSymbol.MarkerType.Square:
                    return "square";
                case SimplePointSymbol.MarkerType.Star:
                    return "star";
                case SimplePointSymbol.MarkerType.Triangle:
                    return "triangle";
            }
            return "circle";
        }
        private static string FontStyle2AXL(System.Drawing.FontStyle fontstyle)
        {
            if (((fontstyle & System.Drawing.FontStyle.Italic) != 0) &&
                ((fontstyle & System.Drawing.FontStyle.Bold) != 0))
                return "bolditalic";
            else if ((fontstyle & System.Drawing.FontStyle.Bold) != 0)
                return "bold";
            else if ((fontstyle & System.Drawing.FontStyle.Italic) != 0)
                return "italic";
            else if ((fontstyle & System.Drawing.FontStyle.Underline) != 0)
                return "underline";
            //else if (fontstyle & System.Drawing.FontStyle.Strikeout)
            //    return "outline";

            return "regular";
        }
        private static string VTextSymbolAlignment2AXL(TextSymbolAlignment alignment)
        {
            switch (alignment)
            {
                case TextSymbolAlignment.Center:
                case TextSymbolAlignment.leftAlignCenter:
                case TextSymbolAlignment.rightAlignCenter:
                    return "center";
                case TextSymbolAlignment.Over:
                case TextSymbolAlignment.leftAlignOver:
                case TextSymbolAlignment.rightAlignOver:
                    return "top";
                case TextSymbolAlignment.Under:
                case TextSymbolAlignment.leftAlignUnder:
                case TextSymbolAlignment.rightAlignUnder:
                    return "bottom";
            }
            return "top";
        }
        private static string HTextSymbolAlignment2AXL(TextSymbolAlignment alignment)
        {
            switch (alignment)
            {
                case TextSymbolAlignment.Center:
                case TextSymbolAlignment.Over:
                case TextSymbolAlignment.Under:
                    return "center";
                case TextSymbolAlignment.leftAlignUnder:
                case TextSymbolAlignment.leftAlignOver:
                case TextSymbolAlignment.leftAlignCenter:
                    return "left";
                case TextSymbolAlignment.rightAlignOver:
                case TextSymbolAlignment.rightAlignCenter:
                case TextSymbolAlignment.rightAlignUnder:
                    return "right";
            }
            return "right";
        }

        public static ISymbol SimpleSymbol(XmlNode axl)
        {
            if (axl == null) return null;

            if (axl.Name == "SIMPLEMARKERSYMBOL")
                return SimpleMarkerSymbol(axl);
            if (axl.Name == "SIMPLELINESYMBOL")
                return SimpleLineSymbol(axl);
            if (axl.Name == "SIMPLEPOLYGONSYMBOL")
                return SimplePolygonSymbol(axl);
            if (axl.Name == "TRUETYPEMARKERSYMBOL")
                return TrueTypeMarkerSymbol(axl);
            if (axl.Name == "TEXTSYMBOL" || axl.Name == "TEXTMARKERSYMBOL")
                return SimpleTextSymbol(axl);
            if (axl.Name == "RASTERMARKERSYMBOL")
                return RasterMarkerSymbol(axl);
            return null;
        }
        public static string Symbol2AXL(ISymbol symbol, SymbolRotation rotation)
        {
            if (symbol == null) return "";

            if (symbol is SimpleFillSymbol)
                return SimpleFillSymbol2AXL(symbol as SimpleFillSymbol, rotation);
            if (symbol is HatchSymbol)
                return HatchSymbol2AXL(symbol as HatchSymbol, rotation);
            if (symbol is SimpleLineSymbol)
                return SimpleLineSymbol2AXL(symbol as SimpleLineSymbol, rotation);
            if (symbol is SimplePointSymbol)
                return SimplePointSymbol2AXL(symbol as SimplePointSymbol, rotation);
            if (symbol is SimpleTextSymbol)
                return SimpleTextSymbol2AXL(symbol as SimpleTextSymbol, rotation);
            if (symbol is TrueTypeMarkerSymbol)
                return TrueTypeMarkerSymol2AXL(symbol as TrueTypeMarkerSymbol, rotation);

            return "";
        }
        public static IFillSymbol SimplePolygonSymbol(XmlNode axl)
        {
            if (axl == null) return null;


            IFillSymbol symbol = null;
            if (axl.Attributes["filltype"] == null ||
                axl.Attributes["filltype"].Value == "solid" ||
                axl.Attributes["filltype"].Value == "")
            {
                symbol = new SimpleFillSymbol();
            }
            else
            {
                symbol = new HatchSymbol();
                switch (axl.Attributes["filltype"].Value)
                {
                    case "bdiagonal":
                        ((HatchSymbol)symbol).HatchStyle = System.Drawing.Drawing2D.HatchStyle.BackwardDiagonal;
                        break;
                    case "fdiagonal":
                        ((HatchSymbol)symbol).HatchStyle = System.Drawing.Drawing2D.HatchStyle.ForwardDiagonal;
                        break;
                    case "cross":
                        ((HatchSymbol)symbol).HatchStyle = System.Drawing.Drawing2D.HatchStyle.Cross;
                        break;
                    case "diagcross":
                        ((HatchSymbol)symbol).HatchStyle = System.Drawing.Drawing2D.HatchStyle.DiagonalCross;
                        break;
                    case "horizontal":
                        ((HatchSymbol)symbol).HatchStyle = System.Drawing.Drawing2D.HatchStyle.Horizontal;
                        break;
                    case "vertical":
                        ((HatchSymbol)symbol).HatchStyle = System.Drawing.Drawing2D.HatchStyle.Vertical;
                        break;
                    case "gray":
                        ((HatchSymbol)symbol).HatchStyle = System.Drawing.Drawing2D.HatchStyle.Percent50;
                        break;
                    case "lightgray":
                        ((HatchSymbol)symbol).HatchStyle = System.Drawing.Drawing2D.HatchStyle.Percent05;
                        break;
                    case "darkgray":
                        ((HatchSymbol)symbol).HatchStyle = System.Drawing.Drawing2D.HatchStyle.Percent80;
                        break;
                    default:
                        ((HatchSymbol)symbol).HatchStyle = System.Drawing.Drawing2D.HatchStyle.BackwardDiagonal;
                        break;
                }
            }
            new SimpleFillSymbol();
            SimpleLineSymbol lSymbol = null;

            int A1 = 255, A2 = 255;
            if (axl.Attributes["filltransparency"] != null)
            {
                A1 = (int)(double.Parse(axl.Attributes["filltransparency"].Value.Replace(",", "."), _nhi) * 255.0);
                if (A1 < 0) A1 = 0;
                if (A1 > 255) A1 = 255;
            }
            if (axl.Attributes["boundarytransparency"] != null)
            {
                A2 = (int)(double.Parse(axl.Attributes["boundarytransparency"].Value.Replace(",", "."), _nhi) * 255.0);
                if (A1 < 0) A2 = 0;
                if (A1 > 255) A2 = 255;
            }
            if (axl.Attributes["transparency"] != null)
            {
                A1 = A2 = (int)(double.Parse(axl.Attributes["transparency"].Value.Replace(",", "."), _nhi) * 255.0);
                if (A1 < 0) A1 = A2 = 0;
                if (A1 > 255) A1 = A2 = 255;
            }

            if (axl.Attributes["fillcolor"] != null)
            {
                if (symbol is SimpleFillSymbol)
                    ((SimpleFillSymbol)symbol).Color = ColorFromRGB(A1, axl.Attributes["fillcolor"].Value);
                else if (symbol is HatchSymbol)
                    ((HatchSymbol)symbol).ForeColor = ColorFromRGB(A1, axl.Attributes["fillcolor"].Value);
            }
            else
            {
                if (symbol is SimpleFillSymbol)
                    ((SimpleFillSymbol)symbol).Color = System.Drawing.Color.FromArgb(A1, ((SimpleFillSymbol)symbol).Color);
                else if (symbol is HatchSymbol)
                    ((HatchSymbol)symbol).ForeColor = System.Drawing.Color.FromArgb(A1, ((HatchSymbol)symbol).ForeColor);
            }

            if (axl.Attributes["boundarycolor"] != null)
            {
                if (lSymbol == null) lSymbol = new SimpleLineSymbol();
                lSymbol.Color = ColorFromRGB(A2, axl.Attributes["boundarycolor"].Value);
            }
            else
            {
                if (lSymbol == null) lSymbol = new SimpleLineSymbol();
                lSymbol.Color = System.Drawing.Color.FromArgb(A2, System.Drawing.Color.Black);
            }

            if (axl.Attributes["boundarywidth"] != null)
            {
                if (lSymbol == null) lSymbol = new SimpleLineSymbol();
                lSymbol.Width = float.Parse(axl.Attributes["boundarywidth"].Value.Replace(",", "."), _nhi);
            }

            if (lSymbol != null)
            {
                if (symbol is SimpleFillSymbol)
                    ((SimpleFillSymbol)symbol).OutlineSymbol = lSymbol;
                else if (symbol is HatchSymbol)
                    ((HatchSymbol)symbol).OutlineSymbol = lSymbol;
            }
            return symbol;
        }
        public static SimpleLineSymbol SimpleLineSymbol(XmlNode axl)
        {
            if (axl == null) return null;

            SimpleLineSymbol symbol = new SimpleLineSymbol();

            int A = 255;
            if (axl.Attributes["transparency"] != null)
            {
                A = (int)(double.Parse(axl.Attributes["transparency"].Value.Replace(",", "."), _nhi) * 255.0);
                if (A < 0) A = 0;
                if (A > 255) A = 255;
            }

            if (axl.Attributes["color"] != null)
            {
                symbol.Color = ColorFromRGB(A, axl.Attributes["color"].Value);
            }
            else if (A != 255)
            {
                symbol.Color = System.Drawing.Color.FromArgb(A, symbol.Color);
            }

            if (axl.Attributes["width"] != null)
            {
                symbol.Width = float.Parse(axl.Attributes["width"].Value.Replace(",", "."), _nhi);
            }
            if (axl.Attributes["type"] != null)
            {
                switch (axl.Attributes["type"].Value)
                {
                    case "dash":
                        symbol.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                        break;
                    case "dot":
                        symbol.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                        break;
                    case "dash_dot":
                        symbol.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;
                        break;
                    case "dash_dot_dot":
                        symbol.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDotDot;
                        break;
                }
            }
            if (axl.Attributes["antialiasing"] != null &&
                axl.Attributes["antialiasing"].Value.ToLower() == "true")
            {
                symbol.Smoothingmode = SymbolSmoothing.AntiAlias;
            }
            if (axl.Attributes["captype"] != null)
            {
                switch (axl.Attributes["captype"].Value.ToLower())
                {
                    case "butt":
                        symbol.LineStartCap = symbol.LineEndCap = System.Drawing.Drawing2D.LineCap.Flat;
                        break;
                    case "round":
                        symbol.LineStartCap = symbol.LineEndCap = System.Drawing.Drawing2D.LineCap.Round;
                        break;
                    case "square":
                        symbol.LineStartCap = symbol.LineEndCap = System.Drawing.Drawing2D.LineCap.Square;
                        break;
                    case "anchormask":
                        symbol.LineStartCap = symbol.LineEndCap = System.Drawing.Drawing2D.LineCap.AnchorMask;
                        break;
                    case "arrowanchor":
                        symbol.LineStartCap = symbol.LineEndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
                        break;
                    case "custom":
                        symbol.LineStartCap = symbol.LineEndCap = System.Drawing.Drawing2D.LineCap.Custom;
                        break;
                    case "diamondanchor":
                        symbol.LineStartCap = symbol.LineEndCap = System.Drawing.Drawing2D.LineCap.DiamondAnchor;
                        break;
                    case "noanchor":
                        symbol.LineStartCap = symbol.LineEndCap = System.Drawing.Drawing2D.LineCap.NoAnchor;
                        break;
                    case "roundanchor":
                        symbol.LineStartCap = symbol.LineEndCap = System.Drawing.Drawing2D.LineCap.RoundAnchor;
                        break;
                    case "squareanchor":
                        symbol.LineStartCap = symbol.LineEndCap = System.Drawing.Drawing2D.LineCap.SquareAnchor;
                        break;
                    case "triangle":
                        symbol.LineStartCap = symbol.LineEndCap = System.Drawing.Drawing2D.LineCap.Triangle;
                        break;
                }
            }
            if (axl.Attributes["startcaptype"] != null)
            {
                switch (axl.Attributes["startcaptype"].Value.ToLower())
                {
                    case "butt":
                        symbol.LineStartCap = System.Drawing.Drawing2D.LineCap.Flat;
                        break;
                    case "round":
                        symbol.LineStartCap = System.Drawing.Drawing2D.LineCap.Round;
                        break;
                    case "square":
                        symbol.LineStartCap = System.Drawing.Drawing2D.LineCap.Square;
                        break;
                    case "anchormask":
                        symbol.LineStartCap = System.Drawing.Drawing2D.LineCap.AnchorMask;
                        break;
                    case "arrowanchor":
                        symbol.LineStartCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
                        break;
                    case "custom":
                        symbol.LineStartCap = System.Drawing.Drawing2D.LineCap.Custom;
                        break;
                    case "diamondanchor":
                        symbol.LineStartCap = System.Drawing.Drawing2D.LineCap.DiamondAnchor;
                        break;
                    case "noanchor":
                        symbol.LineStartCap = System.Drawing.Drawing2D.LineCap.NoAnchor;
                        break;
                    case "roundanchor":
                        symbol.LineStartCap = System.Drawing.Drawing2D.LineCap.RoundAnchor;
                        break;
                    case "squareanchor":
                        symbol.LineStartCap = System.Drawing.Drawing2D.LineCap.SquareAnchor;
                        break;
                    case "triangle":
                        symbol.LineStartCap = System.Drawing.Drawing2D.LineCap.Triangle;
                        break;
                }
            }
            if (axl.Attributes["endcaptype"] != null)
            {
                switch (axl.Attributes["endcaptype"].Value.ToLower())
                {
                    case "butt":
                        symbol.LineEndCap = System.Drawing.Drawing2D.LineCap.Flat;
                        break;
                    case "round":
                        symbol.LineEndCap = System.Drawing.Drawing2D.LineCap.Round;
                        break;
                    case "square":
                        symbol.LineEndCap = System.Drawing.Drawing2D.LineCap.Square;
                        break;
                    case "anchormask":
                        symbol.LineEndCap = System.Drawing.Drawing2D.LineCap.AnchorMask;
                        break;
                    case "arrowanchor":
                        symbol.LineEndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
                        break;
                    case "custom":
                        symbol.LineEndCap = System.Drawing.Drawing2D.LineCap.Custom;
                        break;
                    case "diamondanchor":
                        symbol.LineEndCap = System.Drawing.Drawing2D.LineCap.DiamondAnchor;
                        break;
                    case "noanchor":
                        symbol.LineEndCap = System.Drawing.Drawing2D.LineCap.NoAnchor;
                        break;
                    case "roundanchor":
                        symbol.LineEndCap = System.Drawing.Drawing2D.LineCap.RoundAnchor;
                        break;
                    case "squareanchor":
                        symbol.LineEndCap = System.Drawing.Drawing2D.LineCap.SquareAnchor;
                        break;
                    case "triangle":
                        symbol.LineEndCap = System.Drawing.Drawing2D.LineCap.Triangle;
                        break;
                }
            }

            return symbol;
        }
        public static SimplePointSymbol SimpleMarkerSymbol(XmlNode axl)
        {
            if (axl == null) return null;

            SimplePointSymbol symbol = new SimplePointSymbol();
            int A = 255, OA = 255;
            if (axl.Attributes["transparency"] != null)
            {
                A = (int)(double.Parse(axl.Attributes["transparency"].Value.Replace(",", "."), _nhi) * 255.0);
                if (A < 0) A = 0;
                if (A > 255) A = 255;
                OA = A;
            }
            if (axl.Attributes["gv_outlinetransparency"] != null)
            {
                OA = (int)(double.Parse(axl.Attributes["gv_outlinetransparency"].Value.Replace(",", "."), _nhi) * 255.0);
                if (OA < 0) OA = 0;
                if (OA > 255) OA = 255;
            }
            if (axl.Attributes["color"] != null)
                symbol.Color = ColorFromRGB(A, axl.Attributes["color"].Value);
            else if (A != 255)
                symbol.Color = System.Drawing.Color.FromArgb(A, symbol.Color);

            if (axl.Attributes["outline"] != null)
                symbol.OutlineColor = ColorFromRGB(OA, axl.Attributes["outline"].Value);
            else if (A != 255)
                symbol.OutlineColor = System.Drawing.Color.FromArgb(A, symbol.OutlineColor);

            if (axl.Attributes["width"] != null)
                symbol.Size = float.Parse(axl.Attributes["width"].Value.Replace(",", "."), _nhi);
            if (axl.Attributes["symbolwidth"] != null)
                symbol.SymbolWidth = float.Parse(axl.Attributes["symbolwidth"].Value.Replace(",", "."), _nhi);
            if (axl.Attributes["gv_outlinewidth"] != null)
                symbol.PenWidth = float.Parse(axl.Attributes["gv_outlinewidth"].Value.Replace(",", "."), _nhi);

            if (axl.Attributes["type"] != null)
            {
                switch (axl.Attributes["type"].Value.ToString())
                {
                    case "cross":
                        symbol.Marker = SimplePointSymbol.MarkerType.Cross;
                        break;
                    case "square":
                        symbol.Marker = SimplePointSymbol.MarkerType.Square;
                        break;
                    case "star":
                        symbol.Marker = SimplePointSymbol.MarkerType.Star;
                        break;
                    case "triangle":
                        symbol.Marker = SimplePointSymbol.MarkerType.Triangle;
                        break;
                }
            }
            return symbol;
        }
        public static SimpleTextSymbol SimpleTextSymbol(XmlNode axl)
        {
            bool hasAlignment;

            return SimpleTextSymbol(axl, out hasAlignment);
        }
        public static SimpleTextSymbol SimpleTextSymbol(XmlNode axl, out bool hasAlignment)
        {
            hasAlignment = false;
            if (axl == null) return null;

            SimpleTextSymbol symbol = null;

            int A = 255;
            if (axl.Attributes["transparency"] != null)
            {
                A = (int)(double.Parse(axl.Attributes["transparency"].Value.Replace(",", "."), _nhi) * 255.0);
                if (A < 0) A = 0;
                if (A > 255) A = 255;
            }

            if (axl.Attributes["glowing"] != null)
            {
                symbol = new GlowingTextSymbol();
                ((GlowingTextSymbol)symbol).GlowingColor = ColorFromRGB(A, axl.Attributes["glowing"].Value);
                if (axl.Attributes["glowingwidth"] != null)
                {
                    try
                    {
                        ((GlowingTextSymbol)symbol).GlowingWidth = Convert.ToInt32(axl.Attributes["glowingwidth"].Value);
                    }
                    catch { }
                }
                if (axl.Attributes["glowing_antialiasing"] != null &&
                axl.Attributes["glowing_antialiasing"].Value.ToLower().Equals("true"))
                {
                    ((GlowingTextSymbol)symbol).GlowingSmoothingmode = SymbolSmoothing.AntiAlias;
                }
            }
            else
            {
                symbol = new SimpleTextSymbol();
            }



            if (axl.Attributes["fontcolor"] != null)
                symbol.Color = ColorFromRGB(A, axl.Attributes["fontcolor"].Value);

            System.Drawing.FontStyle fontstyle = System.Drawing.FontStyle.Regular;
            if (axl.Attributes["fontstyle"] != null)
            {
                switch (axl.Attributes["fontstyle"].Value)
                {
                    case "italicbold":
                        fontstyle |= System.Drawing.FontStyle.Italic | System.Drawing.FontStyle.Bold;
                        break;
                    case "italic":
                        fontstyle |= System.Drawing.FontStyle.Italic;
                        break;
                    case "bold":
                        fontstyle |= System.Drawing.FontStyle.Bold;
                        break;
                    case "underline":
                        fontstyle |= System.Drawing.FontStyle.Underline;
                        break;
                }
            }

            if (axl.Attributes["font"] != null)
            {
                float size = 10;
                if (axl.Attributes["fontsize"] != null)
                    size = float.Parse(axl.Attributes["fontsize"].Value.Replace(",", "."), _nhi);

                symbol.Font = new System.Drawing.Font(axl.Attributes["font"].Value, size, fontstyle);
            }

            if (axl.Attributes["angle"] != null)
                symbol.Angle = float.Parse(axl.Attributes["angle"].Value.Replace(",", "."), _nhi);

            if (axl.Attributes["halignment"] != null && axl.Attributes["valignment"] != null)
            {
                hasAlignment = true;
                string h = axl.Attributes["halignment"].Value;
                string v = axl.Attributes["valignment"].Value;

                if (h == "left" && v == "top")
                    symbol.TextSymbolAlignment = TextSymbolAlignment.leftAlignOver;
                if (h == "left" && v == "center")
                    symbol.TextSymbolAlignment = TextSymbolAlignment.leftAlignCenter;
                if (h == "left" && v == "bottom")
                    symbol.TextSymbolAlignment = TextSymbolAlignment.leftAlignUnder;

                if (h == "center" && v == "top")
                    symbol.TextSymbolAlignment = TextSymbolAlignment.Over;
                if (h == "center" && v == "center")
                    symbol.TextSymbolAlignment = TextSymbolAlignment.Center;
                if (h == "center" && v == "bottom")
                    symbol.TextSymbolAlignment = TextSymbolAlignment.Under;

                if (h == "right" && v == "top")
                    symbol.TextSymbolAlignment = TextSymbolAlignment.rightAlignOver;
                if (h == "right" && v == "center")
                    symbol.TextSymbolAlignment = TextSymbolAlignment.rightAlignCenter;
                if (h == "right" && v == "bottom")
                    symbol.TextSymbolAlignment = TextSymbolAlignment.rightAlignUnder;
            }
            else if (axl.Attributes["halignment"] != null)
            {
                hasAlignment = true;
                string h = axl.Attributes["halignment"].Value;

                if (h == "left")
                    symbol.TextSymbolAlignment = TextSymbolAlignment.leftAlignOver;
                if (h == "center")
                    symbol.TextSymbolAlignment = TextSymbolAlignment.Over;
                if (h == "right")
                    symbol.TextSymbolAlignment = TextSymbolAlignment.rightAlignOver;
            }
            else if (axl.Attributes["valignment"] != null)
            {
                hasAlignment = true;
                string v = axl.Attributes["valignment"].Value;

                if (v == "top")
                    symbol.TextSymbolAlignment = TextSymbolAlignment.rightAlignOver;
                if (v == "center")
                    symbol.TextSymbolAlignment = TextSymbolAlignment.rightAlignCenter;
                if (v == "bottom")
                    symbol.TextSymbolAlignment = TextSymbolAlignment.rightAlignUnder;
            }

            if (axl.Attributes["antialiasing"] != null &&
                axl.Attributes["antialiasing"].Value.ToLower().Equals("true"))
            {
                symbol.Smoothingmode = SymbolSmoothing.AntiAlias;
            }

            return symbol;
        }
        public static TrueTypeMarkerSymbol TrueTypeMarkerSymbol(XmlNode axl)
        {
            if (axl == null) return null;

            TrueTypeMarkerSymbol symbol = new TrueTypeMarkerSymbol();

            if (axl.Attributes["fontcolor"] != null)
                symbol.Color = ColorFromRGB(255, axl.Attributes["fontcolor"].Value);
            if (axl.Attributes["font"] != null)
            {
                float size = 10;
                if (axl.Attributes["fontsize"] != null)
                    size = float.Parse(axl.Attributes["fontsize"].Value.Replace(",", "."), _nhi);

                symbol.Font = new System.Drawing.Font(axl.Attributes["font"].Value, size);
            }
            if (axl.Attributes["character"] != null)
                try
                {
                    symbol.Charakter = (byte)Convert.ToInt16(axl.Attributes["character"].Value);
                }
                catch { }

            if (axl.Attributes["angle"] != null)
            {
                try
                {
                    symbol.Angle = float.Parse(axl.Attributes["angle"].Value);
                }
                catch { }
            }

            if (axl.Attributes["hotspot"] != null)
            {
                try
                {
                    string[] hs = axl.Attributes["hotspot"].Value.Split(',');
                    if (hs.Length == 2)
                    {
                        float hs_x = float.Parse(hs[0], _nhi);
                        float hs_y = float.Parse(hs[1], _nhi);

                        //if (symbol.Angle != 0f)
                        //{
                        //    float x = (float)(hs_x * Math.Cos(symbol.Angle * Math.PI / 180.0) + hs_y * Math.Sin(symbol.Angle * Math.PI / 180.0));
                        //    float y = (float)(-hs_x * Math.Sin(symbol.Angle * Math.PI / 180.0) + hs_y * Math.Cos(symbol.Angle * Math.PI / 180.0));

                        //    hs_x = x;
                        //    hs_y = y;
                        //}
                        //symbol.HorizontalOffset = hs_x;
                        //symbol.VerticalOffset = -hs_y;
                        symbol.HorizontalOffset = hs_x;
                        symbol.VerticalOffset = hs_y;
                    }
                }
                catch { }
            }

            return symbol;
        }

        public static RasterMarkerSymbol RasterMarkerSymbol(XmlNode axl)
        {
            if (axl == null || axl.Attributes["image"] == null) return null;

            RasterMarkerSymbol symbol = new RasterMarkerSymbol();
            symbol.Filename = axl.Attributes["image"].Value;

            if (axl.Attributes["size"] != null)
            {
                string[] xy = axl.Attributes["size"].Value.Split(',');
                if (xy.Length == 2)
                {
                    float sizeX, sizeY;
                    if (float.TryParse(xy[0], System.Globalization.NumberStyles.Any, _nhi, out sizeX))
                        symbol.SizeX = sizeX;

                    if (float.TryParse(xy[1], System.Globalization.NumberStyles.Any, _nhi, out sizeY))
                        symbol.SizeY = sizeY;
                }
            }
            return symbol;
        }

        private static System.Drawing.Color ColorFromRGB(int A, string col)
        {
            string[] rgb = col.Split(',');
            if (rgb.Length < 3) return System.Drawing.Color.Transparent;
            return System.Drawing.Color.FromArgb(A,
                Convert.ToInt32(rgb[0]),
                Convert.ToInt32(rgb[1]),
                Convert.ToInt32(rgb[2]));
        }

        private static double Scale(string scaleString)
        {
            if (String.IsNullOrEmpty(scaleString))
                return 0.0;

            if (scaleString.Contains(":"))
                return double.Parse(scaleString.Split(':')[1].Replace(",", "."), _nhi);

            return double.Parse(scaleString.Replace(",", "."), _nhi);
        }

        public static IGraphicElement GraphicElement(XmlNode axl)
        {
            if (axl == null) return null;
            ISymbol symbol = null;
            IGeometry geometry = null;

            foreach (XmlNode node in axl.ChildNodes)
            {
                if (node.Name == "POINT" ||
                    node.Name == "MULTIPOINT" ||
                    node.Name == "POLYLINE" ||
                    node.Name == "POLYGON")
                {
                    geometry = Geometry(node.OuterXml);

                    // manchmal steht symbol auch innerhalb des Points...
                    if (symbol == null)
                    {
                        foreach (XmlNode child in node.ChildNodes)
                        {
                            symbol = SimpleSymbol(child);
                            if (symbol != null) break;
                        }
                    }
                }
                else if (node.Name == "TEXT" && node.Attributes["coords"] != null && node.Attributes["label"] != null)
                {
                    double x, y;
                    string[] xy = node.Attributes["coords"].Value.Split(' ');
                    if (xy.Length == 2)
                    {
                        if (double.TryParse(xy[0].Replace(",", "."), NumberStyles.Any, _nhi, out x) &&
                            double.TryParse(xy[1].Replace(",", "."), NumberStyles.Any, _nhi, out y))
                        {
                            geometry = new Point(x, y);
                        }
                    }
                    // manchmal steht symbol auch innerhalb des Points...
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        symbol = SimpleSymbol(child);
                        if (symbol != null) break;
                    }

                    if (symbol is ILabel)
                    {
                        ((ILabel)symbol).Text = node.Attributes["label"].Value;
                        ((ILabel)symbol).TextSymbolAlignment = TextSymbolAlignment.leftAlignOver;
                    }
                }
                else
                {
                    symbol = SimpleSymbol(node);
                }

                if (geometry != null && symbol != null) break;
            }
            if (symbol != null && geometry != null)
                return new AcetateGraphicElement(symbol, geometry);

            return null;
        }
        public static IGraphicElement GraphicElement(ISymbol symbol, IBufferQueryFilter filter)
        {
            if (symbol == null) return null;
            ISpatialFilter sFilter = BufferQueryFilter.ConvertToSpatialFilter(filter);
            if (sFilter == null || sFilter.Geometry == null) return null;

            return new AcetateGraphicElement(symbol, sFilter.Geometry);
        }

        private class AcetateGraphicElement : IGraphicElement
        {
            private ISymbol _symbol;
            private IGeometry _geometry;

            public AcetateGraphicElement(ISymbol symbol, IGeometry geometry)
            {
                _symbol = symbol;
                _geometry = geometry;
            }

            #region IGraphicElement Member

            public void Draw(IDisplay display)
            {
                if (_symbol == null || _geometry == null) return;

                display.Draw(_symbol, _geometry);
            }

            #endregion
        }

        private static string CheckEsc(string str)
        {
            return str.Replace("&", "&amp;").Replace("\"", "&quot;").Replace("<", "&lt;").Replace(">", "&gt;");
        }
    }

    public class AXLFromObjectFactory
    {
        public static string Renderers(IFeatureLayer layer)
        {
            if (layer == null) return "";

            return "";
        }

        public static string FeatureRenderer(IFeatureRenderer renderer)
        {
            if (renderer is SimpleRenderer)
            {

            }
            return "";
        }

        static public string Geometry(IGeometry geom)
        {
            return ArcXMLGeometry.Geometry2AXL(geom);
        }

        static public string Query(IQueryFilter filter)
        {
            if (filter == null) return "";

            if (filter is ISpatialFilter)
                return SpatialQueryToAXL((ISpatialFilter)filter);
            else
                return QueryToAXL(filter);

            return "";
        }
        private static string QueryToAXL(IQueryFilter filter)
        {
            if (filter == null) return "";

            try
            {
                AXLWriter axl = new AXLWriter();

                axl.WriteStartElement("QUERY");
                if (filter.SubFields == "" || filter.SubFields == "*")
                    axl.WriteAttribute("subfields", "#ALL#");
                else
                {
                    axl.WriteAttribute("subfields", filter.SubFields);
                }
                string where = (filter is IRowIDFilter) ? ((IRowIDFilter)filter).RowIDWhereClause : filter.WhereClause;
                if (where != "")
                {
                    axl.WriteAttribute("where", where);
                }

                if (filter.FeatureSpatialReference != null)
                {
                    axl.WriteStartElement("FEATURECOORDSYS");
                    if (filter.FeatureSpatialReference.Name.ToLower().StartsWith("epsg:"))
                    {
                        axl.WriteAttribute("id", filter.FeatureSpatialReference.Name.Split(':')[1]);
                    }
                    else
                    {
                        axl.WriteAttribute("string", gView.Framework.Geometry.SpatialReference.ToESRIWKT(filter.FeatureSpatialReference));
                    }
                    if (filter.FeatureSpatialReference.Datum != null)
                    {
                        axl.WriteAttribute("datumtransformstring", gView.Framework.Geometry.SpatialReference.ToESRIGeotransWKT(filter.FeatureSpatialReference));
                    }
                    axl.WriteEndElement();
                }

                axl.WriteEndElement(); // QUERY

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(axl.Request);
                return doc.SelectSingleNode("//QUERY").OuterXml;
            }
            catch
            {
                return "";
            }
        }
        private static string SpatialQueryToAXL(ISpatialFilter filter)
        {
            if (filter == null) return "";
            try
            {
                AXLWriter axl = new AXLWriter();
                axl.WriteStartElement("SPATIALQUERY");

                string where = (filter is IRowIDFilter) ? ((IRowIDFilter)filter).RowIDWhereClause : filter.WhereClause;
                if (where != "")
                {
                    if (filter.SubFields == "" || filter.SubFields == "*")
                        axl.WriteAttribute("subfields", "#ALL#");
                    else
                    {
                        axl.WriteAttribute("subfields", filter.SubFields);
                    }
                    axl.WriteAttribute("where", (filter is IRowIDFilter) ? ((IRowIDFilter)filter).RowIDWhereClause : filter.WhereClause);
                }
                else
                {
                    axl.WriteAttribute("subfields", (filter.SubFields == "*") ? "#ALL#" : filter.SubFields);
                }

                if (filter.FilterSpatialReference != null || filter.FeatureSpatialReference != null)
                {
                    if (filter.FilterSpatialReference != null)
                    {
                        axl.WriteStartElement("FILTERCOORDSYS");
                        if (filter.FilterSpatialReference.Name.ToLower().StartsWith("epsg:"))
                        {
                            axl.WriteAttribute("id", filter.FilterSpatialReference.Name.Split(':')[1]);
                        }
                        else
                        {
                            axl.WriteAttribute("string", gView.Framework.Geometry.SpatialReference.ToESRIWKT(filter.FilterSpatialReference));
                        }
                        if (filter.FilterSpatialReference.Datum != null)
                        {
                            axl.WriteAttribute("datumtransformstring", gView.Framework.Geometry.SpatialReference.ToESRIGeotransWKT(filter.FilterSpatialReference));
                        }
                        axl.WriteEndElement();
                    }
                    if (filter.FeatureSpatialReference != null)
                    {
                        axl.WriteStartElement("FEATURECOORDSYS");
                        if (filter.FeatureSpatialReference.Name.ToLower().StartsWith("epsg:"))
                        {
                            axl.WriteAttribute("id", filter.FeatureSpatialReference.Name.Split(':')[1]);
                        }
                        else
                        {
                            axl.WriteAttribute("string", gView.Framework.Geometry.SpatialReference.ToESRIWKT(filter.FeatureSpatialReference));
                        }
                        if (filter.FeatureSpatialReference.Datum != null)
                        {
                            axl.WriteAttribute("datumtransformstring", gView.Framework.Geometry.SpatialReference.ToESRIGeotransWKT(filter.FeatureSpatialReference));
                        }
                        axl.WriteEndElement();
                    }
                }

                else if (filter.FilterSpatialReference != null)
                {
                    // SPATIALREFERENCE
                    axl.WriteStartElement("SPATIALREFERENCE");
                    axl.WriteAttribute("name", filter.FilterSpatialReference.Name);
                    axl.WriteAttribute("param", gView.Framework.Geometry.SpatialReference.ToProj4(filter.FilterSpatialReference));
                    axl.WriteEndElement();

                    // FILTERCOORDSYS
                    axl.WriteStartElement("FILTERCOORDSYS");
                    axl.WriteAttribute("string", gView.Framework.Geometry.SpatialReference.ToESRIWKT(filter.FilterSpatialReference));
                    axl.WriteEndElement();
                }
                if (filter.Geometry != null)
                {
                    axl.WriteStartElement("SPATIALFILTER");
                    axl.WriteAttribute("relation", "area_intersection");

                    axl.WriteRaw(ArcXMLGeometry.Geometry2AXL(filter.Geometry));

                    axl.WriteEndElement(); // SPATIALFILTER
                }

                axl.WriteEndElement(); // SPATIALQUERY

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(axl.Request);
                return doc.SelectSingleNode("//SPATIALQUERY").OuterXml;
            }
            catch
            {
                return "";
            }
        }

        #region Helper Classes
        private class AXLWriter
        {
            protected StringBuilder m_axl;
            //protected StringWriter m_sw;
            protected MemoryStream m_ms;
            protected XmlTextWriter m_xWriter;
            protected Encoding m_encoding = Encoding.UTF8;

            public AXLWriter()
                : this(Encoding.UTF8)
            {
            }

            public AXLWriter(Encoding encoding)
            {
                m_axl = new StringBuilder();
                //m_sw = new StringWriter(m_axl);
                m_ms = new MemoryStream();
                m_xWriter = new XmlTextWriter(m_ms, m_encoding = encoding);
                m_xWriter.WriteStartDocument();
            }

            public string Request
            {
                get
                {
                    //m_xWriter.WriteEndDocument();
                    //m_xWriter.Close();
                    //m_sw.Close();
                    //return m_axl.ToString();

                    m_xWriter.WriteEndDocument();
                    m_xWriter.Flush();
                    m_ms.Flush();

                    m_ms.Position = 0;
                    StreamReader sr = new StreamReader(m_ms);
                    m_axl.Append(sr.ReadToEnd());
                    sr.Close();
                    m_ms.Close();
                    m_xWriter.Close();

                    return m_axl.ToString();
                }
            }

            public XmlTextWriter xmlWriter
            {
                get
                {
                    return m_xWriter;
                }
            }

            public void WriteStartElement(string element)
            {
                m_xWriter.WriteStartElement(element);
            }
            public void WriteEndElement()
            {
                m_xWriter.WriteEndElement();
            }
            public void WriteRaw(string raw)
            {
                m_xWriter.WriteRaw(raw);
            }
            public void WriteAttribute(string tag, string val)
            {
                m_xWriter.WriteAttributeString(tag, val);
            }
        }
        #endregion
    }

}
