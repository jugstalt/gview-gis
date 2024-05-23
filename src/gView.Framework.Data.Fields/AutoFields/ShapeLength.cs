using gView.Framework.Core.Data;
using gView.Framework.Core.FDB;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Common;
using System.Threading.Tasks;

namespace gView.Framework.Data.Fields.AutoFields
{
    [RegisterPlugIn("2CE75DD8-7E11-4047-B3CE-D7BA6EEB5C65")]
    class ShapeLength : Field, IAutoField
    {
        public ShapeLength()
        {
            this.type = AutoFieldType;
            this.size = 8;
        }

        #region IAutoField Member

        public string AutoFieldName
        {
            get { return "Shape Length"; }
        }

        public string AutoFieldDescription
        {
            get { return "The length of a line feature or the circumference of a polygon feature."; }
        }

        public string AutoFieldPrimayName
        {
            get { return "Shape_Length"; }
        }

        public FieldType AutoFieldType
        {
            get { return FieldType.Double; }
        }

        public Task<bool> OnInsert(IFeatureClass fc, IFeature feature)
        {
            return Task.FromResult<bool>(Calc(feature));
        }

        public Task<bool> OnUpdate(IFeatureClass fc, IFeature feature)
        {
            if (feature.Shape == null)
            {
                return Task.FromResult<bool>(true);
            }

            return Task.FromResult<bool>(Calc(feature));
        }

        #endregion

        private bool Calc(IFeature feature)
        {
            if (feature == null || feature.Shape == null)
            {
                return false;
            }

            double length = 0;
            if (feature.Shape is IPolyline)
            {
                for (int i = 0; i < ((IPolyline)feature.Shape).PathCount; i++)
                {
                    IPath path = ((IPolyline)feature.Shape)[i];
                    if (path == null)
                    {
                        continue;
                    }

                    length += path.Length;
                }
            }
            else if (feature.Shape is IPolygon)
            {
                for (int i = 0; i < ((IPolygon)feature.Shape).RingCount; i++)
                {
                    IRing ring = ((IPolygon)feature.Shape)[i];
                    if (ring == null)
                    {
                        continue;
                    }

                    length += ring.Length;
                }
            }

            if (feature[this.name] != null)
            {
                feature[this.name] = length;
            }
            else
            {
                feature.Fields.Add(new FieldValue(this.name, length));
            }

            return true;
        }
    }
}
