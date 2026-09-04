using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Db.Extensions;
using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;

namespace gView.DataSources.Fdb.PostgreSql.Cursors
{
    /// <summary>
    /// Shared lifecycle and row materialization for the PostgreSQL FDB feature cursors. Each
    /// concrete cursor only decides how the SQL statement is built and whether it needs cursor-side
    /// paging / geometry post-filtering.
    /// </summary>
    internal abstract class PgFeatureCursorBase : FeatureCursor
    {
        private readonly DbProviderFactory _factory = pgFDB._dbProviderFactory;
        private DbConnection _connection;
        private DbCommand _command;
        private DbDataReader _reader;

        protected readonly IGeometryDef _geomDef;

        protected PgFeatureCursorBase(IGeometryDef geomDef, ISpatialReference toSRef, IDatumTransformations datumTransformations)
            : base(geomDef,
                   geomDef?.SpatialReference,
                   toSRef,
                   datumTransformations)
        {
            _geomDef = geomDef;
        }

        protected async Task OpenReaderAsync(string connectionString, string commandText)
        {
            _connection = _factory.CreateConnection();
            _connection.ConnectionString = connectionString;
            await _connection.OpenAsync();

            _command = _factory.CreateCommand();
            _command.Connection = _connection;
            _command.CommandText = commandText;
            _command.SetCustomCursorTimeout();

            _reader = await _command.ExecuteReaderAsync(CommandBehavior.Default);
        }

        protected async Task<IFeature> NextRawFeatureAsync()
        {
            try
            {
                while (true)
                {
                    if (_reader == null)
                    {
                        Dispose();
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
                        object obj = _reader.GetValue(i);

                        if (name == ShapeColumn && obj != DBNull.Value)
                        {
                            IGeometry shape = DecodeShape((byte[])obj);
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

        protected virtual bool PassesGeometryFilter(IGeometry shape) => true;

        /// <summary>
        /// Name of the column carrying the geometry in the reader. The classic cursors read the
        /// proprietary blob from <c>FDB_SHAPE</c>; a native cursor aliases <c>ST_AsBinary(...)</c>
        /// to something else (e.g. <c>temp_geometry</c>).
        /// </summary>
        protected virtual string ShapeColumn => "FDB_SHAPE";

        /// <summary>Decodes the geometry bytes of <see cref="ShapeColumn"/>. Default: proprietary FDB blob.</summary>
        protected virtual IGeometry DecodeShape(byte[] bytes) => DeserializeShape(bytes);

        private IGeometry DeserializeShape(byte[] bytes)
        {
            IGeometry p = _geomDef.GeometryType switch
            {
                GeometryType.Point => new gView.Framework.Geometry.Point(),
                GeometryType.Polyline => new gView.Framework.Geometry.Polyline(),
                GeometryType.Polygon => new gView.Framework.Geometry.Polygon(),
                _ => null
            };

            if (p == null)
            {
                return null;
            }

            using var r = new BinaryReader(new MemoryStream());
            r.BaseStream.Write(bytes, 0, bytes.Length);
            r.BaseStream.Position = 0;
            p.Deserialize(r, _geomDef);

            return p;
        }

        public abstract override Task<IFeature> NextFeature();

        public void Reset() { }

        public void Release() => Dispose();

        public override void Dispose()
        {
            base.Dispose();

            if (_connection != null && _command != null && _connection.State == ConnectionState.Open)
            {
                if (_reader != null)
                {
                    try
                    {
                        while (_reader.Read())
                        {
                            try { _reader.Close(); }
                            catch { }
                            _command.Cancel();
                        }
                    }
                    catch (Exception /*ex*/)
                    {
                    }
                }

                _command.Dispose();
                _command = null;
            }

            if (_reader != null)
            {
                try { _reader.Close(); }
                catch { }
                _reader = null;
            }
            if (_connection != null)
            {
                if (_connection.State == ConnectionState.Open)
                {
                    try { _connection.Close(); }
                    catch { }
                }
                _connection.Dispose();
                _connection = null;
            }
        }
    }
}
