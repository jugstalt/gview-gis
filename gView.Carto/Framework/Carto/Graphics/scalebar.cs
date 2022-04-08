using System;

namespace gView.Framework.Carto.Graphics
{
    /// <summary>
    /// Zusammenfassung für scalebar.
    /// </summary>
    internal class Scalebar
    {
        double _scale, _dpi;
        public Scalebar(double scale, double dpi)
        {
            _scale = scale;
            _dpi = dpi;
        }
        public int ScaleBarWidth
        {
            get
            {
                double dpm = _dpi / 0.0256;
                double pix = _scale / dpm;
                double bl = pix * (200 * (_dpi / 96)) / 5.0;

                if (bl > 1000000)
                {
                    bl = Math.Round((int)(bl / 100000) * 100000.0, 0);
                }
                else if (bl > 100000)
                {
                    bl = Math.Round((int)(bl / 10000) * 10000.0, 0);
                }
                else if (bl > 10000)
                {
                    bl = Math.Round((int)(bl / 5000) * 5000.0, 0);
                }
                else if (bl > 1000)
                {
                    bl = Math.Round((int)(bl / 500) * 500.0, 0);
                }
                else if (bl > 100)
                {
                    bl = Math.Round((int)(bl / 100) * 100.0, 0);
                }
                else if (bl > 10)
                {
                    bl = Math.Round((int)(bl / 10) * 10.0, 0);
                }

                int bm_bl = (int)(bl / pix);
                int dist = (int)Math.Round(bl * 5, 0);
                return bm_bl * 5;
            }
        }

        public bool Create(GraphicsEngine.Abstraction.IBitmap bitmap, int a, int y)
        {
            double dpm = _dpi / 0.0256;
            double pix = _scale / dpm;
            double bl = pix * (200 * (_dpi / 96)) / 5.0;
            float fac = (float)_dpi / 96;

            if (bl > 1000000)
            {
                bl = Math.Round((int)(bl / 100000) * 100000.0, 0);
            }
            else if (bl > 100000)
            {
                bl = Math.Round((int)(bl / 10000) * 10000.0, 0);
            }
            else if (bl > 10000)
            {
                bl = Math.Round((int)(bl / 5000) * 5000.0, 0);
            }
            else if (bl > 1000)
            {
                bl = Math.Round((int)(bl / 500) * 500.0, 0);
            }
            else if (bl > 100)
            {
                bl = Math.Round((int)(bl / 100) * 100.0, 0);
            }
            else if (bl > 10)
            {
                bl = Math.Round((int)(bl / 10) * 10.0, 0);
            }

            int bm_bl = (int)(bl / pix);
            using (var canvas = bitmap.CreateCanvas())
            using (var brush = GraphicsEngine.Current.Engine.CreateSolidBrush(GraphicsEngine.ArgbColor.FromArgb(155, 149, 149)))
            using (var brush2 = GraphicsEngine.Current.Engine.CreateSolidBrush(GraphicsEngine.ArgbColor.White))
            using (var brush3 = GraphicsEngine.Current.Engine.CreateSolidBrush(GraphicsEngine.ArgbColor.White))
            using (var pen = GraphicsEngine.Current.Engine.CreatePen(GraphicsEngine.ArgbColor.Black, 1f))
            using (var font = GraphicsEngine.Current.Engine.CreateFont("Verdana", 7 * fac, GraphicsEngine.FontStyle.Bold))
            {
                int dist = (int)Math.Round(bl * 5, 0);

                // Hintergrund und Rahmen zeichnen
                canvas.FillRectangle(brush, new GraphicsEngine.CanvasRectangleF(a + 0, 15 * fac + y, bm_bl, 5 * fac));
                canvas.FillRectangle(brush, new GraphicsEngine.CanvasRectangleF(a + 2 * bm_bl, 15 * fac + y, bm_bl, 5 * fac));
                canvas.FillRectangle(brush, new GraphicsEngine.CanvasRectangleF(a + 4 * bm_bl, 15 * fac + y, bm_bl, 5 * fac));

                canvas.FillRectangle(brush2, new GraphicsEngine.CanvasRectangleF(a + 1 * bm_bl, 15 * fac + y, bm_bl, 5 * fac));
                canvas.FillRectangle(brush2, new GraphicsEngine.CanvasRectangleF(a + 3 * bm_bl, 15 * fac + y, bm_bl, 5 * fac));

                canvas.DrawRectangle(pen, new GraphicsEngine.CanvasRectangleF(a + 0, 14 * fac + y, bm_bl * 5 - 1, 5 * fac));
                canvas.DrawLine(pen, a, 12 * fac + y, a, 19 * fac + y);
                canvas.DrawLine(pen, a + bm_bl * 5 - 1, 12 * fac + y, a + bm_bl * 5 - 1, 19 * fac + y);
                canvas.DrawLine(pen, a + bm_bl, 14 * fac + y, a + bm_bl, 19 * fac + y);
                canvas.DrawLine(pen, a + bm_bl * 2, 14 * fac + y, a + bm_bl * 2, 19 * fac + y);
                canvas.DrawLine(pen, a + bm_bl * 3, 14 * fac + y, a + bm_bl * 3, 19 * fac + y);
                canvas.DrawLine(pen, a + bm_bl * 4, 14 * fac + y, a + bm_bl * 4, 19 * fac + y);

                string text = Math.Round(_scale, 0).ToString(), t = "";
                int counter = 1;
                // Tausenderpunkte
                for (int i = text.Length - 1; i > 0; i--)
                {
                    t = text[i] + t;
                    if ((counter++ % 3) == 0)
                    {
                        t = "." + t;
                    }
                }
                t = text[0] + t;
                text = "M 1:" + t;
                drawString(canvas, font, text, (float)(a + (bm_bl * 5 - canvas.MeasureText(text, font).Width) / 2), y);
                drawString(canvas, font, "0", a - 4, y);

                if (dist > 1000)
                {
                    float x = dist / (float)1000;
                    text = x.ToString() + " km";
                }
                else
                {
                    text = dist.ToString() + " m";
                }
                drawString(canvas, font, text, (float)(a + bm_bl * 5 - 5 * fac), y);
            }
            return true;
        }

        private void drawString(GraphicsEngine.Abstraction.ICanvas canvas,
                                GraphicsEngine.Abstraction.IFont font,
                                string text,
                                float x,
                                float y)
        {
            using (var brush = GraphicsEngine.Current.Engine.CreateSolidBrush(GraphicsEngine.ArgbColor.White))
            {
                canvas.DrawText(text, font, brush, x - 1, y);
                canvas.DrawText(text, font, brush, x + 1, y);
                canvas.DrawText(text, font, brush, x, y + 1);
                canvas.DrawText(text, font, brush, x, y - 1);
            }

            using (var brush = GraphicsEngine.Current.Engine.CreateSolidBrush(GraphicsEngine.ArgbColor.Black))
            {
                canvas.DrawText(text, font, brush, x, y);
            }

        }
    }
}
