using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;
using gView.Interoperability.GeoServices.Rest;
using gView.Interoperability.GeoServices.Rest.DTOs;
using System;
using System.Text;
using System.Threading.Tasks;

namespace gView.Interoperability.GeoServices.Dataset
{
    public class GeoServicesFeatureClass : FeatureClass, IWebFeatureClass
    {
        private GeoServicesDataset _dataaset;

        private GeoServicesFeatureClass() { }

        async static internal Task<GeoServicesFeatureClass> CreateAsync(GeoServicesDataset dataset, JsonLayerDTO jsonLayer)
        {
            var featureClass = new GeoServicesFeatureClass();

            featureClass._dataaset = dataset;

            featureClass.ID = jsonLayer.Id.ToString();
            featureClass.Name = jsonLayer.Name;

            var fields = featureClass.Fields as FieldCollection;
            if (fields != null && jsonLayer.Fields != null)
            {
                foreach (var jsonField in jsonLayer.Fields)
                {
                    var fieldType = RestHelper.FType(jsonField.Type);

                    if (fieldType == FieldType.String)
                    {
                        fields.Add(new Field(jsonField.Name, fieldType, 1024)
                        {
                            aliasname = jsonField.Alias ?? jsonField.Name
                        });
                    }
                    else
                    {
                        fields.Add(new Field(jsonField.Name, fieldType)
                        {
                            aliasname = jsonField.Alias ?? jsonField.Name
                        });
                    }

                    if (fieldType == FieldType.ID)
                    {
                        featureClass.IDFieldName = jsonField.Name;
                    }
                }
            }

            featureClass.GeometryType = jsonLayer.GetGeometryType();

            if (jsonLayer.Extent != null)
            {
                featureClass.Envelope = new Envelope(jsonLayer.Extent.Xmin,
                                             jsonLayer.Extent.Ymin,
                                             jsonLayer.Extent.Xmax,
                                             jsonLayer.Extent.Ymax);
            }

            featureClass.SpatialReference = await dataset.GetSpatialReference();

            return featureClass;
        }

        #region IWebFeatureClass

        public string ID { get; private set; }

        #endregion

        public override Task<IFeatureCursor> GetFeatures(IQueryFilter filter)
        {
            string queryUrl = $"{_dataaset.ServiceUrl()}/{this.ID}/query?f=json";

            string geometryType = String.Empty, geometry = String.Empty;
            int outSrefId = (filter.FeatureSpatialReference != null) ? filter.FeatureSpatialReference.EpsgCode : 0,
                inSrefId = 0;

            // Bitte nicht ändern -> Verursacht Pagination Error vom AGS und Performance Problem wenn daten im SQL Spatial liegen
            string featureLimit = String.Empty; //(filter.FeatureLimit <= 0 ? "" : filter.FeatureLimit.ToString());
            string separatedSubFieldsString = String.IsNullOrWhiteSpace(filter.SubFields) ? "*" : filter.SubFields.Replace(" ", ",");

            if (filter is SpatialFilter)
            {
                var sFilter = (SpatialFilter)filter;
                inSrefId = sFilter.FilterSpatialReference != null ? sFilter.FilterSpatialReference.EpsgCode : 0;
                if (sFilter.Geometry is IPoint)
                {
                    geometry = RestHelper.ConvertGeometryToJson(((Point)sFilter.Geometry), inSrefId);
                    geometryType = RestHelper.GetGeometryTypeString(((Point)sFilter.Geometry));
                }
                else if (sFilter.Geometry is IMultiPoint)
                {
                    geometry = RestHelper.ConvertGeometryToJson(((MultiPoint)sFilter.Geometry), inSrefId);
                    geometryType = RestHelper.GetGeometryTypeString(((MultiPoint)sFilter.Geometry));
                } // TODO - if needed
                else if (sFilter.Geometry is IPolyline)
                {
                    geometry = RestHelper.ConvertGeometryToJson(((Polyline)sFilter.Geometry), inSrefId);
                    geometryType = RestHelper.GetGeometryTypeString(((Polyline)sFilter.Geometry));
                }

                else if (sFilter.Geometry is IPolygon)
                {
                    geometry = RestHelper.ConvertGeometryToJson(((Polygon)sFilter.Geometry), inSrefId);
                    geometryType = RestHelper.GetGeometryTypeString(((Polygon)sFilter.Geometry));
                }
                else if (sFilter.Geometry is IEnvelope)
                {
                    geometry = RestHelper.ConvertGeometryToJson(((Envelope)sFilter.Geometry), inSrefId);
                    geometryType = RestHelper.GetGeometryTypeString(((Envelope)sFilter.Geometry));
                }
                else { }
            }

            string where = filter.WhereClause;

            // is always set in featurecursor
            //string orderBy = this.IDFieldName;

            var postBodyData = new StringBuilder();
            postBodyData.Append($"&geometry={geometry}&geometryType={geometryType}&spatialRel=esriSpatialRelIntersects&relationParam=&objectIds=");
            postBodyData.Append($"&time=&maxAllowableOffset=&outFields={separatedSubFieldsString}&resultRecordCount={featureLimit}&f=pjson");

            postBodyData.Append($"&returnCountOnly=false&returnIdsOnly=false&returnGeometry={true}");

            return Task.FromResult<IFeatureCursor>(
                new GeoServicesFeatureCursor(_dataaset, this, queryUrl, postBodyData.ToString(), where));
        }

        async public override Task<ICursor> Search(IQueryFilter filter)
        {
            var cursor = (await GetFeatures(filter)) as ICursor;

            return cursor;
        }
    }
}
