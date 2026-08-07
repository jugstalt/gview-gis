using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Data;
using gView.Framework.Db.Extensions;
using gView.Framework.Geometry;
using gView.Framework.Common;
using gView.Framework.Common.Diagnostics;
using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using gView.Framework.Common.Extensions;

namespace gView.Framework.OGC.DB
{
    class OgcSpatialFeatureCursor : FeatureCursor
    {
        private DbConnection _conn = null;
        private DbDataReader _reader = null;
        private DbCommand _command = null;
        private ISpatialFilter _spatialfilter = null;
        private string[] _subFields = null;
        private string _shapeField = "", _idField = "";
        OgcSpatialFeatureclass _fc = null;
        private int _generatedOid = 0;

        private OgcSpatialFeatureCursor(OgcSpatialFeatureclass fc, IQueryFilter filter)
            : base(fc, 
                   fc?.SpatialReference,
                   filter?.FeatureSpatialReference, 
                   filter?.DatumTransformations)
        {
            base.CancelTracker = filter?.CancelTracker;
            base.DiagnosticParameters = SystemVariables.UseDiagnostic ?
                                                new DiagnosticParameters() : null;
        }

        async static public Task<IFeatureCursor> Create(OgcSpatialFeatureclass fc, IQueryFilter filter)
        {
            var featureCursor = new OgcSpatialFeatureCursor(fc, filter);
            featureCursor._fc = fc;   // was never assigned before - LastException on the feature class stayed empty forever

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
                         fc.SpatialReference,
                         filter.DatumTransformations);
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

                command.SetCustomCursorTimeout();

                //featureCursor._reader = await command.ExecuteReaderAsync();
                //featureCursor._reader = await featureCursor._diagnostics.StopIdleTimeAsync(command.ExecuteReaderAsync());
                featureCursor._command = command;
                featureCursor._reader = await (command.ExecuteReaderAsync().StopIdleAsync(featureCursor.DiagnosticParameters));

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
                    if (_reader == null || !await _reader.ReadAsync().StopIdleAsync(base.DiagnosticParameters))
                    { 
                        _command = null; // reader finished, avoid _command.cancel() on dispose

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
                                //_spatialfilter.IgnoreFeatureCursorCheckIntersection != true &&   // experimental
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
                            feature.OID = ResolveOid(obj);
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

        /// <summary>
        /// Resolves the feature id for a row's id-column value: the real database value when
        /// possible, otherwise a generated (always negative) id so the feature loads instead of
        /// silently vanishing (this used to throw a FormatException per row that got swallowed
        /// in NextFeature()'s catch block, so the whole layer ended up empty without any visible
        /// error).
        /// </summary>
        private int ResolveOid(object idColumnValue)
        {
            // HasIntegerIdField only promises the *column* is numeric, not that every single
            // value converts cleanly (NULL, overflow, ...) - TryConvertToOid() never throws,
            // so there's no try/catch needed on this hot path either way.
            bool idFieldIsNumeric = _fc != null && _fc.HasIntegerIdField;

            if (idFieldIsNumeric && TryConvertToOid(idColumnValue, out int oid))
            {
                return oid;
            }

            if (idFieldIsNumeric)
            {
                // Schema said this column is numeric, but this particular value wasn't - keep
                // it visible instead of just losing the feature silently.
                _fc.LastException = new InvalidCastException(
                    $"Could not convert id value '{idColumnValue}' (field '{_idField}') to an integer feature id.");
            }

            // Generated ids count DOWN from -1 (never 0/positive) so they can never collide with
            // a real db id and are trivially recognizable as synthetic (real serial/bigserial/oid
            // values are always >= 0).
            return --_generatedOid;
        }

        /// <summary>
        /// Converts a db value known to come from an integer/oid column to an int feature id,
        /// without relying on exceptions for the (expected-to-be-rare) failure case - this runs
        /// per row/per feature, so throwing here for e.g. a NULL id would be a real hot-path cost.
        /// </summary>
        private static bool TryConvertToOid(object obj, out int oid)
        {
            switch (obj)
            {
                case int i32:
                    oid = i32;
                    return true;
                case short i16:
                    oid = i16;
                    return true;
                case long i64 when i64 >= int.MinValue && i64 <= int.MaxValue:
                    oid = (int)i64;
                    return true;
                case uint u32 when u32 <= int.MaxValue:  // PostgreSQL "oid"
                    oid = (int)u32;
                    return true;
                case null:
                case DBNull:
                    oid = 0;
                    return false;
                default:
                    // Unexpected provider type (shouldn't normally happen given HasIntegerIdField) -
                    // still avoid a throw, int.TryParse doesn't.
                    return int.TryParse(
                        Convert.ToString(obj, System.Globalization.CultureInfo.InvariantCulture),
                        out oid);
            }
        }

        #region IDisposable Member

        private object lockThis = new object();
        public override void Dispose()
        {
            base.Dispose();
            //lock (lockThis)
            {
                if (_reader is not null)
                {
                    if (_command is not null)
                    {
                        _command.Cancel();
                        _reader.Close();

                        _command.Dispose();
                        _command = null;
                    }

                    _reader = null;
                }
                if (_conn is not null && _conn.State != ConnectionState.Closed)
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
