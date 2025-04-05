using gView.Framework.Common;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Geometry.Proj;
using gView.Framework.Geometry.SpatialRefTranslation;
using Proj4Net.Core.Datum;
using System;
using System.Linq;
using System.Xml.Linq;

namespace gView.Framework.Geometry;

/// <summary>
/// 
/// </summary>

public class GeodeticDatum : IGeodeticDatum
{
    private double _X, _Y, _Z;
    private double _rX, _rY, _rZ;
    private double _scale;
    private string _name;
    private string _gridShiftFile = null;

    public GeodeticDatum()
    {
        _X = _Y = _Z = _rX = _rY = _rZ = _scale = 0.0;
        _name = "Unknown";
    }
    public GeodeticDatum(string name)
    {
        ProjDB db = new ProjDB(ProjDBTables.datums);
        var toWgs84 = db.GetDatumParameters(name);
        db.Dispose();
        Name = name;

        if (!String.IsNullOrEmpty(toWgs84))
        {
            this.Parameter = toWgs84;
            return;
        }

        if(GeometricTransformerFactory.SupportedGridShifts().Contains(name))
        {
            _gridShiftFile = name;
            return;
        }
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
        _gridShiftFile = datum._gridShiftFile;
    }
    public GeodeticDatum(string name, 
                        double dx, double dy, double dz,
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
            if (!String.IsNullOrEmpty(_gridShiftFile))
            {
                return $"+nadgrids={_gridShiftFile}";
            }

            return $"+towgs84={_X.ToDoubleString()},{_Y.ToDoubleString()},{_Z.ToDoubleString()},{_rX.ToDoubleString()},{_rY.ToDoubleString()},{_rZ.ToDoubleString()},{_scale.ToDoubleString()}";
        }
        set
        {
            value = value?.Trim();

            if (value?.StartsWith("+towgs84=") == true)
            {
                string[] p = value.Substring("+towgs84=".Length).Split(',');
                if (p.Length < 7)
                {
                    return;
                }

                try
                {
                    _X = p[0].ToDouble();
                    _Y = p[1].ToDouble();
                    _Z = p[2].ToDouble();
                    _rX = p[3].ToDouble();
                    _rY = p[4].ToDouble();
                    _rZ = p[5].ToDouble();
                    _scale = p[6].ToDouble();
                }
                catch { }
            }
            else if (value?.StartsWith("+nadgrids=") == true)
            {
                _gridShiftFile = value.Substring("+nadgrids=".Length);
            }
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
        _gridShiftFile = (string)stream.Load("gridShiftFile", null);
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
        if(!String.IsNullOrEmpty(_gridShiftFile))
        {
            stream.Save("gridShiftFile", _gridShiftFile);
        }
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
        datum._gridShiftFile = _gridShiftFile;

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

    public static readonly GeodeticDatum WGS84 = new GeodeticDatum("WGS84", 0D, 0D, 0D, 0D, 0D, 0D, 0D);
}

public class DatumsTransformation : IDatumsTransformation
{
    public IGeodeticDatum FromDatum { get; set; }

    public IGeodeticDatum ToDatum { get; private set; } = GeodeticDatum.WGS84;

    public IGeodeticDatum TransformationDatum { get; set; }

    #region IPersistable Member

    public void Load(IPersistStream stream)
    {
        FromDatum = stream.Load("FromDatum", null, new GeodeticDatum()) as GeodeticDatum;
        TransformationDatum = stream.Load("TransformationDatum", null, new GeodeticDatum()) as GeodeticDatum;
    }

    public void Save(IPersistStream stream)
    {
        if (FromDatum != null)
        {
            stream.Save("FromDatum", FromDatum);
        }

        if (TransformationDatum != null)
        {
            stream.Save("TransformationDatum", TransformationDatum);
        }
    }

    #endregion

    #region IClone Member

    public object Clone()
    {
        return new DatumsTransformation()
        {
            FromDatum = FromDatum?.Clone() as GeodeticDatum,
            ToDatum = ToDatum.Clone() as GeodeticDatum,
            TransformationDatum = TransformationDatum?.Clone() as GeodeticDatum
        };
    }

    #endregion
}
