using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Geometry.Proj;
using gView.Framework.Geometry.SpatialRefTranslation;
using System;

namespace gView.Framework.Geometry
{
    /// <summary>
    /// 
    /// </summary>

    public class GeodeticDatum : IGeodeticDatum
    {
        private double _X, _Y, _Z;
        private double _rX, _rY, _rZ;
        private double _scale;
        private string _name;

        private static IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

        public GeodeticDatum()
        {
            _X = _Y = _Z = _rX = _rY = _rZ = _scale = 0.0;
            _name = "Unknown";
        }
        public GeodeticDatum(string name)
        {
            ProjDB db = new ProjDB(ProjDBTables.datums);
            Parameter = db.GetDatumParameters(name);
            db.Dispose();
            Name = name;
        }
        public GeodeticDatum(GeodeticDatum datum)
        {
            _X = datum._X;
            _Y = datum._Y;
            _Z = datum._Z;
            _rX = datum._rX;
            _rY = datum._rY;
            _rZ = datum._rZ;
            _scale = datum._scale;
            _name = datum._name;
        }
        public GeodeticDatum(string name, double dx, double dy, double dz,
                                         double rx, double ry, double rz,
                                         double scale)
        {
            _name = name;
            _X = dx;
            _Y = dy;
            _Z = dz;
            _rX = rx;
            _rY = ry;
            _rZ = rz;
            _scale = scale;
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Parameter
        {
            get
            {
                return "+towgs84=" +
                    _X.ToString().Replace(",", ".") + "," +
                    _Y.ToString().Replace(",", ".") + "," +
                    _Z.ToString().Replace(",", ".") + "," +
                    _rX.ToString().Replace(",", ".") + "," +
                    _rY.ToString().Replace(",", ".") + "," +
                    _rZ.ToString().Replace(",", ".") + "," +
                    _scale.ToString().Replace(",", ".");
            }
            set
            {
                string[] p = value.Replace("+towgs84=", "").Split(',');
                if (p.Length < 7)
                {
                    return;
                }

                try
                {
                    _X = Convert.ToDouble(p[0], _nhi);
                    _Y = Convert.ToDouble(p[1], _nhi);
                    _Z = Convert.ToDouble(p[2], _nhi);
                    _rX = Convert.ToDouble(p[3], _nhi);
                    _rY = Convert.ToDouble(p[4], _nhi);
                    _rZ = Convert.ToDouble(p[5], _nhi);
                    _scale = Convert.ToDouble(p[6], _nhi);
                }
                catch { }
            }
        }

        #region IGeodeticDatum Member

        public double X_Axis
        {
            get
            {
                return _X;
            }
            set
            {
                _X = value;
            }
        }

        public double Y_Axis
        {
            get
            {
                return _Y;
            }
            set
            {
                _Y = value;
            }
        }

        public double Z_Axis
        {
            get
            {
                return _Z;
            }
            set
            {
                _Z = value;
            }
        }

        public double X_Rotation
        {
            get
            {
                return _rX;
            }
            set
            {
                _rX = value;
            }
        }

        public double Y_Rotation
        {
            get
            {
                return _rY;
            }
            set
            {
                _rY = value;
            }
        }

        public double Z_Rotation
        {
            get
            {
                return _rZ;
            }
            set
            {
                _rZ = value;
            }
        }

        public double Scale_Diff
        {
            get
            {
                return _scale;
            }
            set
            {
                _scale = value;
            }
        }

        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            _name = (string)stream.Load("name", "");
            _X = (double)stream.Load("X", 0.0);
            _Y = (double)stream.Load("Y", 0.0);
            _Z = (double)stream.Load("Z", 0.0);
            _rX = (double)stream.Load("rX", 0.0);
            _rY = (double)stream.Load("rY", 0.0);
            _rZ = (double)stream.Load("rZ", 0.0);
            _scale = (double)stream.Load("scale", 0.0);
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("name", _name);
            stream.Save("X", _X);
            stream.Save("Y", _Y);
            stream.Save("Z", _Z);
            stream.Save("rX", _rX);
            stream.Save("rY", _rY);
            stream.Save("rZ", _rZ);
            stream.Save("scale", _scale);
        }

        #endregion

        #region IClone Member

        public object Clone()
        {
            GeodeticDatum datum = new GeodeticDatum();

            datum._X = _X;
            datum._Y = _Y;
            datum._Z = _Z;
            datum._rX = _rX;
            datum._rY = _rY;
            datum._rZ = _rZ;
            datum._scale = _scale;
            datum._name = _name;

            return datum;
        }

        #endregion

        public static GeodeticDatum FromESRIWKT(string wkt)
        {
            object obj = ESRIGeotransWktCoordinateReader.Create(wkt);
            if (obj is Geotransformation)
            {
                return ((Geotransformation)obj).CreateGeodeticDatum();
            }
            return null;
        }
    }

}
