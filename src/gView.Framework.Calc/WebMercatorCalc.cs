using System;

namespace gView.Framework.Calc;

static public class WebMercatorCalc
{
    public const double EarthRadius = 6378137.0;
    public const int TileSIze = 256;
    private const double MeterPerInch = 0.0254;

    static public double MetersPerPixel(double zoom, double latitude = 0.0)
    {
        return (Math.Cos(latitude * Math.PI / 180.0)
            * 2 * Math.PI * EarthRadius)
            / (TileSIze * Math.Pow(2, zoom));
    }

    static public double MapScale(double zoom, double latitude = 0.0, double dpi = 96.0)
    {
        double metersPerPixel = MetersPerPixel(zoom, latitude);

        return metersPerPixel * dpi / MeterPerInch;
    }

    public static double Zoom(double mapScale, double latitude = 0.0, double dpi = 96.0)
    {
        double resolution = mapScale * MeterPerInch / dpi;
        double metersPerPixel = resolution / Math.Cos(latitude * Math.PI / 180);
        double zoom = Math.Log((2 * Math.PI * EarthRadius) / (256 * metersPerPixel), 2);

        return zoom;
    }
}

