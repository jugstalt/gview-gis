/*
Copyright 2006 Jerry Huxtable

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using Proj4Net.Datum.Grids;
using Proj4Net.Utility;

namespace Proj4Net.Datum
{
    /// <summary>
    /// A class representing a geodetic datum.
    /// <para/>
    /// A geodetic datum consists of a set of reference points on or in the Earth,
    /// and a reference <see cref="Ellipsoid"/> giving an approximation 
    /// to the true shape of the geoid.
    /// <para/>
    /// In order to transform between two geodetic points specified 
    /// on different datums, it is necessary to transform between the 
    /// two datums.  There are various ways in which this
    /// datum conversion may be specified:
    /// <list type="Bullet">
    /// <item>A 3-parameter conversion</item>
    /// <item>A 7-parameter conversion</item>
    /// <item>A grid-shift conversion</item>
    /// </list>
    /// In order to be able to transform between any two datums, 
    /// the parameter-based transforms are provided as a transform to 
    /// the common WGS84 datum.  The WGS transforms of two arbitrary datum transforms can 
    /// be concatenated to provide a transform between the two datums.<para/>
    /// Notable datums in common use include <see cref="NAD83"/> and <see cref="WGS84"/>.
    /// </summary>
    public class Datum : IEquatable<Datum>
    {
        private const double Million = 1000000.0;

        public enum DatumTransformType
        {
            Unknown = 0,
            WGS84 = 1,
            ThreeParameters = 2,
            SevenParameters = 3,
            GridShift = 4
        }

        public static readonly Datum WGS84 = new Datum("WGS84", 0, 0, 0, Ellipsoid.WGS84, "WGS84");
        public static readonly Datum GGRS87 = new Datum("GGRS87", -199.87, 74.79, 246.62, Ellipsoid.GRS80, "Greek_Geodetic_Reference_System_1987");
        public static readonly Datum NAD83 = new Datum("NAD83", 0, 0, 0, Ellipsoid.GRS80, "North_American_Datum_1983");
        public static readonly Datum NAD27 = new Datum("NAD27", "@conus,@alaska,@ntv2_0.gsb,@ntv1_can.dat", Ellipsoid.CLARKE_1866, "North_American_Datum_1927");
        public static readonly Datum Potsdam = new Datum("potsdam", 606.0, 23.0, 413.0, Ellipsoid.BESSEL, "Potsdam Rauenberg 1950 DHDN");
        public static readonly Datum Carthage = new Datum("carthage", -263.0, 6.0, 431.0, Ellipsoid.CLARKE_1880, "Carthage 1934 Tunisia");
        public static readonly Datum Hermannskogel = new Datum("hermannskogel", 653.0, -212.0, 449.0, Ellipsoid.BESSEL, "Hermannskogel");
        public static readonly Datum IRE65 = new Datum("ire65", 482.530, -130.596, 564.557, -1.042, -0.214, -0.631, 8.15, Ellipsoid.MOD_AIRY, "Ireland 1965");
        public static readonly Datum OSGB36 = new Datum("OSGB36", 446.448, -125.157, 542.060, 0.1502, 0.2470, 0.8421, -20.4894, Ellipsoid.AIRY, "Airy 1830");
                                                          //+towgs84=446.448,-125.157, 542.06,  0.15,   0.247,  0.842,  -20.489 +units=m +no_defs

        private readonly String _code;
        private readonly String _name;
        private readonly Ellipsoid _ellipsoid;
        private readonly double[] _transform;
        private readonly string[] _grids;
        
        public Datum(String code,
            String transformSpec,
            Ellipsoid ellipsoid,
            String name)
            : this(code, (double[])null, ellipsoid, name)
        {
            transformSpec=transformSpec.Replace("@null", "");
            if (string.IsNullOrEmpty(transformSpec))
                return;
            
            _grids = transformSpec.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Creates an instance of this class with a <see cref="DatumTransformType.ThreeParameters"/> datum transform
        /// </summary>
        /// <param name="code">The code</param>
        /// <param name="deltaX">The offset in x-direction</param>
        /// <param name="deltaY">The offset in y-direction</param>
        /// <param name="deltaZ">The offset in z-direction</param>
        /// <param name="ellipsoid">The ellipsoid for this <see cref="Datum"/></param>
        /// <param name="name">The name of the</param>
        public Datum(String code,
            double deltaX, double deltaY, double deltaZ,
            Ellipsoid ellipsoid,
            String name)
            : this(code, new[] { deltaX, deltaY, deltaZ }, ellipsoid, name)
        {
        }

        /// <summary>
        /// Creates an instance of this class with a <see cref="DatumTransformType.SevenParameters"/> datum transform
        /// </summary>
        /// <param name="code">The code</param>
        /// <param name="deltaX">The offset in x-direction</param>
        /// <param name="deltaY">The offset in y-direction</param>
        /// <param name="deltaZ">The offset in z-direction</param>
        /// <param name="rx">The rotation around the x-axis in arc-seconds</param>
        /// <param name="ry">The rotation around the y-axis in arc-seconds</param>
        /// <param name="rz">The rotation around the z-axis in arc-secinds</param>
        /// <param name="scaleFactor">Scale factor</param>
        /// <param name="ellipsoid">The ellipsoid for this <see cref="Datum"/></param>
        /// <param name="name">The name of the</param>
        /// <remarks>
        /// PROJ4 towgs84 7-parameter transform uses 
        /// units of arc-seconds for the rotation factors, 
        /// and parts-per-million for the scale factor.
        /// </remarks>
        public Datum(String code,
            double deltaX, double deltaY, double deltaZ,
            double rx, double ry, double rz, double scaleFactor,
            Ellipsoid ellipsoid, String name)
            : this(code, new[] { deltaX, deltaY, deltaZ, 
                ProjectionMath.ArcSecondsToRadians(rx), 
                ProjectionMath.ArcSecondsToRadians(ry), 
                ProjectionMath.ArcSecondsToRadians(rz), 
                (scaleFactor / Million) + 1 }, ellipsoid, name)
        {
        }

        public Datum(String code,
            double[] transform,
            Ellipsoid ellipsoid,
            String name)
        {
            _code = code;
            _name = name;
            _ellipsoid = ellipsoid;
            if (transform != null)
                _transform = CheckTransformAllZero(transform);
        }

        private static Double[] CheckTransformAllZero(Double[] values)
        {
            for (var i = 0; i < values.Length; i++)
            {
                if (values[i] != 0d) return values;
            }
            return null;
        }

        /// <summary>
        /// Gets the code of the datum
        /// </summary>
        public String Code { get { return _code; } }

        /// <summary>
        /// Gets the name of the datum
        /// </summary>
        public string Name { get { return _name; } }

        public Ellipsoid Ellipsoid
        {
            get { return _ellipsoid; }
        }

        public override string ToString()
        {
            return string.Format("[Datum-{0}]", Name);
        }

        public double[] TransformToWGS84
        {
            get { return _transform; }
        }

        public DatumTransformType TransformType
        {
            get
            {
                if (_transform == null)
                {
                    return _grids != null
                        ? DatumTransformType.GridShift 
                        : DatumTransformType.WGS84;
                }

                if (IsIdentity(_transform)) return DatumTransformType.WGS84;

                if (_transform.Length == 3) return DatumTransformType.ThreeParameters;
                if (_transform.Length == 7) return DatumTransformType.SevenParameters;
                
                return DatumTransformType.WGS84;
            }
        }

        /// <summary>
        /// Tests whether the datum parameter-based transform 
        /// is the identity transform 
        /// (in which case datum transformation can be short-circuited,
        /// thus avoiding some loss of numerical precision).
        /// </summary>
        private static bool IsIdentity(double[] transform)
        {
            for (var i = 0; i < transform.Length; i++)
            {
                // scale factor will normally be 1 for an identity transform
                if (i == 6)
                {
                    if (transform[i] != 1.0 && transform[i] != 0.0)
                        return false;
                }
                else if (transform[i] != 0.0) return false;
            }
            return true;
        }
        public Boolean HasTransformToWGS84
        {
            get
            {
                var transformType = TransformType;
                return transformType == DatumTransformType.ThreeParameters || transformType == DatumTransformType.SevenParameters;
            }
        }

        public const double ELLIPSOID_E2_TOLERANCE = 0.000000000050;
  
        /// <summary>
        /// Tests if this is equal to another {@link Datum}.
        /// <para/>
        /// Datums are considered to be equal iff:
        /// <list type="Bullet">
        /// <item>their transforms are equal</item>
        /// <item>OR their ellipsoids are (approximately) equal</item>
        /// </list>
        /// </summary>
        /// <param name="datum">The datum to compare</param>
        /// <returns><c>true</c> if <paramref name="datum"/> equals this</returns>
        public bool Equals(Datum datum)
        {
            // false if tranforms are not equal
            if (TransformType != datum.TransformType)
            {
                return false;
            }

            // false if ellipsoids are not (approximately) equal
            if (_ellipsoid.EquatorRadius != _ellipsoid.EquatorRadius)
            {
                if (Math.Abs(_ellipsoid.EccentricitySquared
                     - datum._ellipsoid.EccentricitySquared) > ELLIPSOID_E2_TOLERANCE)
                    return false;
            }

            // false if transform parameters are not identical
            if (TransformType == DatumTransformType.ThreeParameters || TransformType == DatumTransformType.SevenParameters)
            {
                for (var i = 0; i < _transform.Length; i++)
                {
                    if (_transform[i] != datum._transform[i])
                        return false;
                }
                return true;
            }
            
            if(TransformType == DatumTransformType.GridShift)
            {
                if (_grids.Length != datum._grids.Length)
                    return false;

                var gridList = new List<string>(datum._grids);
                gridList.Sort();
                for (var i = 0; i < _grids.Length; i++)
                {
                    if (!gridList.Contains(_grids[i]))
                        return false;
                }
            }
            
            return true; // datums are equal

        }

        public void TransformFromGeocentricToWgs84(Coordinate p)
        {
            if (_transform.Length == 3)
            {
                p.X += _transform[0];
                p.Y += _transform[1];
                p.Z += _transform[2];

            }
            else if (_transform.Length == 7)
            {
                double Dx_BF = _transform[0];
                double Dy_BF = _transform[1];
                double Dz_BF = _transform[2];
                double Rx_BF = _transform[3];
                double Ry_BF = _transform[4];
                double Rz_BF = _transform[5];
                double M_BF = _transform[6];

                double x_out = M_BF * (p.X - Rz_BF * p.Y + Ry_BF * p.Z) + Dx_BF;
                double y_out = M_BF * (Rz_BF * p.X + p.Y - Rx_BF * p.Z) + Dy_BF;
                double z_out = M_BF * (-Ry_BF * p.X + Rx_BF * p.Y + p.Z) + Dz_BF;

                p.X = x_out;
                p.Y = y_out;
                p.Z = z_out;
            }
        }
        public void TransformToGeocentricFromWgs84(Coordinate p)
        {
            if (_transform.Length == 3)
            {
                p.X -= _transform[0];
                p.Y -= _transform[1];
                p.Z -= _transform[2];

            }
            else if (_transform.Length == 7)
            {
                double Dx_BF = _transform[0];
                double Dy_BF = _transform[1];
                double Dz_BF = _transform[2];
                double Rx_BF = _transform[3];
                double Ry_BF = _transform[4];
                double Rz_BF = _transform[5];
                double M_BF = _transform[6];

                double x_tmp = (p.X - Dx_BF) / M_BF;
                double y_tmp = (p.Y - Dy_BF) / M_BF;
                double z_tmp = (p.Z - Dz_BF) / M_BF;

                p.X = x_tmp + Rz_BF * y_tmp - Ry_BF * z_tmp;
                p.Y = -Rz_BF * x_tmp + y_tmp + Rx_BF * z_tmp;
                p.Z = Ry_BF * x_tmp - Rx_BF * y_tmp + z_tmp;
            }

        }
        public void ApplyGridShift(Coordinate c, bool inverse)
        {
            foreach (var grid in _grids)
            {
                var gridOptional = grid.StartsWith("@");
                var gridName = grid.StartsWith("@") ? grid.Substring(1) : grid;
                var uri = new Uri(System.IO.Path.Combine(".", gridName));
                var table = GridTable.Load(uri);
                if (table == null)
                {    
                    if (!gridOptional)
                        throw new Proj4NetException();
                    continue;
                }

                GridTable useTable;
                if(table.Applies(new PhiLambda { Lambda = c.Y, Phi = c.X}, out useTable))
                {
                    useTable.Apply(c, inverse);
                }
            }
        }
    }
}