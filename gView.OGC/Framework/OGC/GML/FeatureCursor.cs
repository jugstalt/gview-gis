using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using System.Xml;
using gView.Framework.OGC.WFS;
using gView.Framework.Geometry;
using gView.Framework.OGC.WFS;

namespace gView.Framework.OGC.GML
{
    public class FeatureCursor : gView.Framework.Data.FeatureCursor
    {
        private XmlNodeList _features = null;
        private XmlNamespaceManager _ns;
        private IQueryFilter _filter;
        private int _pos = 0;
        private IFeatureClass _fc;
        private bool _checkGeometryRelation = true;
        private GmlVersion _gmlVersion;

        public FeatureCursor(IFeatureClass fc, XmlNode featureCollection, XmlNamespaceManager ns, IQueryFilter filter, GmlVersion gmlVersion)
            : base((fc != null) ? fc.SpatialReference : null,
                   (filter != null) ? filter.FeatureSpatialReference : null)
        {
            _ns = ns;
            _filter = filter;
            _fc = fc;
            _gmlVersion = gmlVersion;

            if (featureCollection == null || ns == null || fc == null) return;

            try
            {
                _features = featureCollection.SelectNodes("GML:featureMember/myns:" + XML.Globals.TypeWithoutPrefix(fc.Name), ns);
            }
            catch
            {
                _features = null;
            }
        }
        public FeatureCursor(IFeatureClass fc, XmlNode featureCollection, XmlNamespaceManager ns, IQueryFilter filter, GmlVersion gmlVersion, Filter_Capabilities filterCapabilities)
            : this(fc, featureCollection, ns, filter, gmlVersion)
        {
            //
            // wenn Filter schon geometry operation implementiert
            // ist es hier nicht noch einmal zu vergleichen...
            //
            if (filterCapabilities != null &&
                _filter is ISpatialFilter &&
                filterCapabilities.SupportsSpatialOperator(((ISpatialFilter)_filter).SpatialRelation))
            {
                _checkGeometryRelation = false;
            }
        }

        #region IFeatureCursor Member

        public override IFeature NextFeature
        {
            get
            {
                while (true)
                {
                    if (_features == null || _pos >= _features.Count) return null;
                    XmlNode featureNode = _features[_pos++];

                    Feature feature = new Feature();
                    if (featureNode.Attributes["fid"] != null)
                    {
                        feature.OID = XML.Globals.IntegerFeatureID(featureNode.Attributes["fid"].Value);
                    }
                    foreach (XmlNode fieldNode in featureNode.SelectNodes("myns:*", _ns))
                    {
                        string fieldName = fieldNode.Name.Split(':')[1];

                        if (fieldName == _fc.ShapeFieldName.Replace("#", ""))
                        {
                            feature.Shape = GeometryTranslator.GML2Geometry(fieldNode.InnerXml, _gmlVersion);
                        }
                        else
                        {
                            FieldValue fv = new FieldValue(fieldName, fieldNode.InnerText);
                            feature.Fields.Add(fv);

                            try
                            {
                                if (fieldName == _fc.IDFieldName)
                                    feature.OID = Convert.ToInt32(fieldNode.InnerText);
                            }
                            catch { }
                        }
                    }

                    if (feature.Shape == null)
                    {
                        foreach (XmlNode gmlNode in featureNode.SelectNodes("GML:*", _ns))
                        {
                            feature.Shape = GeometryTranslator.GML2Geometry(gmlNode.OuterXml, _gmlVersion);
                            if (feature.Shape != null) break;
                        }
                    }

                    if (feature.Shape != null &&
                        _filter is ISpatialFilter &&
                        _checkGeometryRelation)
                    {
                        if (!SpatialRelation.Check(_filter as ISpatialFilter, feature.Shape))
                            continue;
                    }

                    Transform(feature);
                    return feature;
                }
            }
        }

        #endregion

        #region IDisposable Member

        public override void Dispose()
        {
            base.Dispose();
        }

        #endregion
    }

    public class FeatureCursor2 : gView.Framework.Data.FeatureCursor
    {
        private XmlTextReader _reader = null;
        private XmlNamespaceManager _ns;
        private IQueryFilter _filter;
        private IFeatureClass _fc;
        private bool _checkGeometryRelation = true;
        private GmlVersion _gmlVersion;

        public FeatureCursor2(IFeatureClass fc, XmlTextReader reader, XmlNamespaceManager ns, IQueryFilter filter, GmlVersion gmlVersion)
            : base((fc != null) ? fc.SpatialReference : null,
                   (filter != null) ? filter.FeatureSpatialReference : null)
        {
            _ns = ns;
            _filter = filter;
            _fc = fc;
            _gmlVersion = gmlVersion;

            if (reader == null || ns == null || fc == null) return;

            try
            {
                _reader = reader;
            }
            catch
            {
                _reader = null;
            }
        }
        public FeatureCursor2(IFeatureClass fc, XmlTextReader reader, XmlNamespaceManager ns, IQueryFilter filter, GmlVersion gmlVersion, Filter_Capabilities filterCapabilities)
            : this(fc, reader, ns, filter, gmlVersion)
        {
            //
            // wenn Filter schon geometry operation implementiert
            // ist es hier nicht noch einmal zu vergleichen...
            //
            if (filterCapabilities != null &&
                _filter is ISpatialFilter &&
                filterCapabilities.SupportsSpatialOperator(((ISpatialFilter)_filter).SpatialRelation))
            {
                _checkGeometryRelation = false;
            }
        }

        #region IFeatureCursor Member

        public override IFeature NextFeature
        {
            get
            {
                while (true)
                {
                    if (_reader == null) return null;

                    if (!_reader.ReadToFollowing(_fc.Name, _ns.LookupNamespace("myns")))
                        return null;
                    string featureString = _reader.ReadOuterXml();

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(featureString);
                    XmlNode featureNode = doc.ChildNodes[0];

                    Feature feature = new Feature();
                    if (featureNode.Attributes["fid"] != null)
                    {
                        feature.OID = XML.Globals.IntegerFeatureID(featureNode.Attributes["fid"].Value);
                    }
                    foreach (XmlNode fieldNode in featureNode.SelectNodes("myns:*", _ns))
                    {
                        string fieldName = fieldNode.Name.Split(':')[1];

                        if (fieldName == _fc.ShapeFieldName.Replace("#", ""))
                        {
                            feature.Shape = GeometryTranslator.GML2Geometry(fieldNode.InnerXml, _gmlVersion);
                        }
                        else
                        {
                            FieldValue fv = new FieldValue(fieldName, fieldNode.InnerText);
                            feature.Fields.Add(fv);

                            try
                            {
                                if (fieldName == _fc.IDFieldName)
                                    feature.OID = Convert.ToInt32(fieldNode.InnerText);
                            }
                            catch { }
                        }
                    }

                    if (feature.Shape == null)
                    {
                        foreach (XmlNode gmlNode in featureNode.SelectNodes("GML:*", _ns))
                        {
                            feature.Shape = GeometryTranslator.GML2Geometry(gmlNode.OuterXml, _gmlVersion);
                            if (feature.Shape != null) break;
                        }
                    }

                    if (feature.Shape != null &&
                        _filter is ISpatialFilter &&
                        _checkGeometryRelation)
                    {
                        if (!SpatialRelation.Check(_filter as ISpatialFilter, feature.Shape))
                            continue;
                    }

                    Transform(feature);
                    return feature;
                }
            }
        }

        #endregion

        #region IDisposable Member

        public override void Dispose()
        {
            base.Dispose();
            if (_reader != null)
                _reader.Close();
            _reader = null;
        }

        #endregion
    }
}
