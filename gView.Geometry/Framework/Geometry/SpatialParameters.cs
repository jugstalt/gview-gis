using gView.Framework.Core.Carto;
using gView.Framework.Core.Geometry;

namespace gView.Framework.Geometry
{
    public class SpatialParameters : ISpatialParameters
    {
        private GeoUnits _unit = GeoUnits.Unknown;
        private bool _geographic = false;
        private double _lat_0 = 0.0, _lon_0 = 0.0, _x_0 = 0.0, _y_0 = 0.0;

        internal void SetMembers(string[] Parameters)
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
                if (p.Length < 2)
                {
                    continue;
                }

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
                            if (_unit == GeoUnits.Unknown)
                            {
                                _unit = GeoUnits.DecimalDegrees;
                            }

                            break;
                    }
                }
                if (p[0] == "+lon_0")
                {
                    double.TryParse(p[1].Replace(".", ","), out _lon_0);
                }

                if (p[0] == "+lat_0")
                {
                    double.TryParse(p[1].Replace(".", ","), out _lat_0);
                }

                if (p[0] == "+x_0")
                {
                    double.TryParse(p[1].Replace(".", ","), out _x_0);
                }

                if (p[0] == "+y_0")
                {
                    double.TryParse(p[1].Replace(".", ","), out _y_0);
                }
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

}
