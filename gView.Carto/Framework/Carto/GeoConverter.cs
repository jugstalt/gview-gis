using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Geometry;

namespace gView.Framework.Carto
{
    public class GeoUnitConverter
    {
        private double _R = 6378137.0; // m
        private const double RAD2DEG = (180.0 / Math.PI);

        public string[] Convert(string[] val, GeoUnits from, GeoUnits to,ISpatialParameters parameters)
        {
            if (val.Length != 2) return null;

            double x = 0.0, y = 0.0, phi = 0.0;
            if (from == GeoUnits.DegreesMinutesSeconds)
            {
                // Umwandelt in double
            }
            else
            {
                if(!double.TryParse(val[0], out x)) return null;
                if(!double.TryParse(val[1], out y)) return null;
            }

            if (from == GeoUnits.DegreesMinutesSeconds || from == GeoUnits.DecimalDegrees)
            {
                if (parameters != null)
                {
                    x += parameters.lon_0;
                    y += parameters.lat_0;
                }
                phi = y;
            }
            else
            {
                if (parameters != null)
                {
                    x -= Convert(parameters.x_0, parameters.Unit, from);
                    y -= Convert(parameters.y_0, parameters.Unit, from);
                }
            }
            
            double Xm = ToMeters(x, from, phi,1);
            double Ym = ToMeters(y, from, 0.0,1);

            double Yt = FromMeters(Ym, to, 0.0,1),Xt;
            if (to == GeoUnits.DegreesMinutesSeconds || to == GeoUnits.DecimalDegrees)
            {
                phi = Yt + ((parameters != null) ? parameters.lat_0 : 0.0);
                Xt = FromMeters(Xm, to, phi,1) + ((parameters != null) ? parameters.lon_0 : 0.0);
            }
            else
            {
                Xt = FromMeters(Xm, to, 0.0,1);
            }

            if (to == GeoUnits.DegreesMinutesSeconds)
            {
                return new string[] { deg2GMS(Xt, 2), deg2GMS(Yt, 2) };
            }
            else if (to == GeoUnits.DecimalDegrees)
            {
                Xt = Math.Round(Xt, 2);
                Yt = Math.Round(Yt, 2);

                return new string[] { Xt.ToString(), Yt.ToString() };
            }
            else
            {
                if (parameters != null)
                {
                    Xt += Convert(parameters.x_0, parameters.Unit, to);
                    Yt += Convert(parameters.y_0, parameters.Unit, to);
                }
                Xt = Math.Round(Xt, 2);
                Yt = Math.Round(Yt, 2);

                return new string[] { String.Format("{0:+0.00;-0.00;}", Xt), String.Format("{0:+0.00;-0.00;}", Yt) };
            }
        }

        public double Convert(double val, GeoUnits from, GeoUnits to)
        {
            return Convert(val, from, to, 1, 0.0);
        }
        internal double Convert(double val, GeoUnits from, GeoUnits to, int dim)
        {
            return Convert(val, from, to, dim, 0.0);
        }

        internal double Convert(double val, GeoUnits from, GeoUnits to, int dim, double phi)
        {
            double m = ToMeters(val, from, phi, dim);
            return FromMeters(m, to, phi, dim);
        }

        public double R
        {
            get { return _R; }
            set { _R = value; }
        }

        private double ToMeters(double val, GeoUnits unit, double phi, int dim)
        {
            // http://jumk.de/calc/index.shtml
            if (dim <= 0) dim = 1;
            switch (unit)
            {
                case GeoUnits.Unknown:
                    return val;
                case GeoUnits.Inches:
                    return val * Math.Pow(0.0254, dim);
                case GeoUnits.Feet:
                    return val * Math.Pow(0.3048, dim);
                case GeoUnits.Yards:
                    return val * Math.Pow(0.9144, dim);
                case GeoUnits.Miles:
                    return val * Math.Pow(1609.344, dim);
                case GeoUnits.NauticalMiles:
                    return val * Math.Pow(1852.01, dim);
                case GeoUnits.Millimeters:
                    return val * Math.Pow(0.001, dim);
                case GeoUnits.Centimeters:
                    return val * Math.Pow(0.01, dim);
                case GeoUnits.Decimeters:
                    return val * Math.Pow(0.1, dim);
                case GeoUnits.Meters:
                    return val;
                case GeoUnits.Kilometers:
                    return val * Math.Pow(1000.0, dim);
                case GeoUnits.DegreesMinutesSeconds:
                case GeoUnits.DecimalDegrees:
                    if (dim > 1) return 0.0;
                    return val / RAD2DEG * _R * Math.Cos(phi / RAD2DEG);
            }
            return 0.0;
        }

        private double FromMeters(double val, GeoUnits unit, double phi, int dim)
        {
            if (dim <= 0) dim = 1;
            switch (unit)
            {
                case GeoUnits.Unknown:
                    return val;
                case GeoUnits.Inches:
                    return val * Math.Pow(39.37008, dim);
                case GeoUnits.Feet:
                    return val * Math.Pow(3.28084, dim);
                case GeoUnits.Yards:
                    return val * Math.Pow(1.09361, dim);
                case GeoUnits.Miles:
                    return val * Math.Pow(0.000621371, dim);
                case GeoUnits.NauticalMiles:
                    return val * Math.Pow(0.000539954, dim);
                case GeoUnits.Millimeters:
                    return val * Math.Pow(1000.0, dim);
                case GeoUnits.Centimeters:
                    return val * Math.Pow(100.0, dim);
                case GeoUnits.Decimeters:
                    return val * Math.Pow(10.0, dim);
                case GeoUnits.Meters:
                    return val;
                case GeoUnits.Kilometers:
                    return val * Math.Pow(0.001, dim);
                case GeoUnits.DegreesMinutesSeconds:
                case GeoUnits.DecimalDegrees:
                    if (Math.Cos(phi / RAD2DEG) == 0.0) return 0.0;
                    return val / (R * Math.Cos(phi / RAD2DEG)) * RAD2DEG;
            }
            return 0.0;
        }

        static public double GMS2deg(string gms) 
        {
            gms = gms.Replace("\u00b0", " ");   // degree sign: °
            gms = gms.Replace("''", " ");
            gms = gms.Replace("'", " ");
            gms = gms.Replace("\"", " ");
            gms = gms.Replace(".", ",");

            replaceDoubleSpace(ref gms);
            gms = gms.TrimEnd();
            gms = gms.TrimStart();
            gms = gms.Replace(" ", ";");

            string[] GMS = gms.Split(';');
            double deg = 0.0;
            if (GMS.Length > 0) deg += System.Convert.ToDouble(GMS[0]);
            if (GMS.Length > 1) deg += System.Convert.ToDouble(GMS[1]) / 60.0;
            if (GMS.Length > 2) deg += System.Convert.ToDouble(GMS[2]) / 3600.0;

            return deg;
        }

        static public string deg2GMS(double deg, int digits)
        {
            int g = (int)Math.Floor(deg);
            deg -= g;
            int m = (int)Math.Floor(deg * 60);
            deg -= (double)(m) / 60.0;
            double s = Math.Round(deg * 3600.0, 3);

            if (s >= 60.0) { m++; s = 0.0; }
            if (m == 60.0) { g++; m = 0; }

            //int digits=getCoordDigits();
            string digs = "";
            if (digits > 0) digs = ".";
            for (int i = 0; i < digits; i++) digs += "0";

            return String.Format("{0}\u00b0{1:00}'{2:00" + digs + "}''", g, m, s);
            //return g.ToString()+"\u00b0"+m.ToString()+"'"+s.ToString()+"''";
        }

        static private void replaceDoubleSpace(ref string str)
        {
            str = str.Replace("  ", " ");
            if (str.IndexOf("  ") != -1) replaceDoubleSpace(ref str);
        }
    }
}
