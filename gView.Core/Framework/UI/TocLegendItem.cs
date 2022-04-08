using System;

namespace gView.Framework.UI
{
    public class TocLegendItem : IDisposable
    {
        public string Label { get; set; }
        public GraphicsEngine.Abstraction.IBitmap Image { get; set; }

        public void Dispose()
        {
            if (Image != null)
            {
                Image.Dispose();
                Image = null;
            }
        }
    }


}