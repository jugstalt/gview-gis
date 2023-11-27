using gView.Framework.Core.Carto;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Core.Symbology;
using gView.Framework.Core.system;
using System;
using System.ComponentModel;

namespace gView.Framework.Symbology
{
    [RegisterPlugIn("D03DE1E2-B2D1-4C25-A39E-29DE828C43BC")]
    public sealed class NullSymbol : INullSymbol
    {
        private GeometryType _geomType = GeometryType.Unknown;

        public NullSymbol() { }
        public NullSymbol(GeometryType geomType)
        {
            _geomType = geomType;
        }

        public override string ToString()
        {
            return this.Name;
        }

        public bool SupportsGeometryType(GeometryType geomType) => true;

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
            _geomType = (GeometryType)stream.Load("geomtype", (int)GeometryType.Unknown);
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

        public GeometryType GeomtryType
        {
            get { return _geomType; }
            set { _geomType = value; }
        }

        #region ISymbol Member

        [Browsable(false)]
        public SymbolSmoothing SymbolSmoothingMode
        {
            get => SymbolSmoothing.None;
            set { }
        }

        public bool RequireClone()
        {
            return false;
        }

        #endregion
    }
}
