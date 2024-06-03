using gView.Framework.Core.Geometry;
using gView.GraphicsEngine;
using System;

namespace gView.Framework.Core.Data
{
    public class GeorefBitmap : IDisposable
    {
        public GraphicsEngine.Abstraction.IBitmap Bitmap = null;
        public IEnvelope Envelope = null;
        public ISpatialReference SpatialReference = null;
        public float Opacity = 1.0f;

        public GeorefBitmap(GraphicsEngine.Abstraction.IBitmap bitmap)
        {
            Bitmap = bitmap;
        }

        public void MakeTransparent(ArgbColor transColor)
        {
            if (Bitmap == null)
            {
                return;
            }

            try
            {
                var b = Current.Engine.CreateBitmap(Bitmap.Width, Bitmap.Height, PixelFormat.Rgba32);
                using (var g = b.CreateCanvas())
                {
                    g.DrawBitmap(Bitmap, new CanvasPoint(0, 0));
                }
                b.MakeTransparent(transColor);
                Bitmap.Dispose();
                Bitmap = b;
            }
            catch { }
        }

        #region IDisposable Member

        public void Dispose()
        {
            if (Bitmap != null)
            {
                Bitmap.Dispose();
                Bitmap = null;
            }
            Envelope = null;
            SpatialReference = null;
        }

        #endregion
    }
}