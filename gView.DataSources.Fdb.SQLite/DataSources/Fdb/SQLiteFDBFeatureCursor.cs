using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Db.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace gView.DataSources.Fdb.SQLite
{
    internal class SQLiteFDBFeatureCursor : FeatureCursor
    {
        SQLiteConnection _connection = null;
        SQLiteDataReader _reader = null;
        SQLiteCommand _readerCommand = null;
        //DataTable _schemaTable=null;
        string _sql = "", _where = "", _orderby = "";
        int _nid_pos = 0;
        List<long> _nids;
        //IGeometry _queryGeometry;
        //Envelope _queryEnvelope;
        ISpatialFilter _spatialFilter = null;
        IGeometryDef _geomDef;
        int _limit = 0, _beginRecord = 0;

        private SQLiteFDBFeatureCursor(IGeometryDef geomDef, ISpatialReference toSRef) :
            base((geomDef != null) ? geomDef.SpatialReference : null, toSRef)
        {

        }

        async static public Task<IFeatureCursor> Create(string connString, string sql, string where, string orderby, int limit, int beginRecord, List<long> nids, ISpatialFilter filter, IGeometryDef geomDef, ISpatialReference toSRef)
        {
            var cursor = new SQLiteFDBFeatureCursor(geomDef, toSRef);

            //try 
            {
                cursor._connection = new SQLiteConnection(connString);
                cursor._sql = sql;
                await cursor._connection.OpenAsync();
                cursor._geomDef = geomDef;
                cursor._limit = limit;
                cursor._beginRecord = beginRecord;

                cursor._where = where;
                cursor._orderby = orderby;

                if (nids != null)
                {
                    /*if (nids.Count > 0)*/
                    cursor._nids = nids;
                }

                cursor._spatialFilter = filter;

                await cursor.ExecuteReaderAsync();
            }
            //catch(Exception ex) 
            {
                //Dispose();
                //throw(ex);
            }

            return cursor;
        }

        public override void Dispose()
        {
            base.Dispose();
            if (_readerCommand != null)
            {
                _readerCommand.Dispose();
                _readerCommand = null;
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

        async private Task<bool> ExecuteReaderAsync()
        {
            if (_readerCommand != null)
            {
                _readerCommand.Dispose();
                _readerCommand = null;
            }
            if (_reader != null)
            {
                _reader.Close();
                _reader = null;
            }

            string where = _where;

            StringBuilder sb = new StringBuilder();
            SQLiteParameter parameter = null, parameter2 = null;
            long pVal = 0, p2Val = 0;
            if (_nids != null)
            {
                //if (_nids != null)
                //{
                //    if (_nid_pos >= _nids.Count) return false;
                //    if (where != "") where += " AND ";
                //    where += "(FDB_NID=" + _nids[_nid_pos].ToString() + ")";
                //}

                if (_nid_pos >= _nids.Count)
                {
                    return false;
                }

                if (_nids[_nid_pos] < 0)
                {
                    where = "(FDB_NID BETWEEN @FDB_NID_FROM AND @FDB_NID_TO)" + (!String.IsNullOrEmpty(where) ? " AND (" + where + ")" : String.Empty);
                    //where = "(FDB_NID>=@FDB_NID_FROM AND FDB_NID<=@FDB_NID_TO)" + (!String.IsNullOrEmpty(where) ? " AND (" + where + ")" : String.Empty);
                    parameter = new SQLiteParameter("@FDB_NID_FROM", 0L);
                    parameter2 = new SQLiteParameter("@FDB_NID_TO", 0L);

                    pVal = -_nids[_nid_pos];
                    p2Val = _nids[_nid_pos + 1];
                    _nid_pos++;
                }
                else
                {
                    where = "(FDB_NID=@FDB_NID)" + (!String.IsNullOrEmpty(where) ? " AND (" + where + ")" : String.Empty);
                    parameter = new SQLiteParameter("@FDB_NID", 0L);

                    pVal = _nids[_nid_pos];
                }
            }
            else
            {
                if (_nid_pos > 0)
                {
                    return false;
                }
            }
            if (where != "")
            {
                where = " WHERE " + where;
            }

            _nid_pos++;
            _readerCommand = new SQLiteCommand(_sql +
                                                    where +
                                                    (!String.IsNullOrWhiteSpace(_orderby) ? " ORDER BY " + _orderby : "") +
                                                    (_limit > 0 ? " LIMIT " + _limit : "") +
                                                    (_beginRecord > 0 && _limit > 0 ? " OFFSET " + Math.Max(0, _beginRecord - 1) : ""),
                                                    _connection);


            if (parameter != null)
            {
                _readerCommand.Parameters.Add(parameter);
            }

            if (parameter2 != null)
            {
                _readerCommand.Parameters.Add(parameter2);
            }

            _readerCommand.Prepare();

            if (parameter != null)
            {
                parameter.Value = pVal;
            }

            if (parameter2 != null)
            {
                parameter2.Value = p2Val;
            }

            _readerCommand.SetCustomCursorTimeout();
            _reader = (SQLiteDataReader)await _readerCommand.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

            return true;
        }

        #region IFeatureCursor Member

        public void Reset()
        {

        }
        public void Release()

        {
            this.Dispose();
        }

        async public override Task<IFeature> NextFeature()
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
                        await ExecuteReaderAsync();
                        //return NextFeature;
                        continue;
                    }

                    bool nextFeature = false;
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

                        if (/*_schemaTable.Rows[i][0].ToString()*/name == "FDB_SHAPE" && obj != DBNull.Value)
                        {
                            BinaryReader r = new BinaryReader(new MemoryStream());
                            r.BaseStream.Write((byte[])obj, 0, ((byte[])obj).Length);
                            r.BaseStream.Position = 0;

                            IGeometry p = null;
                            switch (_geomDef.GeometryType)
                            {
                                case GeometryType.Point:
                                    p = new gView.Framework.Geometry.Point();
                                    break;
                                case GeometryType.Polyline:
                                    p = new gView.Framework.Geometry.Polyline();
                                    break;
                                case GeometryType.Polygon:
                                    p = new gView.Framework.Geometry.Polygon();
                                    break;
                            }
                            if (p != null)
                            {
                                p.Deserialize(r, _geomDef);
                                r.Close();

                                /*
                                if(_queryEnvelope!=null) 
                                {
                                    if(!_queryEnvelope.Intersects(p.Envelope)) 
                                        return NextFeature;
                                    if (_queryGeometry is IEnvelope)
                                    {
                                        if (!Algorithm.InBox(p, (IEnvelope)_queryGeometry))
                                            return NextFeature;
                                    }
                                }
                                */
                                if (_spatialFilter != null)
                                {
                                    if (!gView.Framework.Geometry.SpatialRelation.Check(_spatialFilter, p))
                                    {
                                        nextFeature = true;
                                        break;
                                    }
                                }
                                feature.Shape = p;
                            }
                        }
                        else
                        {
                            FieldValue fv = new FieldValue(/*_schemaTable.Rows[i][0].ToString()*/name, obj);
                            feature.Fields.Add(fv);
                            if (fv.Name == "FDB_OID")
                            {
                                feature.OID = Convert.ToInt32(obj);
                            }
                        }
                    }

                    if (nextFeature == true)
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
                //return null;
            }
        }

        #endregion
    }
}
