using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using gView.Framework.Data;
using gView.Framework.Geometry;
using System.Data.Common;

namespace gView.Framework.OGC.DB
{
    class OgcSpatialFeatureCursor : FeatureCursor
    {
        private DbConnection _conn = null;
        private DbDataReader _reader = null;
        //private OleDbConnection _conn = null;
        //private OleDbDataReader _reader = null;
        private ISpatialFilter _spatialfilter = null;
        private string _shapeField = "", _idField = "";
        OgcSpatialFeatureclass _fc = null;

        public OgcSpatialFeatureCursor(OgcSpatialFeatureclass fc, IQueryFilter filter)
            : base((fc != null) ? fc.SpatialReference : null,
                   (filter != null) ? filter.FeatureSpatialReference : null)
        {
            if (fc == null || fc.Dataset == null) return;

            _idField = fc.IDFieldName;
            if (filter is ISpatialFilter)
                _spatialfilter = (ISpatialFilter)filter;

            try
            {
                if (fc.SpatialReference != null &&
                    filter is ISpatialFilter &&
                    ((ISpatialFilter)filter).FilterSpatialReference != null &&
                    !((ISpatialFilter)filter).FilterSpatialReference.Equals(fc.SpatialReference))
                {
                    filter = (ISpatialFilter)filter.Clone();

                    ((ISpatialFilter)filter).Geometry =
                        GeometricTransformer.Transform2D(((ISpatialFilter)filter).Geometry,
                         ((ISpatialFilter)filter).FilterSpatialReference,
                         fc.SpatialReference);
                    ((ISpatialFilter)filter).FilterSpatialReference = null;
                    if (((ISpatialFilter)filter).SpatialRelation == spatialRelation.SpatialRelationMapEnvelopeIntersects &&
                       ((ISpatialFilter)filter).Geometry != null)
                        ((ISpatialFilter)filter).Geometry = ((ISpatialFilter)filter).Geometry.Envelope;

                    _spatialfilter = (ISpatialFilter)filter;
                }
                DbCommand command = ((OgcSpatialDataset)fc.Dataset).SelectCommand(
                    fc, filter, out _shapeField);
                if (command == null) return;

                _conn = ((OgcSpatialDataset)fc.Dataset).ProviderFactory.CreateConnection();
                _conn.ConnectionString = fc.Dataset.ConnectionString;

                command.Connection = _conn;

                if (_conn.State != ConnectionState.Closed)
                {
                    try
                    {
                        _conn.Close();
                    }
                    catch { }
                }
                _conn.Open();
                //command.Prepare();

                _reader = command.ExecuteReader();
            }
            catch (Exception ex)
            {
                if (_fc != null)
                    _fc.LastException = ex;

                if (_conn != null && _conn.State != ConnectionState.Closed)
                {
                    _conn.Close();
                    _conn = null;
                }
            }
        }

        #region IFeatureCursor Member

        public override IFeature NextFeature
        {
            get
            {

                while (true)
                {
                    try
                    {
                        if (_reader == null || !_reader.Read()) return null;

                        Feature feature = new Feature();
                        for (int i = 0; i < _reader.FieldCount; i++)
                        {
                            string fieldname = _reader.GetName(i);
                            object obj = _reader.GetValue(i);

                            if (fieldname == _shapeField)
                            {
                                feature.Shape = gView.Framework.OGC.OGC.WKBToGeometry((byte[])obj);

                                if (_spatialfilter != null &&
                                    _spatialfilter.SpatialRelation != spatialRelation.SpatialRelationMapEnvelopeIntersects)
                                {
                                    if (!gView.Framework.Geometry.SpatialRelation.Check(_spatialfilter, feature.Shape))
                                    {
                                        feature = null;
                                        break;
                                    }
                                }
                            }
                            else if (fieldname == _idField)
                            {
                                feature.Fields.Add(new FieldValue(fieldname, obj));
                                feature.OID = Convert.ToInt32(obj);
                            }
                            else
                            {
                                feature.Fields.Add(new FieldValue(fieldname, obj));
                            }
                        }

                        if (feature == null) continue;

                        Transform(feature);
                        return feature;
                    }
                    catch (Exception ex)
                    {
                        if (_fc != null)
                            _fc.LastException = ex;
                        //string errMsg = ex.Message;
                        //return null;
                    }
                }
            }
        }

        #endregion

        #region IDisposable Member

        private object lockThis = new object();
        public override void Dispose()
        {
            base.Dispose();
            //lock (lockThis)
            {
                if (_reader != null)
                {
                    _reader.Close();
                    try
                    {
                        while (_reader.Read())
                        {
                            //_command.Cancel();
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    _reader.Dispose();
                    _reader = null;
                }
                if (_conn != null && _conn.State != ConnectionState.Closed)
                {
                    _conn.Close();
                    _conn = null;
                }
            }
        }

        #endregion
    }
}
