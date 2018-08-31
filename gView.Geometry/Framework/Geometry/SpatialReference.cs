using System;
using System.Text;
using System.ComponentModel;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.Carto;
using gView.Framework.Geometry.SpatialRefTranslation;
using System.Collections.Generic;
using System.IO;
using gView.Framework.Proj;

namespace gView.Framework.Geometry
{
	/// <summary>
	/// 
	/// </summary>

	public class GeodeticDatum : IGeodeticDatum
	{
		private double _X,_Y,_Z;
		private double _rX,_rY,_rZ;
		private double _scale;
		private string _name;

        private static IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

		public GeodeticDatum() 
		{
			_X=_Y=_Z=_rX=_rY=_rZ=_scale=0.0;
			_name="Unknown";
		}
		public GeodeticDatum(string name) 
		{
			ProjDB db=new ProjDB(ProjDBTables.datums);
			Parameter=db.GetDatumParameters(name);
			db.Dispose();
			Name=name;
		}
		public GeodeticDatum(GeodeticDatum datum) 
		{
			_X=datum._X;
			_Y=datum._Y;
			_Z=datum._Z;
			_rX=datum._rX;
			_rY=datum._rY;
			_rZ=datum._rZ;
			_scale=datum._scale;
			_name=datum._name;
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
			set { _name=value; }
		}

		public string Parameter 
		{
			get 
			{
				return "+towgs84="+
					_X.ToString().Replace(",",".")+","+
					_Y.ToString().Replace(",",".")+","+
					_Z.ToString().Replace(",",".")+","+
					_rX.ToString().Replace(",",".")+","+
					_rY.ToString().Replace(",",".")+","+
					_rZ.ToString().Replace(",",".")+","+
					_scale.ToString().Replace(",",".");
			}
			set 
			{
				string [] p=value.Replace("+towgs84=","").Split(',');
				if(p.Length<7) return;

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
				_X=value;
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
				_Y=value;
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
				_Z=value;
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
				_rX=value;
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
				_rY=value;
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
				_rZ=value;
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
				_scale=value;
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

    public class SpatialParameters : ISpatialParameters
    {
        private GeoUnits _unit = GeoUnits.Unknown;
        private bool _geographic = false;
        private double _lat_0 = 0.0, _lon_0 = 0.0, _x_0 = 0.0, _y_0 = 0.0;

        internal void SetMembers(string [] Parameters)
        {
            _unit = GeoUnits.Unknown;
            _geographic = false;
            _lat_0 = 0.0;
            _lon_0 = 0.0;
            _x_0 = 0.0;
            _y_0 = 0.0;

            foreach (string parameter in Parameters)
            {
                string[] p = parameter.ToLower().Replace(" ", "").Split('=');
                if (p.Length < 2) continue;

                if (p[0] == "+units")
                {
                    switch (p[1])
                    {
                        case "m":
                            _unit = GeoUnits.Meters;
                            break;
                    }
                }
                if (p[0] == "+proj")
                {
                    switch (p[1])
                    {
                        case "longlat":
                        case "latlong":
                            _geographic = true;
                            if (_unit == GeoUnits.Unknown) _unit = GeoUnits.DecimalDegrees;
                            break;
                    }
                }
                if (p[0] == "+lon_0") double.TryParse(p[1].Replace(".", ","), out _lon_0);
                if (p[0] == "+lat_0") double.TryParse(p[1].Replace(".", ","), out _lat_0);
                if (p[0] == "+x_0") double.TryParse(p[1].Replace(".", ","), out _x_0);
                if (p[0] == "+y_0") double.TryParse(p[1].Replace(".", ","), out _y_0);
            }
        }

        #region ISpatialParameters Member

        public GeoUnits Unit
        {
            get { return _unit; }
        }

        public bool IsGeographic
        {
            get { return _geographic; }
        }

        public double lat_0
        {
            get { return _lat_0; }
        }

        public double lon_0
        {
            get { return _lon_0; }
        }

        public double x_0
        {
            get { return _x_0; }
        }

        public double y_0
        {
            get { return _y_0; }
        }

        #endregion

        #region IClone Member

        public object Clone()
        {
            SpatialParameters p = new SpatialParameters();

            p._unit = _unit;
            p._geographic = _geographic;
            p._lat_0 = _lat_0;
            p._lon_0 = _lon_0;
            p._x_0 = _x_0;
            p._y_0 = _y_0;

            return p;
        }

        #endregion
    }

    public class SpatialReference : ISpatialReference
    {
        private string _ID, _params, _description;
        private IGeodeticDatum _datum = null;
        private SpatialParameters _sParams = new SpatialParameters();
        private AxisDirection _axisX = AxisDirection.East, _axisY = AxisDirection.North;

        public SpatialReference()
        {
            _ID = "custom";
            _params = "";

            _sParams.SetMembers(this.Parameters);
        }
        public SpatialReference(string name)
        {
            _ID = name;
            ProjDB db = new ProjDB(ProjDBTables.projs);
            this.Parameters = db.GetParameters(_ID).Split(' ');
            _description = db.GetDescription(_ID);
            if (this.Datum != null)
                this.Datum.Name = db.GetDatumName(_ID);
            db.Dispose();

            _sParams.SetMembers(this.Parameters);
        }
        public SpatialReference(string name, IGeodeticDatum datum)
        {
            _ID = name;
            ProjDB db = new ProjDB();
            this.Parameters = db.GetParameters(_ID).Split(' ');
            _description = db.GetDescription(_ID);
            db.Dispose();
            _datum = datum;

            _sParams.SetMembers(this.Parameters);
        }
        public SpatialReference(string name, string description, string param, IGeodeticDatum datum)
        {
            _ID = name;
            _description = description;
            this.Parameters = param.Split(' ');
            if (datum != null)
                _datum = datum;

            _sParams.SetMembers(this.Parameters);
        }

        public SpatialReference(SpatialReference sRef)
        {
            if (sRef == null) return;
            _ID = sRef._ID;
            _params = sRef._params;
            _description = sRef._description;

            if (sRef.Datum is GeodeticDatum)
            {
                Datum = new GeodeticDatum((GeodeticDatum)sRef.Datum);
            }

            _sParams.SetMembers(this.Parameters);
        }

        #region ISpatialReference Member

        public string Name
        {
            get
            {
                return _ID;
            }
            set
            {
                _ID = value;
            }
        }

        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
            }
        }
        public string[] Parameters
        {
            get
            {
                return _params.Split(' ');
            }
            set
            {
                _params = "";
                foreach (string p in value)
                {
                    if (p.Trim().IndexOf("+towgs84=") == 0)
                    {
                        if (_datum == null) _datum = new GeodeticDatum();

                        _datum.Name = "User defined";
                        _datum.Parameter = p;
                    }
                    else if (p.Trim() == "+no_defs")
                    {
                        continue;
                    }
                    else
                    {
                        if (_params.Length > 0) _params += " ";
                        _params += p;
                    }
                }

                _sParams.SetMembers(this.Parameters);
            }
        }

        public ISpatialParameters SpatialParameters
        {
            get { return _sParams; }
        }

        public IGeodeticDatum Datum
        {
            get
            {
                return _datum;
            }
            set
            {
                _datum = value;
            }
        }

        public AxisDirection Gml3AxisX
        {
            get { return _axisX; }
            set { _axisX = value; }
        }

        public AxisDirection Gml3AxisY
        {
            get { return _axisY; }
            set { _axisY = value; }
        }

        public bool Equals(ISpatialReference sRef)
        {
            if (sRef == null) return true; // keine Projektion möglich/notwendig!!!

            string[] parms1 = this.Parameters;
            string[] parms2 = sRef.Parameters;
            if (parms1.Length != parms2.Length)
            {
                return false;
            }

            for (int i = 0; i < parms1.Length; i++)
            {
                bool found = false;
                for (int j = 0; j < parms2.Length; j++)
                {
                    if (parms1[i] == parms2[j])
                    {
                        found = true;
                        break;
                    }
                }
                if (!found) return false;
            }

            if (this.Datum != null && sRef.Datum != null)
            {
                if (this.Datum.Parameter != sRef.Datum.Parameter) return false;
            }
            return true;
        }

        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            _ID = (string)stream.Load("id", "custom");
            _description = (string)stream.Load("description", "");
            _params = (string)stream.Load("params", "");

            _datum = stream.Load("GeodeticDatum", null, new GeodeticDatum()) as GeodeticDatum;

            _sParams.SetMembers(this.Parameters);
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("id", _ID);
            stream.Save("description", _description);
            stream.Save("params", _params);

            if (_datum != null) stream.Save("GeodeticDatum", _datum);
        }

        #endregion

        public override string ToString()
        {
            return "SpatialReference: " + _params + ((_datum != null) ? _datum.Parameter : "");
        }

        public string ToString(string param)
        {
            switch (param.ToUpper())
            {
                case "P":
                    return _params;
                case "D":
                    return ((_datum != null) ? _datum.Parameter : string.Empty);
                case "SD":
                    return _params + ((_datum != null) ? " "+_datum.Parameter : "");
            }
            return String.Empty;
        }

        public static ISpatialReference FromID(string id)
        {
            if (String.IsNullOrEmpty(id)) return null;

            return SpatialReferenceCache.FromID(id);
        }
        internal static ISpatialReference FromID_(string id)
        {
            ProjDB db = new ProjDB();
            string parameters = db.GetParameters(id);
            string descr = db.GetDescription(id);

            SpatialReference ret = new SpatialReference(id, descr, parameters, null);

            string pgWKT = db.GetPgWkt(id);
            if (!String.IsNullOrEmpty(pgWKT))
            {
                string axisX = db.GetQoutedWKTParameter(pgWKT, "AXIS[\"X\"", ",", "]");
                string axisY = db.GetQoutedWKTParameter(pgWKT, "AXIS[\"Y\"", ",", "]");

                switch (axisX)
                {
                    case "NORTH":
                        ret.Gml3AxisX = AxisDirection.North;
                        break;
                    case "EAST":
                        ret.Gml3AxisX = AxisDirection.East;
                        break;
                    case "SOUTH":
                        ret.Gml3AxisX = AxisDirection.South;
                        break;
                    case "WEST":
                        ret.Gml3AxisX = AxisDirection.West;
                        break;
                }
                switch (axisY)
                {
                    case "NORTH":
                        ret.Gml3AxisY = AxisDirection.North;
                        break;
                    case "EAST":
                        ret.Gml3AxisY = AxisDirection.East;
                        break;
                    case "SOUTH":
                        ret.Gml3AxisY = AxisDirection.South;
                        break;
                    case "WEST":
                        ret.Gml3AxisY = AxisDirection.West;
                        break;
                }
            }
            return ret;
        }

        public static void FromProj4(ISpatialReference sReference, string parameters)
        {
            if (parameters == String.Empty || parameters == null) return;

            if (sReference is SpatialReference)
            {
                ((SpatialReference)sReference).Parameters = parameters.Split(' ');
            }
        }
        public static string ToProj4(ISpatialReference sReference)
        {
            if (sReference == null) return "";

            StringBuilder sb = new StringBuilder();
            foreach (string param in sReference.Parameters)
            {
                if (sb.Length > 0) sb.Append(" ");
                sb.Append(param);
            }
            if (sReference.Datum != null)
            {
                sb.Append(" " + sReference.Datum.Parameter);
            }
            return sb.ToString();
        }

        public static ISpatialReference FromWKT(string wkt)
        {
            try
            {
                object obj = WktCoordinateSystemReader.Create(wkt);
                string p4 = Proj4CoordinateSystemWriter.Write(obj);

                string name = "Unknown", datumName = "Unknown";

                if (obj is AbstractInformation)
                    name = ((AbstractInformation)obj).Name;
                if (obj is GeographicCoordinateSystem)
                {
                    datumName = ((GeographicCoordinateSystem)obj).HorizontalDatum.Name;
                }
                else if (obj is ProjectedCoordinateSystem)
                {
                    datumName = ((ProjectedCoordinateSystem)obj).HorizontalDatum.Name;
                }

                SpatialReference sRef = new SpatialReference(
                    name, name, p4, null);

                if (sRef.Datum != null)
                    sRef.Datum.Name = datumName;

                return sRef;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static string ToWKT(ISpatialReference sRef)
        {
            if (sRef == null) return "";

            object obj = Proj4CoordinateSystemReader.Create(ToProj4(sRef));
            if (obj == null) return "";

            if (obj is AbstractInformation)
                ((AbstractInformation)obj).Name = (sRef.Description != String.Empty) ? sRef.Description : sRef.Name;

            if (obj is ProjectedCoordinateSystem)
            {
                ProjectedCoordinateSystem sys = obj as ProjectedCoordinateSystem;
                if (sys.HorizontalDatum != null && sRef.Datum != null)
                    sys.HorizontalDatum.Name=sRef.Datum.Name;
                if (sys.GeographicCoordinateSystem != null && sRef.Datum != null)
                    sys.GeographicCoordinateSystem.Name = GeogrNameFromDatumName(sRef.Datum.Name);
            }
            return WktCoordinateSystemWriter.Write(obj);
        }
        private static string GeogrNameFromDatumName(string descr)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < descr.Length; i++)
            {
                char c = descr[i];
                if (c >= 'A' && c <= 'Z' || c >= '0' && c <= '9')
                    sb.Append(c.ToString());
                else if (c == '(')
                {
                    while (c != ')' && i < descr.Length - 1)
                    {
                        sb.Append(c.ToString());
                        c = descr[++i];
                    }
                    sb.Append(")");
                }
            }
            return sb.ToString();
        }

        public static string ToESRIWKT(ISpatialReference sRef)
        {
            if (sRef is SpatialReference)
            {
                return SpatialReferenceCache.ToESRIWKT((SpatialReference)sRef);
            }
            else
            {
                return ToESRIWKT_(sRef);
            }
        }
        internal static string ToESRIWKT_(ISpatialReference sRef)
        {
            if (sRef == null) return "";

            object obj = Proj4CoordinateSystemReader.Create(ToProj4(sRef));
            if (obj == null) return "";

            if (obj is AbstractInformation)
                ((AbstractInformation)obj).Name = (sRef.Description != String.Empty) ? sRef.Description : sRef.Name;

            if (obj is ProjectedCoordinateSystem)
            {
                ProjectedCoordinateSystem sys = obj as ProjectedCoordinateSystem;
                if (sys.HorizontalDatum != null && sRef.Datum != null)
                    sys.HorizontalDatum.Name = sRef.Datum.Name;
                if (sys.GeographicCoordinateSystem != null && sRef.Datum != null)
                    sys.GeographicCoordinateSystem.Name = GeogrNameFromDatumName(sRef.Datum.Name);
            }

            return ESRIWktCoordinateSystemWriter.Write(obj);
        }
        public static string ToESRIGeotransWKT(ISpatialReference sRef)
        {
            if (sRef == null || sRef.Datum == null) return "";

            object obj = Proj4CoordinateSystemReader.Create(ToProj4(sRef));
            if (obj == null) return "";

            if (obj is AbstractInformation)
                ((AbstractInformation)obj).Name = (sRef.Description != String.Empty) ? sRef.Description : sRef.Name;

            if (obj is ProjectedCoordinateSystem)
            {
                ProjectedCoordinateSystem sys = obj as ProjectedCoordinateSystem;
                if (sys.HorizontalDatum != null && sRef.Datum != null)
                    sys.HorizontalDatum.Name = sRef.Datum.Name;
                if (sys.GeographicCoordinateSystem != null && sRef.Datum != null)
                    sys.GeographicCoordinateSystem.Name = GeogrNameFromDatumName(sRef.Datum.Name);
            }

            return ESRIGeotransWktCoordinateWriter.Write(obj);
        }

        #region IClone Member

        public object Clone()
        {
            SpatialReference sRef = new SpatialReference();
            sRef._ID = _ID;
            sRef._params = _params;
            sRef._description = _description;
            sRef._datum = (_datum != null) ? _datum.Clone() as IGeodeticDatum : null;
            sRef._sParams = (_sParams != null) ? _sParams.Clone() as SpatialParameters : null;

            return sRef;
        }

        #endregion

        public string ToXmlString()
        {
            XmlStream xmlStream = new XmlStream("SpatialReference");
            this.Save(xmlStream);
            return xmlStream.ToString();
        }

        public void FromXmlString(string xml)
        {
            XmlStream stream = new XmlStream("SpatialReference");
            StringReader sr = new StringReader(xml);
            stream.ReadStream(sr);

            this.Load(stream);
        }

        #region IBase64String Member

        public string ToBase64String()
        {
            string xmlString = this.ToXmlString();

            byte[] bytes = System.Text.ASCIIEncoding.ASCII.GetBytes(xmlString);
            string returnValue = System.Convert.ToBase64String(bytes);

            return returnValue;
        }

        public void FromBase64String(string base64)
        {
            if (String.IsNullOrEmpty(base64))
                return;
            byte[] bytes = System.Convert.FromBase64String(base64);
            string xmlString = System.Text.ASCIIEncoding.ASCII.GetString(bytes);

            FromXmlString(xmlString);
        }

        #endregion
    }

    internal class SpatialReferenceCache
    {
        private static List<ISpatialReference> _sRefs = new List<ISpatialReference>();
        private static Dictionary<string, string> _esriWTKs = new Dictionary<string, string>();
        private static object lockThis=new object();
       
        public SpatialReferenceCache()
        {
        }

        public static void Clear() 
        {
            _sRefs.Clear();
        }
        public static ISpatialReference FromID(string id)
        {
            lock (lockThis)
            {
                foreach (ISpatialReference sRef in _sRefs)
                {
                    if (sRef.Name == id) return sRef;
                }

                ISpatialReference sr = SpatialReference.FromID_(id);
                if (sr != null) _sRefs.Add(sr);

                return sr;
            }
        }
        public static string ToESRIWKT(SpatialReference sRef)
        {
            lock (lockThis)
            {
                string sParam = sRef.ToString("P");

                foreach (string key in _esriWTKs.Keys)
                {
                    if (key.Equals(sParam))
                    {
                        return _esriWTKs[key];
                    }
                }

                string wkt = SpatialReference.ToESRIWKT_(sRef);
                _esriWTKs.Add(sParam, wkt);
                return wkt;
            }
        }
    }
	public class IMSSpatialReference : ISpatialReference
	{
		protected string _id="",_string="",
			_datumId="",_datumString="";
        private ISpatialParameters _sParams = new SpatialParameters();


		public IMSSpatialReference() 
		{
		}
		#region IPersist Member
		
		[Browsable(false)]
		public string PersistString
		{
			get
			{
				_id=_id.Trim();
				_string=_string.Trim();
				_datumId=_datumId.Trim();
				_datumString=_datumString.Trim();

				return 
					_id+";"+
					_string+";"+
					_datumId+";"+
					_datumString;
			}
			set
			{
				string [] param=value.Split(';');

				if(param.Length==4) 
				{
					_id=param[0].Trim();
					_string=param[1].Trim();
					_datumId=param[2].Trim();
					_datumString=param[3].Trim();
				}
			}
		}

		#endregion

		[Category("Projection")]
		public string ID 
		{
			get 
			{
				return _id;
			}
			set 
			{
				try 
				{
					if(value=="") _id="";
					_id=Convert.ToInt32(value).ToString();
				} 
				catch 
				{
					_id="";
				}
			}
		}
		public string Description 
		{
			get { return ""; }
		}
		[Category("Projection")]
		public string String 
		{
			get { return _string; }
			set { _string=value; }
		}

		[Category("Datum")]
		public string datumID 
		{
			get { return _datumId; }
			set 
			{
				try 
				{
					if(value=="") _datumId="";
					_datumId=Convert.ToInt32(value).ToString();
				} 
				catch 
				{
					_datumId="";
				}
			}
		}
		[Category("Datum")]
		public string datumString 
		{
			get { return _datumString; }
			set { _datumString=value; }
		}
		#region ISpatialReference Member

		public gView.Framework.Geometry.IGeodeticDatum Datum
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public string Name
		{
			get
			{
				return null;
			}
		}

		public string[] Parameters
		{
			get
			{
				return null;
			}
		}

        public ISpatialParameters SpatialParameters
        {
            get { return _sParams; }
        }

        public AxisDirection Gml3AxisX
        {
            get { return AxisDirection.East; }
        }

        public AxisDirection Gml3AxisY
        {
            get { return AxisDirection.North; }
        }

		public bool Equals(ISpatialReference sRef) 
		{
			return false;
		}
		#endregion

		#region IPersistable Member

		public string PersistID
		{
			get
			{
				return null;
			}
		}

		public void Load(IPersistStream stream)
		{
		}

		public void Save(IPersistStream stream)
		{
		}

		#endregion

        #region IClone Member

        public object Clone()
        {
            IMSSpatialReference sRef = new IMSSpatialReference();

            sRef._id = _id;
            sRef._string = _string;
            sRef._datumId = _datumId;
            sRef._datumString = _datumString;
            sRef._sParams = (_sParams != null) ? _sParams.Clone() as ISpatialParameters : null;

            return sRef;
        }

        #endregion

        #region IXmlString Member

        public string ToXmlString()
        {
            return String.Empty;
        }

        public void FromXmlString(string xml)
        {
            
        }

        #endregion

        #region IBase64String Member

        public string ToBase64String()
        {
            return String.Empty;
        }

        public void FromBase64String(string base64)
        {

        }

        #endregion
    }

}
