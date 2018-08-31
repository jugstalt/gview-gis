using System;
using Proj4Net.Datum;

namespace Proj4Net.Parser
{
    ///<summary>
    /// Contains the parsed/computed parameter values which are used to create 
    /// the datum and ellipsoid for a <see cref="CoordinateReferenceSystem"/>.
    /// This class also implements the policies for which parameters take precedence
    /// when multiple inconsistent ones are present.
    ///</summary>
    public class DatumParameters
    {
        // TODO: check for inconsistent _datum and _ellipsoid (some PROJ4 cs specify both - not sure why)

        // ReSharper disable InconsistentNaming
        private const double OneSixth = .1666666666666666667; /* 1/6 */
        private const double RA4 = .04722222222222222222; /* 17/360 */
        private const double RA6 = .02215608465608465608; /* 67/3024 */
        //private const double RV4 = .06944444444444444444; /* 5/72 */
        //private const double RV6 = .04243827160493827160; /* 55/1296 */
        // ReSharper restore InconsistentNaming

        private Datum.Datum _datum;
        private double[] _datumTransform;

        private Ellipsoid _ellipsoid;
        private double _a = Double.NaN;
        private double _es = Double.NaN;

        private string _nadGrids;

        public DatumParameters()
        {
            // Default _datum is WGS84
            //    setDatum(Datum.WGS84);
        }

        public Datum.Datum Datum
        {
            get
            {
                if (_datum != null)
                    return _datum;
                // if no _ellipsoid was specified, return WGS84 as the default
                if (_ellipsoid == null && !IsDefinedExplicitly)
                {
                    return Proj4Net.Datum.Datum.WGS84;
                }
                // if _ellipsoid was WGS84, return that _datum
                if (_ellipsoid == Ellipsoid.WGS84)
                    return Proj4Net.Datum.Datum.WGS84;

                if (!string.IsNullOrEmpty(_nadGrids))
                    return new Datum.Datum("User", _nadGrids, _ellipsoid, "User-defined");

                // otherwise, return _a custom _datum with the specified _ellipsoid
                return new Datum.Datum("User", _datumTransform, Ellipsoid, "User-defined");
            }

            set
            {
                _datum = value;
            }
        }

        private Boolean IsDefinedExplicitly
        {
            get { return !(Double.IsNaN(_a) || Double.IsNaN(_es)); }
        }

        public Ellipsoid Ellipsoid
        {
            get
            {
                if (_ellipsoid != null)
                    return _ellipsoid;
                return new Ellipsoid("user", _a, _es, "User-defined");
            }
            set
            {
                _ellipsoid = value;
                _es = value.EccentricitySquared;
                _a = value.EquatorRadius;
            }
        }

        public void SetDatumTransform(double[] datumTransform)
        {
            _datumTransform = datumTransform;
            // force new Datum to be created
            _datum = null;
        }

        public Double EquatorRadius
        {
            get { return _a; }
            set
            {
                _ellipsoid = null; // force user-defined _ellipsoid
                _a = value;
            }
        }

        [Obsolete("Use EquatorRadius")]
        public Double A
        {
            get { return _a; }
            set
            {
                _ellipsoid = null; // force user-defined _ellipsoid
                _a = value;
            }
        }

        public void SetB(double b)
        {
            _ellipsoid = null;  // force user-defined _ellipsoid
            _es = 1.0 - (b * b) / (_a * _a);
        }

        public double EccentricitySquared
        {
            get { return _es; }
            set
            {
                _ellipsoid = null; // force user-defined _ellipsoid
                _es = value;
            }
        }

        public void SetRF(double rf)
        {
            _ellipsoid = null;  // force user-defined _ellipsoid
            _es = rf * (2.0 - rf);
        }

        public void setR_A()
        {
            _ellipsoid = null;  // force user-defined _ellipsoid
            _a *= 1.0 - _es * (OneSixth + _es * (RA4 + _es * RA6));
        }

        public void SetF(double f)
        {
            _ellipsoid = null;  // force user-defined _ellipsoid
            double rf = 1.0 / f;
            _es = rf * (2.0 - rf);
        }

        public void SetNadGrids(string grids)
        {
            
        }

    }
}