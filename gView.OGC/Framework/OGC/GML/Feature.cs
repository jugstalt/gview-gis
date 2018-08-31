using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using System.IO;
using gView.Framework.Geometry;

namespace gView.Framework.OGC.GML
{
    public class FeatureTranslator
    {
        #region Feature To GML
        public static void Feature2GML(IFeature feature, IFeatureClass fc, string fcID, StringBuilder sb, string srsName, IGeometricTransformer transformer, GmlVersion version)
        {
            if (feature == null ||  fc == null) return;

            sb.Append(@"
   <gml:featureMember>
      <gv:" + fcID + " gml:id=\"" + fcID + "." + feature.OID + "\">");

            // Shape
            IGeometry shape = (transformer != null) ? transformer.Transform2D(feature.Shape) as IGeometry : feature.Shape;
            if (shape != null)
            {
                sb.Append(@"
         <gml:boundedBy>");
                sb.Append(GeometryTranslator.Geometry2GML(shape.Envelope, srsName, version));
                sb.Append(@"
         </gml:boundedBy>");

                sb.Append(@"
         <gv:" + fc.ShapeFieldName.Replace("#", "") + ">");
                sb.Append(GeometryTranslator.Geometry2GML(shape, srsName, version));
                sb.Append(@"
         </gv:" + fc.ShapeFieldName.Replace("#", "") + ">");
            }

            // Fields
            foreach (FieldValue fv in feature.Fields)
            {
                if (fv.Name == fc.ShapeFieldName) continue;

                // TODO: Value soll noch auf <,>,&,... untersucht werden !!!
                sb.Append(@"
         <gv:" + fv.Name.Replace("#", "") + ">" + fv.Value + "</gv:" + fv.Name.Replace("#", "") + ">");
            }
            sb.Append(@"
      </gv:" + fcID + @">
   </gml:featureMember>");
        }

        public static void Features2GML(IFeatureCursor cursor, IFeatureClass fc, string fcID, StringBuilder sb, string srsName, IGeometricTransformer transformer, GmlVersion version)
        {
            Features2GML(cursor, fc, fcID, sb, srsName, transformer, version, -1);
        }
        public static void Features2GML(IFeatureCursor cursor, IFeatureClass fc, string fcID, StringBuilder sb, string srsName, IGeometricTransformer transformer, GmlVersion version, int maxFeatures)
        {
            if (cursor == null || fc == null) return;

            int count = 0;
            IFeature feature = null;
            while ((feature = cursor.NextFeature) != null)
            {
                Feature2GML(feature, fc, fcID, sb, srsName, transformer, version);
                count++;
                if (maxFeatures > 0 && count > maxFeatures)
                    break;
            }
        }
        #endregion
    }
}
