using gView.GraphicsEngine;
using System;
using static gView.Framework.Symbology.SimplePointSymbol;

namespace gView.Interoperability.GeoServices.Rest.DTOs
{
    static public class Extensions
    {
        static public int[] ToArray(this ArgbColor color)
        {
            return new int[] { color.R, color.G, color.B, color.A };
        }

        static public ArgbColor ToColor(this int[] col)
        {
            if (col == null)
            {
                return ArgbColor.Transparent;
            }

            if (col.Length == 3)
            {
                return ArgbColor.FromArgb(col[0], col[1], col[2]);
            }

            if (col.Length == 4)
            {
                return ArgbColor.FromArgb(col[3], col[0], col[1], col[2]);
            }

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
            switch (markerType)
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

        static public LineDashStyle ToDashStyle(this string style)
        {
            switch (style)
            {
                case "esriSLSDash":
                    return LineDashStyle.Dash;
                case "esriSLSDashDot":
                    return LineDashStyle.DashDot;
                case "esriSLSDashDotDot":
                    return LineDashStyle.DashDotDot;
                case "esriSLSDot":
                    return LineDashStyle.Dot;
                case "esriSLSNull":
                case "esriSLSSolid":
                    return LineDashStyle.Solid;
            }

            return LineDashStyle.Solid;
        }

        static public string FromDashStyle(this LineDashStyle dashStyle)
        {
            switch (dashStyle)
            {
                case LineDashStyle.Solid:
                    return "esriSLSSolid";
                case LineDashStyle.Dot:
                    return "esriSLSDot";
                case LineDashStyle.DashDotDot:
                    return "esriSLSDashDotDot";
                case LineDashStyle.DashDot:
                    return "esriSLSDashDotDot";
                case LineDashStyle.Dash:
                    return "esriSLSDash";
            }
            return "esriSLSSolid";
        }

        /// <summary>
        /// Maps an esriSFS hatch style to a gView hatch style. Returns null for esriSFSSolid, esriSFSNull and unknown styles.
        /// </summary>
        static public HatchStyle? ToHatchStyle(this string style)
        {
            switch (style)
            {
                case "esriSFSBackwardDiagonal":
                    return HatchStyle.BackwardDiagonal;
                case "esriSFSForwardDiagonal":
                    return HatchStyle.ForwardDiagonal;
                case "esriSFSCross":
                    return HatchStyle.LargeGrid;
                case "esriSFSDiagonalCross":
                    return HatchStyle.DiagonalCross;
                case "esriSFSHorizontal":
                    return HatchStyle.Horizontal;
                case "esriSFSVertical":
                    return HatchStyle.Vertical;
            }

            return null;
        }

        static public string FromHatchStyle(this HatchStyle style)
        {
            switch (style)
            {
                case HatchStyle.BackwardDiagonal:
                case HatchStyle.DarkDownwardDiagonal:
                case HatchStyle.LightDownwardDiagonal:
                    return "esriSFSBackwardDiagonal";
                case HatchStyle.ForwardDiagonal:
                case HatchStyle.DarkUpwardDiagonal:
                case HatchStyle.LightUpwardDiagonal:
                    return "esriSFSForwardDiagonal";
                case HatchStyle.LargeGrid:
                case HatchStyle.SmallGrid:
                    return "esriSFSCross";
                case HatchStyle.DiagonalCross:
                    return "esriSFSDiagonalCross";
                case HatchStyle.Horizontal:
                case HatchStyle.DarkHorizontal:
                case HatchStyle.LightHorizontal:
                    return "esriSFSHorizontal";
                case HatchStyle.Vertical:
                case HatchStyle.DarkVertical:
                case HatchStyle.LightVertical:
                    return "esriSFSVertical";
            }

            return "esriSFSSolid";
        }
    }
}
