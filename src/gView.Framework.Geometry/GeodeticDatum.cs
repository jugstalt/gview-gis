using gView.Framework.Common;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Geometry.Proj;
using gView.Framework.Geometry.SpatialRefTranslation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gView.Framework.Geometry;

/// <summary>
/// 
/// </summary>

public class GeodeticDatum : IGeodeticDatum
{
    private const double DoubleTolerance = 1e-8;

    private double _X, _Y, _Z;
    private double _rX, _rY, _rZ;
    private double _scale;
    private string _name;

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

        foreach (var namePart in name.Split(","))
        {
            if (GeometricTransformerFactory.SupportedGridShifts().Select(g => g.shortName).Contains(namePart))
            {
                GridShiftFile = name;
                return;
            }
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
        GridShiftFile = datum.GridShiftFile;
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

    public GeodeticDatum(string name,
                         string gridShiftFile)
    {
        _name = name;
        GridShiftFile = gridShiftFile;
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
            if (!String.IsNullOrEmpty(GridShiftFile))
            {
                return $"+nadgrids={GridShiftFile}";
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
                GridShiftFile = value.Substring("+nadgrids=".Length);
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

    public string GridShiftFile { get; set; }

    public bool IsEqual(IGeodeticDatum geodeticDatum, bool equalName, bool equalParameter)
    {
        if (geodeticDatum == null)
        {
            return false;
        }

        bool hasEqualName = false, hasEqualParameters = false;
        if (equalName)
        {
            hasEqualName = this.Name.Equals(geodeticDatum.Name, StringComparison.OrdinalIgnoreCase);
        }

        //if (equalParameter && !this.Parameter.Equals(geodeticDatum.Parameter, StringComparison.OrdinalIgnoreCase))
        //{
        //    return false;
        //}

        if (equalParameter)
        {
            if (!String.IsNullOrEmpty(GridShiftFile))
            {
                hasEqualParameters = GridShiftFile.Equals(geodeticDatum.GridShiftFile);
            }
            else
            {
                hasEqualParameters =
                    this.X_Axis.EqualWithTolerance(geodeticDatum.X_Axis, DoubleTolerance) &&
                    this.Y_Axis.EqualWithTolerance(geodeticDatum.Y_Axis, DoubleTolerance) &&
                    this.Z_Axis.EqualWithTolerance(geodeticDatum.Z_Axis, DoubleTolerance) &&
                    this.X_Rotation.EqualWithTolerance(geodeticDatum.X_Rotation, DoubleTolerance) &&
                    this.Y_Rotation.EqualWithTolerance(geodeticDatum.Y_Rotation, DoubleTolerance) &&
                    this.Z_Rotation.EqualWithTolerance(geodeticDatum.Z_Rotation, DoubleTolerance) &&
                    this.Scale_Diff.EqualWithTolerance(geodeticDatum.Scale_Diff, DoubleTolerance);
            }
        }

        return hasEqualName || hasEqualParameters;
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
        GridShiftFile = (string)stream.Load("gridShiftFile", null);
    }

    public void Save(IPersistStream stream)
    {
        stream.Save("name", _name);

        if (String.IsNullOrEmpty(GridShiftFile))
        {
            stream.Save("X", _X);
            stream.Save("Y", _Y);
            stream.Save("Z", _Z);
            stream.Save("rX", _rX);
            stream.Save("rY", _rY);
            stream.Save("rZ", _rZ);
            stream.Save("scale", _scale);
        }
        else
        {
            stream.Save("gridShiftFile", GridShiftFile);
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
        datum.GridShiftFile = GridShiftFile;

        return datum;
    }

    #endregion

    #region Static Members

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

    #endregion

    private bool AreDoublesEqual(double a, double b, double tolerance = 1e-8)
    {
        return Math.Abs(a - b) < tolerance;
    }
}

public class DatumTransformation : IDatumTransformation
{
    public IGeodeticDatum FromDatum { get; set; }

    public IGeodeticDatum ToDatum { get; private set; } = GeodeticDatum.WGS84;

    public IGeodeticDatum TransformationDatum { get; set; }

    public bool Use { get; set; } = true;

    #region IPersistable Member

    public void Load(IPersistStream stream)
    {
        Use = (bool)stream.Load("Use", true);
        FromDatum = stream.Load("FromDatum", null, new GeodeticDatum()) as GeodeticDatum;
        TransformationDatum = stream.Load("TransformationDatum", null, new GeodeticDatum()) as GeodeticDatum;
    }

    public void Save(IPersistStream stream)
    {
        stream.Save("Use", Use);

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

    public IDatumTransformation Clone()
    {
        return new DatumTransformation()
        {
            Use = Use,
            FromDatum = FromDatum?.Clone() as GeodeticDatum,
            ToDatum = ToDatum.Clone() as GeodeticDatum,
            TransformationDatum = TransformationDatum?.Clone() as GeodeticDatum
        };
    }

    #endregion
}

public class DatumTransformations : IDatumTransformations
{
    public IDatumTransformation[] Transformations { get; set; }

    public IDatumTransformations Clone()
    {
        return new DatumTransformations()
        {
            Transformations = Transformations?.Select(t => t.Clone()).ToArray()
        };
    }

    public void Load(IPersistStream stream)
    {
        var datumTransformations = new List<IDatumTransformation>();
        IDatumTransformation datumTransformation;

        while ((datumTransformation = (IDatumTransformation)stream.Load("IDatumTransformation", null, new DatumTransformation())) != null)
        {
            datumTransformations.Add(datumTransformation);
        }

        Transformations = datumTransformations.ToArray();
    }

    public void Save(IPersistStream stream)
    {
        foreach (var datumTransformation in Transformations ?? [])
        {
            stream.Save("IDatumTransformation", datumTransformation);
        }
    }
}
