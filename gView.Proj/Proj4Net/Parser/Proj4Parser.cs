using System;
using System.Collections.Generic;
using System.Globalization;
using Proj4Net.Datum;
using Proj4Net.Projection;
using Proj4Net.Units;

namespace Proj4Net.Parser
{
    public class Proj4Parser
    {
        /* SECONDS_TO_RAD = Pi/180/3600 */
        private const double SecondsToRad = 4.84813681109535993589914102357e-6;
        private const double Million = 1000000.0;
        
        private Registry registry;

        public Proj4Parser(Registry registry)
        {
            this.registry = registry;
        }

        public CoordinateReferenceSystem Parse(String name, String[] args)
        {
            if (args == null)
                return null;

            IDictionary<String, String> parameters = CreateParameterMap(args);
            Proj4Keyword.CheckUnsupported(parameters.Keys);
            DatumParameters datumParam = new DatumParameters();
            ParseDatum(parameters, datumParam);
            ParseEllipsoid(parameters, datumParam);
            Datum.Datum datum = datumParam.Datum;
            Ellipsoid ellipsoid = datum.Ellipsoid;
            // TODO: this makes a difference - why?
            // which is better?
            //    Ellipsoid ellipsoid = datumParam.getEllipsoid(); 
            Projection.Projection proj = ParseProjection(parameters, ellipsoid);
            return new CoordinateReferenceSystem(name, args, datum, proj);
        }

        /*
  
        // not currently used
       private const double SIXTH = .1666666666666666667; // 1/6 
       private const double RA4 = .04722222222222222222; // 17/360 
       private const double RA6 = .02215608465608465608; // 67/3024 
       private const double RV4 = .06944444444444444444; // 5/72 
       private const double RV6 = .04243827160493827160; // 55/1296 
       */

        private static readonly AngleFormat format = new AngleFormat(AngleFormat.PatternDDMMSS1, true);

        ///<summary>
        /// Creates a <see cref="Projection"/> initialized from a PROJ.4 argument list.
        ///</summary>
        private Projection.Projection ParseProjection(IDictionary<String, String> parameters, Ellipsoid ellipsoid)
        {
            Projection.Projection projection = null;

            String s;
            if (parameters.TryGetValue(Proj4Keyword.proj, out s))
            {
                projection = registry.GetProjection(s, parameters);
            }

            if (projection == null)
                throw new ArgumentException("Unknown projection: " + s);

            projection.Ellipsoid = ellipsoid;

            // not sure what CSes use this??
            /*
            s = (String)parameters.TryGetValue( "init" );
            if ( s != null ) {
              projection = CreateFromName( s ).getProjection();
              if ( projection == null )
                throw new ProjectionException( "Unknown projection: "+s );
                    a = projection.getEquatorRadius();
                    es = projection.getEllipsoid().getEccentricitySquared();
            }
            */


            //TODO: better error handling for things like bad number syntax.  
            // Should be able to report the original param string in the error message
            // Also should the exception be lib specific?  (Say ParseException)

            // Other parameters
            //   projection.ProjectionLatitudeDegrees = 0;
            //   projection.ProjectionLatitude1Degrees = 0;
            //   projection.ProjectionLatitude2Degrees = 0;
            if (parameters.TryGetValue(Proj4Keyword.alpha, out s))
                projection.AlphaDegrees = Double.Parse(s, CultureInfo.InvariantCulture);

            if (parameters.TryGetValue(Proj4Keyword.lonc, out s))
                projection.LonCDegrees = Double.Parse(s, CultureInfo.InvariantCulture);

            if (parameters.TryGetValue(Proj4Keyword.lat_0, out s))
                projection.ProjectionLatitudeDegrees = ParseAngle(s);

            if (parameters.TryGetValue(Proj4Keyword.lon_0, out s))
                projection.ProjectionLongitudeDegrees = ParseAngle(s);

            if (parameters.TryGetValue(Proj4Keyword.lat_1, out s))
                projection.ProjectionLatitude1Degrees = ParseAngle(s);

            if (parameters.TryGetValue(Proj4Keyword.lat_2, out s))
                projection.ProjectionLatitude2Degrees = ParseAngle(s);

            if (parameters.TryGetValue(Proj4Keyword.lat_ts, out s))
                projection.TrueScaleLatitudeDegrees = ParseAngle(s);

            if (parameters.TryGetValue(Proj4Keyword.x_0, out s))
                projection.FalseEasting = Double.Parse(s, CultureInfo.InvariantCulture);

            if (parameters.TryGetValue(Proj4Keyword.y_0, out s))
                projection.FalseNorthing = Double.Parse(s, CultureInfo.InvariantCulture);

            if (!parameters.TryGetValue(Proj4Keyword.k_0, out s))
                if (!parameters.TryGetValue(Proj4Keyword.k, out s)) s = null;
            if (s != null)
                projection.ScaleFactor = Double.Parse(s, CultureInfo.InvariantCulture);

            if (parameters.TryGetValue(Proj4Keyword.units, out s))
            {
                Unit unit = Units.Units.FindUnit(s);
                // TODO: report unknown units name as error
                if (unit != null)
                {
                    projection.FromMetres = (1.0 / unit.Value);
                    projection.Unit = unit;
                }
            }

            if (parameters.TryGetValue(Proj4Keyword.to_meter, out s))
                projection.FromMetres = (1.0 / Double.Parse(s, CultureInfo.InvariantCulture));

            if (parameters.ContainsKey(Proj4Keyword.south))
                projection.SouthernHemisphere = true;

            if (parameters.TryGetValue(Proj4Keyword.pm, out s))
            {
                double pm;
                projection.PrimeMeridian = double.TryParse(s, out pm) 
                    ? Meridian.CreateByDegree(pm) 
                    : Meridian.CreateByName(s);
            }

            //TODO: implement some of these parameters ?

            // this must be done last, since behaviour depends on other parameters being set (eg +south)
            if (projection is TransverseMercatorProjection)
            {
                if (parameters.TryGetValue("zone", out s))
                    ((TransverseMercatorProjection)projection).UTMZone = int.Parse(s);
            }

            projection.Initialize();

            return projection;
        }

        private void ParseDatum(IDictionary<String, String> parameters, DatumParameters datumParam)
        {
            String towgs84;
            if (parameters.TryGetValue(Proj4Keyword.towgs84, out towgs84))
            {
                double[] datumConvparameters = ParseToWGS84(towgs84);
                datumParam.SetDatumTransform(datumConvparameters);
            }

            String code;
            if (parameters.TryGetValue(Proj4Keyword.datum, out code))
            {
                Datum.Datum datum = registry.GetDatum(code);
                if (datum == null)
                    throw new ArgumentException("Unknown datum: " + code);
                datumParam.Datum = datum;
            }

            string grids;
            if (parameters.TryGetValue(Proj4Keyword.nadgrids, out grids))
                datumParam.SetNadGrids(grids);
        }

        private static double[] ParseToWGS84(String paramList)
        {
            var numStr = paramList.Split(',');

            if (!(numStr.Length == 3 || numStr.Length == 7))
            {
                throw new ArgumentException("Invalid number of values (must be 3 or 7) in +towgs84: " + paramList);
            }
            var param = new double[numStr.Length];
            for (var i = 0; i < numStr.Length; i++)
            {
                Double val;
                if (!Double.TryParse(numStr[i], NumberStyles.Any, CultureInfo.InvariantCulture, out val))
                    throw new ArgumentException("Cannot parse '" + numStr[i] + "' to double.");
                param[i] = val;
            }

            // optimization to detect 3-parameter transform
            if (param[3] == 0.0 && param[4] == 0.0 && 
                param[5] == 0.0 && param[6] == 0.0 )
            {
                param = new [] { param[0], param[1], param[2] };
            }
   

            /**
             * PROJ4 towgs84 7-parameter transform uses 
             * units of arc-seconds for the rotation factors, 
             * and parts-per-million for the scale factor.
             * These need to be converted to radians and a scale factor. 
             */
            if (param.Length > 3)
            {
                param[3] *= SecondsToRad;
                param[4] *= SecondsToRad;
                param[5] *= SecondsToRad;
                param[6] = (param[6] / Million) + 1;
            }

            return param;
        }

        private void ParseEllipsoid(IDictionary<String, String> parameters, DatumParameters datumParam)
        {
            double b = 0;
            String s;

            /*
             * // not supported by PROJ4 s = (String) parameters.TryGetValue(Proj4Param.R); if (s !=
             * null) a = Double.parseDouble(s);
             */

            String code;
            if (parameters.TryGetValue(Proj4Keyword.ellps, out code))
            {
                Ellipsoid ellipsoid = registry.GetEllipsoid(code);
                if (ellipsoid == null)
                    throw new ArgumentException("Unknown ellipsoid: " + code);
                datumParam.Ellipsoid = ellipsoid;
            }

            /*
             * Explicit parameters override ellps and datum settings
             */
            if (parameters.TryGetValue(Proj4Keyword.a, out s))
            {
                double a = Double.Parse(s, CultureInfo.InvariantCulture);
                datumParam.EquatorRadius = a;
            }

            if (parameters.TryGetValue(Proj4Keyword.es, out s))
            {
                double es = Double.Parse(s, CultureInfo.InvariantCulture);
                datumParam.EccentricitySquared = es;
            }

            if (parameters.TryGetValue(Proj4Keyword.rf, out s))
            {
                double rf = Double.Parse(s, CultureInfo.InvariantCulture);
                datumParam.SetRF(rf);
            }

            if (parameters.TryGetValue(Proj4Keyword.f, out s))
            {
                double f = Double.Parse(s, CultureInfo.InvariantCulture);
                datumParam.SetF(f);
            }

            if (parameters.TryGetValue(Proj4Keyword.b, out s))
            {
                b = Double.Parse(s, CultureInfo.InvariantCulture);
                datumParam.SetB(b);
            }

            if (b == 0)
            {
                b = datumParam.EquatorRadius * Math.Sqrt(1.0 - datumParam.EccentricitySquared);
            }

            ParseEllipsoidModifiers(parameters, datumParam);

            /*
             * // None of these appear to be supported by PROJ4 ??
             * 
             * s = (String)
             * parameters.TryGetValue(Proj4Param.R_A); if (s != null && Boolean.getBoolean(s)) { a *=
             * 1. - es * (SIXTH + es * (RA4 + es * RA6)); } else { s = (String)
             * parameters.TryGetValue(Proj4Param.R_V); if (s != null && Boolean.getBoolean(s)) { a *=
             * 1. - es * (SIXTH + es * (RV4 + es * RV6)); } else { s = (String)
             * parameters.TryGetValue(Proj4Param.R_a); if (s != null && Boolean.getBoolean(s)) { a =
             * .5 * (a + b); } else { s = (String) parameters.TryGetValue(Proj4Param.R_g); if (s !=
             * null && Boolean.getBoolean(s)) { a = Math.sqrt(a * b); } else { s =
             * (String) parameters.TryGetValue(Proj4Param.R_h); if (s != null &&
             * Boolean.getBoolean(s)) { a = 2. * a * b / (a + b); es = 0.; } else { s =
             * (String) parameters.TryGetValue(Proj4Param.R_lat_a); if (s != null) { double tmp =
             * Math.sin(ParseAngle(s)); if (Math.abs(tmp) > MapMath.HALFPI) throw new
             * ProjectionException("-11"); tmp = 1. - es * tmp * tmp; a *= .5 * (1. - es +
             * tmp) / (tmp * Math.sqrt(tmp)); es = 0.; } else { s = (String)
             * parameters.TryGetValue(Proj4Param.R_lat_g); if (s != null) { double tmp =
             * Math.sin(ParseAngle(s)); if (Math.abs(tmp) > MapMath.HALFPI) throw new
             * ProjectionException("-11"); tmp = 1. - es * tmp * tmp; a *= Math.sqrt(1. -
             * es) / tmp; es = 0.; } } } } } } } }
             */
        }

        /**
         * Parse ellipsoid modifiers.
         * 
         * @param parameters
         * @param datumParam
         */
        private static void ParseEllipsoidModifiers(IDictionary<String, String> parameters, DatumParameters datumParam)
        {
            /**
             * Modifiers are mutually exclusive, so when one is detected method returns
             */
            if (parameters.ContainsKey(Proj4Keyword.R_A))
            {
                datumParam.setR_A();
                return;
            }

        }

        private static IDictionary<String, String> CreateParameterMap(String[] args)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            for (int i = 0; i < args.Length; i++)
            {
                String arg = args[i];
                if (arg.StartsWith("+"))
                {
                    int index = arg.IndexOf('=');
                    if (index != -1)
                    {
                        // parameters of form +pppp=vvvv
                        String key = arg.Substring(1, index - 1);
                        String value = arg.Substring(index + 1);
                        parameters.Add(key, value);
                    }
                    else
                    {
                        // parameters of form +ppppp
                        String key = arg.Substring(1);
                        if (!parameters.ContainsKey(key))
                        parameters.Add(key, null);
                    }
                }
            }
            return parameters;
        }

        private static double ParseAngle(String s)
        {
            return format.Parse(s);
        }

    }
}