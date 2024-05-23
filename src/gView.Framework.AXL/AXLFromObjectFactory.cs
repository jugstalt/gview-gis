using gView.Framework.Cartography.Rendering;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.XML;
using System.IO;
using System.Text;
using System.Xml;

namespace gView.Framework.AXL
{
    public class AXLFromObjectFactory
    {
        public static string Renderers(IFeatureLayer layer)
        {
            if (layer == null)
            {
                return "";
            }

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
            if (filter == null)
            {
                return "";
            }

            if (filter is ISpatialFilter)
            {
                return SpatialQueryToAXL((ISpatialFilter)filter);
            }
            else
            {
                return QueryToAXL(filter);
            }
        }
        private static string QueryToAXL(IQueryFilter filter)
        {
            if (filter == null)
            {
                return "";
            }

            try
            {
                AXLWriter axl = new AXLWriter();

                axl.WriteStartElement("QUERY");
                if (filter.SubFields == "" || filter.SubFields == "*")
                {
                    axl.WriteAttribute("subfields", "#ALL#");
                }
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
            if (filter == null)
            {
                return "";
            }

            try
            {
                AXLWriter axl = new AXLWriter();
                axl.WriteStartElement("SPATIALQUERY");

                string where = (filter is IRowIDFilter) ? ((IRowIDFilter)filter).RowIDWhereClause : filter.WhereClause;
                if (where != "")
                {
                    if (filter.SubFields == "" || filter.SubFields == "*")
                    {
                        axl.WriteAttribute("subfields", "#ALL#");
                    }
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
