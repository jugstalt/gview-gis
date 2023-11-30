using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace gView.DataSources.MSSqlSpatial
{
    public class Featureclass : gView.Framework.OGC.DB.OgcSpatialFeatureclass
    {
        private static string[] IdFieldCandidates = new string[] {  "gview_id", "id", "objectid", "row_id",  "object_id", "row_id" };

        private Featureclass(GeometryDataset dataset, string name, string idFieldName, string shapeFieldName, bool isView)
        {

        }

        async static public Task<IFeatureClass> Create(GeometryDataset dataset, string name, string idFieldName, string shapeFieldName, bool isView)
        {
            try
            {
                var featureClass = new Featureclass(dataset, name, idFieldName, shapeFieldName, isView);

                featureClass._name = await dataset.TableNamePlusSchema(name, isView);
                featureClass._idfield = idFieldName;
                featureClass._shapefield = shapeFieldName;
                featureClass._geomType = GeometryType.Unknown;

                featureClass._dataset = dataset;
                if (featureClass._dataset is GeographyDataset)
                {
                    featureClass._sRef = gView.Framework.Geometry.SpatialReference.FromID("epsg:4326");
                }

                await featureClass.ReadSchema();

                if (String.IsNullOrEmpty(featureClass._idfield) && featureClass._fields.Count > 0 && featureClass._dataset != null)
                {
                    for (int i = 0; i < featureClass._fields.Count; i++)
                    {
                        var field = featureClass._fields[i] as Field;

                        if (field != null && IdFieldCandidates.Contains(field.name.ToLower()))
                        {
                            if ((field.type == FieldType.integer || field.type == FieldType.biginteger || field.type == FieldType.ID))
                            {
                                featureClass._idfield = field.name;
                                ((Field)field).type = FieldType.ID;

                                break;
                            }
                        }
                    }
                }

                //base._geomType = geometryType.Polygon;

                if (featureClass._sRef == null)
                {
                    featureClass._sRef = await gView.Framework.OGC.DB.OgcSpatialFeatureclass.TrySelectSpatialReference(dataset, featureClass);
                }

                return featureClass;
            }
            catch
            {
                return null;
            }
        }

        public override ISpatialReference SpatialReference
        {
            get
            {
                return _sRef;
            }
        }
    }
}
