using gView.Framework.Data.Cursors;
using gView.Framework.FDB;
using gView.Framework.Geometry;
using System;
using System.Threading.Tasks;

namespace gView.Framework.Data.Filters
{
    public class BufferQueryFilter : QueryFilter, IBufferQueryFilter
    {
        private IQueryFilter _rootFilter = null;
        private IFeatureClass _rootFC = null;
        private double _bufferDistance = 0.0;
        private gView.Framework.Carto.GeoUnits _units = gView.Framework.Carto.GeoUnits.Meters;

        public BufferQueryFilter() { }
        public BufferQueryFilter(IQueryFilter proto)
            : base(proto)
        {
            if (proto is IBufferQueryFilter)
            {
                IBufferQueryFilter bProto = (IBufferQueryFilter)proto;
                if (bProto.RootFilter != null)
                {
                    _rootFilter = bProto.RootFilter.Clone() as IQueryFilter;
                }

                _rootFC = bProto.RootFeatureClass;
                _bufferDistance = bProto.BufferDistance;
                _units = bProto.BufferUnits;
            }
        }

        #region IBufferQuery Member

        public IQueryFilter RootFilter
        {
            get { return _rootFilter; }
            set { _rootFilter = value; }
        }

        public IFeatureClass RootFeatureClass
        {
            get { return _rootFC; }
            set { _rootFC = value; }
        }

        public double BufferDistance
        {
            get { return _bufferDistance; }
            set { _bufferDistance = value; }
        }

        public gView.Framework.Carto.GeoUnits BufferUnits
        {
            get { return _units; }
            set { _units = value; }
        }

        #endregion

        public override object Clone()
        {
            BufferQueryFilter filter = new BufferQueryFilter(this);
            return filter;
        }
        async public static Task<ISpatialFilter> ConvertToSpatialFilter(IBufferQueryFilter bufferQuery)
        {
            try
            {
                if (bufferQuery == null ||
                    bufferQuery.RootFilter == null ||
                    bufferQuery.RootFeatureClass == null)
                {
                    return null;
                }

                IQueryFilter rootFilter = bufferQuery.RootFilter.Clone() as IQueryFilter;
                if (bufferQuery.RootFeatureClass.Dataset != null && bufferQuery.RootFeatureClass.Dataset.Database is IDatabaseNames)
                {
                    rootFilter.SubFields = ((IDatabaseNames)bufferQuery.RootFeatureClass.Dataset.Database).DbColName(bufferQuery.RootFeatureClass.IDFieldName) + "," +
                                           ((IDatabaseNames)bufferQuery.RootFeatureClass.Dataset.Database).DbColName(bufferQuery.RootFeatureClass.ShapeFieldName);
                }
                else
                {
                    rootFilter.SubFields = bufferQuery.RootFeatureClass.IDFieldName + "," + bufferQuery.RootFeatureClass.ShapeFieldName;
                }
                // Wenn SpatialFilter, dann gleich hier projezieren, damit
                // FEATURECOORDSYS und FILTERCOORDSYS für IMS Requests immer gleich sind,
                // da unten die SpatialReference für den konvertierten Filter die SpatialRef 
                // der RootfeatureClass übernonnem wird...
                //
                // Seit es den FilterSpatialReference und FeatureSpatialReference implementiert 
                // sind, ist das nicht mehr notwendig bzw. erwünscht...
                //
                //if (rootFilter is ISpatialFilter)
                //    rootFilter = SpatialFilter.Project(rootFilter as ISpatialFilter,
                //        bufferQuery.RootFeatureClass.SpatialReference);

                IPolygon buffer = null;
                using (IFeatureCursor cursor = (IFeatureCursor)await bufferQuery.RootFeatureClass.Search(rootFilter))
                {
                    IFeature feature;
                    while ((feature = await cursor.NextFeature()) != null)
                    {
                        if (feature.Shape == null)
                        {
                            continue;
                        }

                        if (!(feature.Shape is ITopologicalOperation))
                        {
                            throw new Exception("Buffer is not implemented for selected geometry type...");
                        }

                        ITopologicalOperation topoOp = feature.Shape as ITopologicalOperation;
                        IPolygon poly = topoOp.Buffer(bufferQuery.BufferDistance);
                        if (poly == null)
                        {
                            throw new Exception("Buffer fails for geometry...");
                        }
                        if (buffer == null)
                        {
                            buffer = poly;
                        }
                        else
                        {
                            ((ITopologicalOperation)buffer).Union(poly);
                        }
                    }
                }

                if (buffer == null)
                {
                    throw new Exception("No geometry found to buffer! (SELECT " + rootFilter.SubFields + " FROM " + bufferQuery.RootFeatureClass.Name + " WHERE " + rootFilter.WhereClause + ")");
                }

                ISpatialFilter spatialFilter = new SpatialFilter();
                spatialFilter.SubFields = bufferQuery.SubFields;
                spatialFilter.Geometry = buffer;
                spatialFilter.WhereClause = bufferQuery.WhereClause;
                // Die abgefragen Features liegen im FeatureSpatialReference System
                // des RootFilter!!
                spatialFilter.FilterSpatialReference = bufferQuery.RootFilter.FeatureSpatialReference;
                //spatialFilter.FilterSpatialReference = ((bufferQuery.RootFilter is ISpatialFilter) ?
                //    ((ISpatialFilter)bufferQuery.RootFilter).FilterSpatialReference :
                //    bufferQuery.RootFeatureClass.SpatialReference);

                spatialFilter.FeatureSpatialReference = bufferQuery.FeatureSpatialReference;

                // UserData übernehen
                bufferQuery.CopyUserDataTo(spatialFilter);

                return spatialFilter;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
