using System;
using System.Linq;

namespace gView.GraphicsEngine.Skia.Extensions
{
    static class LineDashStyleExtensions
    {
        static public float[] ToPickerArray(this LineDashStyle dashStyle, float penWidth, LineCap lineCap)
        {
            float[] pickerArray = null;
            float dashhWidth = 4f, dashHole = 2f;
            float dotWidth = 1f, dotHole = 1f;

            switch (lineCap)
            {
                case LineCap.Square:
                    dashhWidth -= 1;
                    dashHole += 1;
                    dotWidth = 0.001f;
                    dotHole = 2f;
                    break;
                case LineCap.Round:
                    dashhWidth -= 1f;
                    dashHole += 1;
                    dotWidth = 0.001f;
                    dotHole = 1.7f;
                    break;
            }

            switch (dashStyle)
            {
                case LineDashStyle.Dot:
                    pickerArray = new[] { dotWidth, dotHole };
                    break;
                case LineDashStyle.Dash:
                    pickerArray = new[] { dashhWidth, dashHole };
                    break;
                case LineDashStyle.DashDot:
                    pickerArray = new[] { dashhWidth, dotHole, dotWidth, dotHole };
                    break;
                case LineDashStyle.DashDotDot:
                    pickerArray = new[] { dashhWidth, dotHole, dotWidth, dotHole, dotWidth, dotHole };
                    break;
                default:
                    return null;
            }

            // ToDo: lineCap?

            penWidth = Math.Max(penWidth, 1f);
            return pickerArray.Select(a => a * penWidth).ToArray();
        }
    }
}
