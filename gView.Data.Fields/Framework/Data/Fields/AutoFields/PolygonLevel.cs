using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.FDB;
using gView.Framework.Data;
using gView.Data.Fields.Fields.AutoFields;

namespace gView.Framework.Data.Fields.AutoFields
{
    [gView.Framework.system.RegisterPlugIn("D2DB93C7-5614-47ef-B8F7-ABDA2C6151A7")]
    class PolygonLevel : Field, IAutoField
    {
        public PolygonLevel()
        {
            this.type = AutoFieldType;
        }

        #region IAutoField Member

        public string AutoFieldName
        {
            get { return "Level"; }
        }

        public string AutoFieldDescription
        {
            get { return "Calculates the level from polygon overlaps on feature creation."; }
        }

        public string AutoFieldPrimayName
        {
            get { return "Overlap_Level"; }
        }

        public FieldType AutoFieldType
        {
            get { return FieldType.integer; }
        }

        public bool OnInsert(IFeatureClass fc, IFeature feature)
        {
            if (fc == null || feature == null) return false;

            if (feature.Shape == null) return true;

            SpatialFilter filter = new SpatialFilter();
            filter.Geometry = feature.Shape;
            filter.SpatialRelation = spatialRelation.SpatialRelationIntersects;
            filter.FilterSpatialReference = fc.SpatialReference;
            filter.AddField(this.name);

            using (IFeatureCursor cursor = fc.Search(filter) as IFeatureCursor)
            {
                List<int> levels = new List<int>();

                IFeature f;
                while (((f = cursor.NextFeature) != null))
                {
                    if (f[this.name] == null || f[this.name] == DBNull.Value) continue;
                    int level = Convert.ToInt32(f[this.name]);
                    
                    int index = levels.BinarySearch(level);
                    if (index >= 0) continue;

                    levels.Insert(~index, level);
                }

                int Level = 0;
                if (levels.Count != 0)
                {
                    int min = levels[0];
                    int max = levels[levels.Count - 1];

                    if (min == 0)
                    {
                        for (int i = 0; i < levels.Count; i++)
                        {
                            if (levels.BinarySearch(i) < 0)
                            {
                                Level = i;
                                break;
                            }
                        }
                        if (Level == 0) Level = max + 1;
                    }
                }
                if (feature[this.name] != null)
                    feature[this.name] = Level;
                else
                    feature.Fields.Add(new FieldValue(this.name, Level));
            }

            return true;
        }

        public bool OnUpdate(IFeatureClass fc, IFeature feature)
        {
            return true;
        }

        #endregion
    }
}
