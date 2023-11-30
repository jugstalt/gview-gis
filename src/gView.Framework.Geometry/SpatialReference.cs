using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Geometry.Proj;
using gView.Framework.Geometry.SpatialRefTranslation;
using gView.Framework.IO;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace gView.Framework.Geometry
{

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
            Parameters = db.GetParameters(_ID).Split(' ');
            _description = db.GetDescription(_ID);
            if (this.Datum != null)
            {
                this.Datum.Name = db.GetDatumName(_ID);
            }

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
            {
                _datum = datum;
            }

            _sParams.SetMembers(this.Parameters);
        }

        public SpatialReference(SpatialReference sRef)
        {
            if (sRef == null)
            {
                return;
            }

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
                        if (_datum == null)
                        {
                            _datum = new GeodeticDatum();
                        }

                        _datum.Name = "User defined";
                        _datum.Parameter = p;
                    }
                    else if (p.Trim() == "+no_defs")
                    {
                        continue;
                    }
                    else
                    {
                        if (_params.Length > 0)
                        {
                            _params += " ";
                        }

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

        public int EpsgCode
        {
            get
            {
                try
                {
                    if (!String.IsNullOrWhiteSpace(_ID) && _ID.ToLower().StartsWith("epsg:"))
                    {
                        return int.Parse(_ID.Substring(5));
                    }
                }
                catch { }

                return 0;
            }
        }

        public bool Equals(ISpatialReference sRef)
        {
            if (sRef == null)
            {
                return true; // keine Projektion möglich/notwendig!!!
            }

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
                if (!found)
                {
                    return false;
                }
            }

            if (this.Datum != null && sRef.Datum != null)
            {
                if (this.Datum.Parameter != sRef.Datum.Parameter)
                {
                    return false;
                }
            }
            return true;
        }

        public double MakeValidTolerance => this.SpatialParameters?.IsGeographic == false ? 5e-4 : 5e-8;

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

            if (_datum != null)
            {
                stream.Save("GeodeticDatum", _datum);
            }
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
                    return _params + ((_datum != null) ? " " + _datum.Parameter : "");
            }
            return String.Empty;
        }

        public static ISpatialReference FromID(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return null;
            }

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

                if (String.IsNullOrWhiteSpace(axisX) && String.IsNullOrWhiteSpace(axisY))
                {
                    if (ret.Parameters.Contains("+proj=utm"))
                    {
                        ret.Gml3AxisX = AxisDirection.East;
                        ret.Gml3AxisY = AxisDirection.North;
                    }
                    else if (ret.SpatialParameters.IsGeographic)  // 4326 => X-Axis to north => tested with QGIS!!
                    {
                        ret.Gml3AxisX = AxisDirection.North;
                        ret.Gml3AxisY = AxisDirection.East;
                    }
                }
                else
                {
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
            }
            return ret;
        }

        public static void FromProj4(ISpatialReference sReference, string parameters)
        {
            if (parameters == String.Empty || parameters == null)
            {
                return;
            }

            if (sReference is SpatialReference)
            {
                ((SpatialReference)sReference).Parameters = parameters.Split(' ');
            }
        }
        public static string ToProj4(ISpatialReference sReference)
        {
            if (sReference == null)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
            foreach (string param in sReference.Parameters)
            {
                if (sb.Length > 0)
                {
                    sb.Append(" ");
                }

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
                {
                    name = ((AbstractInformation)obj).Name;
                }

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
                {
                    sRef.Datum.Name = datumName;
                }

                return sRef;
            }
            catch (Exception /*ex*/)
            {
                return null;
            }
        }
        public static string ToWKT(ISpatialReference sRef)
        {
            if (sRef == null)
            {
                return "";
            }

            object obj = Proj4CoordinateSystemReader.Create(ToProj4(sRef));
            if (obj == null)
            {
                return "";
            }

            if (obj is AbstractInformation)
            {
                ((AbstractInformation)obj).Name = (sRef.Description != String.Empty) ? sRef.Description : sRef.Name;
            }

            if (obj is ProjectedCoordinateSystem)
            {
                ProjectedCoordinateSystem sys = obj as ProjectedCoordinateSystem;
                if (sys.HorizontalDatum != null && sRef.Datum != null)
                {
                    sys.HorizontalDatum.Name = sRef.Datum.Name;
                }

                if (sys.GeographicCoordinateSystem != null && sRef.Datum != null)
                {
                    sys.GeographicCoordinateSystem.Name = GeogrNameFromDatumName(sRef.Datum.Name);
                }
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
                {
                    sb.Append(c.ToString());
                }
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
            if (sRef == null)
            {
                return "";
            }

            object obj = Proj4CoordinateSystemReader.Create(ToProj4(sRef));
            if (obj == null)
            {
                return "";
            }

            if (obj is AbstractInformation)
            {
                ((AbstractInformation)obj).Name = (sRef.Description != String.Empty) ? sRef.Description : sRef.Name;
            }

            if (obj is ProjectedCoordinateSystem)
            {
                ProjectedCoordinateSystem sys = obj as ProjectedCoordinateSystem;
                if (sys.HorizontalDatum != null && sRef.Datum != null)
                {
                    sys.HorizontalDatum.Name = sRef.Datum.Name;
                }

                if (sys.GeographicCoordinateSystem != null && sRef.Datum != null)
                {
                    sys.GeographicCoordinateSystem.Name = GeogrNameFromDatumName(sRef.Datum.Name);
                }
            }

            return ESRIWktCoordinateSystemWriter.Write(obj);
        }
        public static string ToESRIGeotransWKT(ISpatialReference sRef)
        {
            if (sRef == null || sRef.Datum == null)
            {
                return "";
            }

            object obj = Proj4CoordinateSystemReader.Create(ToProj4(sRef));
            if (obj == null)
            {
                return "";
            }

            if (obj is AbstractInformation)
            {
                ((AbstractInformation)obj).Name = (sRef.Description != String.Empty) ? sRef.Description : sRef.Name;
            }

            if (obj is ProjectedCoordinateSystem)
            {
                ProjectedCoordinateSystem sys = obj as ProjectedCoordinateSystem;
                if (sys.HorizontalDatum != null && sRef.Datum != null)
                {
                    sys.HorizontalDatum.Name = sRef.Datum.Name;
                }

                if (sys.GeographicCoordinateSystem != null && sRef.Datum != null)
                {
                    sys.GeographicCoordinateSystem.Name = GeogrNameFromDatumName(sRef.Datum.Name);
                }
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
            {
                return;
            }

            byte[] bytes = System.Convert.FromBase64String(base64);
            string xmlString = System.Text.ASCIIEncoding.ASCII.GetString(bytes);

            FromXmlString(xmlString);
        }

        #endregion
    }

}
