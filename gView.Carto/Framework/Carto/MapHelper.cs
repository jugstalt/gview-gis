using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;
using System;

namespace gView.Framework.Carto
{
    public class MapHelper
    {
        static public IEnvelope Project3(IFeatureClass fc, IDisplay display)
        {
            if (fc == null || display == null)
            {
                return null;
            }

            if (display.GeometricTransformer == null)
            {
                return display.Envelope;
            }

            //Feature feature=new Feature();
            //feature.Shape=fClass.Envelope;
            //_layer.FeatureRenderer.Draw(_map.Display, feature);

            IEnvelope classEnvelope = ((IGeometry)display.GeometricTransformer.Transform2D(fc.Envelope)).Envelope;

            classEnvelope.minx = Math.Max(classEnvelope.minx, display.Envelope.minx);
            classEnvelope.miny = Math.Max(classEnvelope.miny, display.Envelope.miny);
            classEnvelope.maxx = Math.Min(classEnvelope.maxx, display.Envelope.maxx);
            classEnvelope.maxy = Math.Min(classEnvelope.maxy, display.Envelope.maxy);

            IEnvelope filterGeom = ((IGeometry)display.GeometricTransformer.InvTransform2D(classEnvelope)).Envelope;
            //feature = new Feature();
            //feature.Shape = filterGeom;
            //_layer.FeatureRenderer.Draw(_map.Display, feature);

            return filterGeom;
        }
        static public IEnvelope Project(IFeatureClass fc, IDisplay display)
        {
            if (fc == null || display == null)
            {
                return null;
            }

            if (display.GeometricTransformer == null)
            {
                return display.Envelope;
            }

            IPointCollection pColl = display.Envelope.ToPointCollection(0);

            IPointCollection pColl2 = (IPointCollection)display.GeometricTransformer.InvTransform2D(pColl);
            IPointCollection pColl3 = (IPointCollection)display.GeometricTransformer.Transform2D(pColl2);

            double epsi = 0.0;
            if (display.SpatialReference.SpatialParameters.IsGeographic)
            {
                // ???
                epsi = Math.Max(display.Envelope.Width, display.Envelope.Height) / 1e2;
            }
            else
            {
                // ???
                epsi = Math.Max(display.Envelope.Width, display.Envelope.Height) / 1e3;
            }
            if (!((PointCollection)pColl).Equals(pColl3, epsi))
            {
                return null;
            }
            else
            {
                return pColl2.Envelope;
            }
        }
        static public IEnvelope Project2(IFeatureClass fc, IDisplay display)
        {
            if (display == null)
            {
                return null;
            }

            if (display.GeometricTransformer == null)
            {
                return display.Envelope;
            }

            IPointCollection pColl = display.Envelope.ToPointCollection(100);

            pColl = (IPointCollection)display.GeometricTransformer.InvTransform2D(pColl);

            //MultiPoint mPoint = new MultiPoint(pColl);
            //ISymbol pSymbol = PlugInManager.Create(KnownObjects.Symbology_SimplePointSymbol) as ISymbol;
            //display.Draw(pSymbol, mPoint);

            return pColl.Envelope;
        }

        static public IQueryFilter MapQueryFilter(IQueryFilter filter)
        {
            if (filter is ISpatialFilter)
            {
                if (((ISpatialFilter)filter).Geometry == null)
                {
                    return new QueryFilter(filter);
                }
            }
            return filter;
        }
    }
}
