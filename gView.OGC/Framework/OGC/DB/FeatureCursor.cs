using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.system;
using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Framework.OGC.DB
{
    class OgcSpatialFeatureCursor : FeatureCursor
    {
        private DbConnection _conn = null;
        private DbDataReader _reader = null;
        //private OleDbConnection _conn = null;
        //private OleDbDataReader _reader = null;
        private ISpatialFilter _spatialfilter = null;
        private string[] _subFields = null;
        private string _shapeField = "", _idField = "";
        OgcSpatialFeatureclass _fc = null;

        private OgcSpatialFeatureCursor(OgcSpatialFeatureclass fc, IQueryFilter filter)
            : base((fc != null) ? fc.SpatialReference : null,
                   (filter != null) ? filter.FeatureSpatialReference : null)
        {
            base.CancelTracker = filter?.CancelTracker;
        }

        async static public Task<IFeatureCursor> Create(OgcSpatialFeatureclass fc, IQueryFilter filter)
        {
            var featureCursor = new OgcSpatialFeatureCursor(fc, filter);

            if (fc == null || fc.Dataset == null)
            {
                return featureCursor;
            }

            featureCursor._idField = fc.IDFieldName;
            if (filter is ISpatialFilter)
            {
                featureCursor._spatialfilter = (ISpatialFilter)filter;
            }

            try
            {
                if (fc.SpatialReference != null &&
                    filter is ISpatialFilter &&
                    ((ISpatialFilter)filter).FilterSpatialReference != null &&
                    !((ISpatialFilter)filter).FilterSpatialReference.Equals(fc.SpatialReference))
                {
                    filter = (ISpatialFilter)filter.Clone();

                    ((ISpatialFilter)filter).Geometry =
                        GeometricTransformerFactory.Transform2D(((ISpatialFilter)filter).Geometry,
                         ((ISpatialFilter)filter).FilterSpatialReference,
                         fc.SpatialReference);
                    ((ISpatialFilter)filter).FilterSpatialReference = null;
                    if (((ISpatialFilter)filter).SpatialRelation == spatialRelation.SpatialRelationMapEnvelopeIntersects &&
                       ((ISpatialFilter)filter).Geometry != null)
                    {
                        ((ISpatialFilter)filter).Geometry = ((ISpatialFilter)filter).Geometry.Envelope;
                    }

                    featureCursor._spatialfilter = (ISpatialFilter)filter;
                }
                DbCommand command = ((OgcSpatialDataset)fc.Dataset).SelectCommand(
                    fc, filter, out featureCursor._shapeField);
                if (command == null)
                {
                    return featureCursor;
                }

                featureCursor._subFields = filter.QuerySubFields.ToArray();

                featureCursor._conn = ((OgcSpatialDataset)fc.Dataset).ProviderFactory.CreateConnection();
                featureCursor._conn.ConnectionString = fc.Dataset.ConnectionString;

                command.Connection = featureCursor._conn;

                if (featureCursor._conn.State != ConnectionState.Closed)
                {
                    try
                    {
                        featureCursor._conn.Close();
                    }
                    catch { }
                }
                await featureCursor._conn.OpenAsync();

                command.CommandTimeout = 60 * 2;
                featureCursor._reader = await command.ExecuteReaderAsync();

                return featureCursor;
            }
            catch (Exception ex)
            {
                if (featureCursor._fc != null)
                {
                    featureCursor._fc.LastException = ex;
                }

                if (featureCursor._conn != null && featureCursor._conn.State != ConnectionState.Closed)
                {
                    featureCursor._conn.Close();
                    featureCursor._conn = null;
                }

                fc.LastException = ex;

                return null;
            }
        }

        #region IFeatureCursor Member

        async public override Task<IFeature> NextFeature()
        {
            while (true)
            {
                try
                {
                    if (_reader == null || !await _reader.ReadAsync())
                    {
                        return null;
                    }

                    Feature feature = new Feature();
                    for (int i = 0; i < _reader.FieldCount; i++)
                    {
                        string fieldname = _reader.GetName(i).OrTake(_subFields != null && _subFields.Length == _reader.FieldCount ? _subFields[i] : "");   // Functions Shape.STArea() as no fieldname
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

                    if (feature == null)
                    {
                        continue;
                    }

                    Transform(feature);
                    return feature;
                }
                catch (Exception ex)
                {
                    if (_fc != null)
                    {
                        _fc.LastException = ex;
                    }

                    //string errMsg = ex.Message;
                    //return null;
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
                    //_reader.Close();
                    //try
                    //{
                    //    while (_reader.Read())
                    //    {
                    //        //_command.Cancel();
                    //    }
                    //}
                    //catch (Exception)
                    //{

                    //}
                    //_reader.Dispose();
                    _reader = null;
                }
                if (_conn != null && _conn.State != ConnectionState.Closed)
                {
                    _conn.Close();
                    _conn.Dispose();
                    _conn = null;
                }
            }
        }

        #endregion
    }
}
