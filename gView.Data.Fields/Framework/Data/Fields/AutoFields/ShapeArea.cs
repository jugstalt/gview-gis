using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Framework.FDB;
using gView.Framework.Geometry;

namespace gView.Framework.Data.Fields.AutoFields
{
    [gView.Framework.system.RegisterPlugIn("2923B158-F8FB-45f5-B82B-2376612ABD50")]
    class ShapeArea : Field, IAutoField
    {
        public ShapeArea()
        {
            this.type = AutoFieldType;
            this.size = 8;
        }

        #region IAutoField Member

        public string AutoFieldName
        {
            get { return "Shape Area"; }
        }

        public string AutoFieldDescription
        {
            get { return "The area for a polygon feature."; }
        }

        public string AutoFieldPrimayName
        {
            get { return "Shape_Area"; }
        }

        public FieldType AutoFieldType
        {
            get { return FieldType.Double; }
        }

        public bool OnInsert(IFeatureClass fc, IFeature feature)
        {
            return Calc(feature);
        }

        public bool OnUpdate(IFeatureClass fc, IFeature feature)
        {
            if (feature.Shape == null) return true;

            return Calc(feature);
        }

        #endregion

        private bool Calc(IFeature feature)
        {
            if (feature == null || feature.Shape == null) return false;

            double area = 0.0;

            if (feature.Shape is Polygon)
            {
                area = ((Polygon)feature.Shape).CalcArea();
            } 
            else if (feature.Shape is IPolygon)
            {
                for (int i = 0; i < ((IPolygon)feature.Shape).RingCount; i++)
                {
                    IRing ring = ((IPolygon)feature.Shape)[i];
                    if (ring == null) continue;

                    if (ring is IHole)
                    {
                        area -= ring.Area;
                    }
                    else
                    {
                        area += ring.Area;
                    }
                }
                feature[this.name] = area;
            }

            if (feature[this.name] != null)
                feature[this.name] = area;
            else
                feature.Fields.Add(new FieldValue(this.name, area));

            return true;
        }
    }
}
