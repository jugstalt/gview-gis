using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using System;
using System.ComponentModel;

namespace gView.Framework.Geometry
{
    public class IMSSpatialReference : ISpatialReference
    {
        protected string _id = "", _string = "",
            _datumId = "", _datumString = "";
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
                _id = _id.Trim();
                _string = _string.Trim();
                _datumId = _datumId.Trim();
                _datumString = _datumString.Trim();

                return
                    _id + ";" +
                    _string + ";" +
                    _datumId + ";" +
                    _datumString;
            }
            set
            {
                string[] param = value.Split(';');

                if (param.Length == 4)
                {
                    _id = param[0].Trim();
                    _string = param[1].Trim();
                    _datumId = param[2].Trim();
                    _datumString = param[3].Trim();
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
                    if (value == "")
                    {
                        _id = "";
                    }

                    _id = Convert.ToInt32(value).ToString();
                }
                catch
                {
                    _id = "";
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
            set { _string = value; }
        }

        [Category("Datum")]
        public string datumID
        {
            get { return _datumId; }
            set
            {
                try
                {
                    if (value == "")
                    {
                        _datumId = "";
                    }

                    _datumId = Convert.ToInt32(value).ToString();
                }
                catch
                {
                    _datumId = "";
                }
            }
        }
        [Category("Datum")]
        public string datumString
        {
            get { return _datumString; }
            set { _datumString = value; }
        }
        #region ISpatialReference Member

        public IGeodeticDatum Datum
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

        public int EpsgCode { get { return 0; } }

        public bool Equals(ISpatialReference sRef)
        {
            return false;
        }

        public double MakeValidTolerance => this.SpatialParameters?.IsGeographic == false ? 5e-4 : 5e-8;

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
