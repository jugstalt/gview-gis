using gView.DataSources.Fdb;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Db.Extensions;
using System;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace gView.DataSources.Fdb.SQLite.Cursors
{
    /// <summary>
    /// Shared lifecycle and row materialization for the FDB feature cursors. Each concrete cursor
    /// only decides how the SQL statement is built and whether it needs cursor-side paging /
    /// geometry post-filtering; everything else (connection, reader, <c>FDB_SHAPE</c> decoding,
    /// <c>Transform</c>, disposal) lives here.
    /// </summary>
    internal abstract class SQLiteFeatureCursorBase : FeatureCursor, IFeatureCursor
    {
        private SQLiteConnection _connection;
        private SQLiteCommand _command;
        private SQLiteDataReader _reader;

        protected readonly IGeometryDef _geomDef;
        private readonly IFdbGeometryCodec _geometryCodec;

        protected SQLiteFeatureCursorBase(IGeometryDef geomDef, ISpatialReference toSRef, IDatumTransformations datumTransformations)
            : base(geomDef,
                   geomDef?.SpatialReference,
                   toSRef,
                   datumTransformations)
        {
            _geomDef = geomDef;
            _geometryCodec = FdbGeometryCodec.ForFeatureClass(geomDef);
        }

        /// <summary>Opens the connection and executes <paramref name="commandText"/>.</summary>
        protected async Task OpenReaderAsync(string connectionString, string commandText)
        {
            _connection = new SQLiteConnection(connectionString);
            await _connection.OpenAsync();

            _command = new SQLiteCommand(commandText, _connection);
            _command.Prepare();
            _command.SetCustomCursorTimeout();

            _reader = (SQLiteDataReader)await _command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
        }

        /// <summary>
        /// Streams the next materialized feature, skipping rows rejected by
        /// <see cref="PassesGeometryFilter"/>. Returns <c>null</c> at the end of the reader.
        /// Concrete cursors call this from <see cref="NextFeature"/> and add their own
        /// paging accounting on top where needed.
        /// </summary>
        protected async Task<IFeature> NextRawFeatureAsync()
        {
            try
            {
                while (true)
                {
                    if (_reader == null)
                    {
                        return null;
                    }

                    if (!await _reader.ReadAsync())
                    {
                        Dispose();
                        return null;
                    }

                    bool skip = false;
                    Feature feature = new Feature();

                    for (int i = 0; i < _reader.FieldCount; i++)
                    {
                        string name = _reader.GetName(i);
                        object obj = null;
                        try
                        {
                            obj = _reader.GetValue(i);
                        }
                        catch { }

                        if (name == "FDB_SHAPE" && obj != DBNull.Value)
                        {
                            IGeometry shape = DeserializeShape((byte[])obj);
                            if (shape != null)
                            {
                                if (!PassesGeometryFilter(shape))
                                {
                                    skip = true;
                                    break;
                                }
                                feature.Shape = shape;
                            }
                        }
                        else
                        {
                            FieldValue fv = new FieldValue(name, obj);
                            feature.Fields.Add(fv);
                            if (fv.Name == "FDB_OID")
                            {
                                feature.OID = Convert.ToInt32(obj);
                            }
                        }
                    }

                    if (skip)
                    {
                        continue;
                    }

                    Transform(feature);
                    return feature;
                }
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }

        /// <summary>Cursor-side geometry post-filter. Base implementation accepts every row.</summary>
        protected virtual bool PassesGeometryFilter(IGeometry shape) => true;

        private IGeometry DeserializeShape(byte[] bytes)
            => _geometryCodec.Decode(bytes, _geomDef);

        public abstract override Task<IFeature> NextFeature();

        public void Reset() { }

        public void Release() => Dispose();

        public override void Dispose()
        {
            base.Dispose();

            if (_command != null)
            {
                _command.Dispose();
                _command = null;
            }
            if (_reader != null)
            {
                _reader.Close();
                _reader = null;
            }
            if (_connection != null)
            {
                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                }
                _connection.Dispose();
                _connection = null;
            }
        }
    }
}
