using gView.Framework.Carto;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.system;
using System;
using System.ComponentModel;


namespace gView.Framework.Symbology
{
    [gView.Framework.system.RegisterPlugIn("D03DE1E2-B2D1-4C25-A39E-29DE828C43BC")]
    public sealed class NullSymbol : INullSymbol
    {
        private geometryType _geomType = geometryType.Unknown;

        public NullSymbol() { }
        public NullSymbol(geometryType geomType)
        {
            _geomType = geomType;
        }

        public void Draw(IDisplay display, IGeometry geometry)
        {
        }

        public void Release()
        {
        }

        public string Name
        {
            get { return "Null Symbol"; }
        }

        public void Load(IPersistStream stream)
        {
            _geomType = (geometryType)stream.Load("geomtype", (int)geometryType.Unknown);
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("geomtype", (int)_geomType);
        }

        public object Clone()
        {
            return this;
        }

        public object Clone(CloneOptions options)
        {
            return this;
        }

        public string LegendLabel
        {
            get
            {
                return String.Empty;
            }
            set
            {
            }
        }

        public bool ShowInTOC
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        public int IconHeight
        {
            get { return 0; }
        }

        public geometryType GeomtryType
        {
            get { return _geomType; }
            set { _geomType = value; }
        }

        #region ISymbol Member

        [Browsable(false)]
        public SymbolSmoothing SymbolSmothingMode
        {
            set { }
        }

        public bool RequireClone()
        {
            return false;
        }

        #endregion
    }
}
