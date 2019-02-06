using System;
using System.Collections.Generic;
using System.Text;
using static gView.Framework.Symbology.SimplePointSymbol;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    static public class Extensions
    {
        static public int[] ToArray(this System.Drawing.Color color)
        {
            if (color == null)
                return null;

            return new int[] { color.R, color.G, color.B, color.A };
        }

        static public System.Drawing.Color ToColor(this int[] col)
        {
            if (col == null)
                return System.Drawing.Color.Transparent;
            if (col.Length == 3)
                return System.Drawing.Color.FromArgb(col[0], col[1], col[2]);
            if (col.Length == 4)
                return System.Drawing.Color.FromArgb(col[3], col[0], col[1], col[2]);

            throw new Exception("Invalid symbol color: [" + String.Join(",", col) + "]");
        }


        static public MarkerType ToMarkerType(this string style)
        {
            switch (style)
            {
                case "esriSMSCircle":
                    return MarkerType.Circle;
                case "esriSMSCross":
                    return MarkerType.Cross;
                case "esriSMSDiamond":
                case "esriSMSSquare":
                    return MarkerType.Square;
                case "esriSMSX":
                    return MarkerType.Star;
                case "esriSMSTriangle":
                    return MarkerType.Triangle;

            }

            return MarkerType.Circle;
        }

        static public string FromMarkerType(this MarkerType markerType)
        {
            switch(markerType)
            {
                case MarkerType.Circle:
                    return "esriSMSCircle";
                case MarkerType.Cross:
                    return "esriSMSCross";
                case MarkerType.Square:
                    return "esriSMSSquare";
                case MarkerType.Star:
                    return "esriSMSX";
                case MarkerType.Triangle:
                    return "esriSMSTriangle";
            }

            return "esriSMSCircle";
        }

        static public System.Drawing.Drawing2D.DashStyle ToDashStyle(this string style)
        {
            switch(style)
            {
                case "esriSLSDash":
                    return System.Drawing.Drawing2D.DashStyle.Dash;
                case "esriSLSDashDot":
                    return System.Drawing.Drawing2D.DashStyle.DashDot;
                case "esriSLSDashDotDot":
                    return System.Drawing.Drawing2D.DashStyle.DashDotDot;
                case "esriSLSDot":
                    return System.Drawing.Drawing2D.DashStyle.Dot;
                case "esriSLSNull":
                case "esriSLSSolid":
                    return System.Drawing.Drawing2D.DashStyle.Solid;
            }

            return System.Drawing.Drawing2D.DashStyle.Solid;
        }

        static public string FromDashStyle(this System.Drawing.Drawing2D.DashStyle dashStyle)
        {
            switch(dashStyle)
            {
                case System.Drawing.Drawing2D.DashStyle.Solid:
                    return "esriSLSSolid";
                case System.Drawing.Drawing2D.DashStyle.Dot:
                    return "esriSLSDot";
                case System.Drawing.Drawing2D.DashStyle.DashDotDot:
                    return "esriSLSDashDotDot";
                case System.Drawing.Drawing2D.DashStyle.DashDot:
                    return "esriSLSDashDotDot";
                case System.Drawing.Drawing2D.DashStyle.Dash:
                    return "esriSLSDash";
            }
            return "esriSLSSolid";
        }
    }
}
