using gView.Framework.Carto;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.system;
using gView.GraphicsEngine.Abstraction;
using System;
using System.ComponentModel;

namespace gView.Framework.Symbology
{
    [gView.Framework.system.RegisterPlugIn("16519A8B-2945-4F74-A38D-98D8E41BF3EA")]
    public class SymbolCappedLineSymbol : Symbol, ILineSymbol, IPersistable
    {
        public SymbolCappedLineSymbol()
        {
            this.RotateStartSymbol = this.RotateEndSymbol = true;
        }

        public override string ToString()
        {
            return this.Name;
        }

        #region Properties

        [Browsable(true)]
        [DisplayName("Start Cap Symbol")]
        [Category("Cap Symbols")]
        [UsePointSymbolPicker()]
        public ISymbol StartCapPointSymbol
        {
            get; set;
        }

        [Browsable(true)]
        [DisplayName("End Cap Symbol")]
        [Category("Cap Symbols")]
        [UsePointSymbolPicker()]
        public ISymbol EndCapPointSymbol
        {
            get; set;
        }

        [Browsable(true)]
        [DisplayName("Rotate Start Symbol")]
        [Category("Cap Symbols")]
        public bool RotateStartSymbol { get; set; }

        [Browsable(true)]
        [DisplayName("Rotate Start Symbol")]
        [Category("Cap Symbols")]
        public bool RotateEndSymbol { get; set; }

        [Browsable(true)]
        [DisplayName("Symbol")]
        [Category("Line Symbol")]
        [UseLineSymbolPicker()]
        public ISymbol LineSymbol
        {
            get; set;
        }

        #endregion

        #region ILineSymbol

        [Browsable(false)]
        public string Name => "Symbol Capped Line";

        [Browsable(false)]
        public SymbolSmoothing SymbolSmothingMode
        {
            set
            {
                if (this.LineSymbol != null)
                {
                    this.LineSymbol.SymbolSmothingMode = value;
                }

                if (this.StartCapPointSymbol != null)
                {
                    this.StartCapPointSymbol.SymbolSmothingMode = value;
                }

                if (this.EndCapPointSymbol != null)
                {
                    this.EndCapPointSymbol.SymbolSmothingMode = value;
                }
            }
        }

        public bool SupportsGeometryType(GeometryType geomType) => geomType == GeometryType.Polyline;

        public void Draw(IDisplay display, IGeometry geometry)
        {
            try
            {
                var polyline = geometry as IPolyline;
                if (polyline == null || polyline.PathCount == 0)
                {
                    return;
                }

                if (this.LineSymbol != null)
                {
                    this.LineSymbol.Draw(display, geometry);
                }
                if (this.StartCapPointSymbol != null || this.EndCapPointSymbol != null)
                {
                    for (int p = 0, to = polyline.PathCount; p < to; p++)
                    {
                        var path = polyline[p];
                        var pointCount = path.PointCount;

                        if (pointCount < 2)
                        {
                            continue;
                        }

                        if (this.StartCapPointSymbol != null)
                        {
                            var symbolRotation = this.RotateStartSymbol ?
                                this.StartCapPointSymbol as ISymbolRotation :
                                null;

                            if (symbolRotation != null)
                            {
                                double dx = path[1].X - path[0].X,
                                       dy = path[1].Y - path[0].Y;

                                symbolRotation.Rotation = (float)(Math.Atan2(-dy, dx) * 180.0 / Math.PI);
                            }

                            this.StartCapPointSymbol.Draw(display, path[0]);
                        }

                        if (this.EndCapPointSymbol != null)
                        {
                            var symbolRotation = this.RotateEndSymbol ?
                                this.EndCapPointSymbol as ISymbolRotation :
                                null;

                            if (symbolRotation != null)
                            {
                                double dx = path[pointCount - 1].X - path[pointCount - 2].X,
                                       dy = path[pointCount - 1].Y - path[pointCount - 2].Y;

                                symbolRotation.Rotation = (float)(Math.Atan2(-dy, dx) * 180.0 / Math.PI);
                            }

                            this.EndCapPointSymbol.Draw(display, path[path.PointCount - 1]);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void DrawPath(IDisplay display, IGraphicsPath path)
        {

        }

        public void Release()
        {
            if (this.StartCapPointSymbol != null)
            {
                this.StartCapPointSymbol.Release();
            }

            if (this.EndCapPointSymbol != null)
            {
                this.EndCapPointSymbol.Release();
            }

            if (this.LineSymbol != null)
            {
                this.LineSymbol.Release();
            }
        }

        public bool RequireClone()
        {
            return (LineSymbol != null && LineSymbol.RequireClone()) ||
                   (StartCapPointSymbol != null && StartCapPointSymbol.RequireClone()) ||
                   (EndCapPointSymbol != null && EndCapPointSymbol.RequireClone());
        }

        #endregion

        #region IClone

        public object Clone(CloneOptions options)
        {
            SymbolCappedLineSymbol cloneSym = new SymbolCappedLineSymbol();
            if (this.LineSymbol != null)
            {
                cloneSym.LineSymbol = this.LineSymbol.Clone(options) as ISymbol;
            }

            if (this.StartCapPointSymbol != null)
            {
                cloneSym.StartCapPointSymbol = this.StartCapPointSymbol.Clone(options) as ISymbol;
            }

            if (this.EndCapPointSymbol != null)
            {
                cloneSym.EndCapPointSymbol = this.EndCapPointSymbol.Clone(options) as ISymbol;
            }

            cloneSym.LegendLabel = _legendLabel;

            cloneSym.RotateStartSymbol = this.RotateStartSymbol;
            cloneSym.RotateEndSymbol = this.RotateEndSymbol;

            return cloneSym;
        }

        #endregion

        #region IPersistable

        new public void Load(IPersistStream stream)
        {
            base.Load(stream);

            this.StartCapPointSymbol = (ISymbol)stream.Load("startcappointsymbol");
            this.EndCapPointSymbol = (ISymbol)stream.Load("endcappointsymbol");
            this.LineSymbol = (ISymbol)stream.Load("linesymbol");

            this.RotateStartSymbol = (bool)stream.Load("rotatestartsymbol", true);
            this.RotateEndSymbol = (bool)stream.Load("rotateendsymbol", true);
        }

        new public void Save(IPersistStream stream)
        {
            base.Save(stream);

            if (this.StartCapPointSymbol != null)
            {
                stream.Save("startcappointsymbol", this.StartCapPointSymbol);
            }

            if (this.EndCapPointSymbol != null)
            {
                stream.Save("endcappointsymbol", this.EndCapPointSymbol);
            }

            if (this.LineSymbol != null)
            {
                stream.Save("linesymbol", this.LineSymbol);
            }

            stream.Save("rotatestartsymbol", this.RotateStartSymbol);
            stream.Save("rotateendsymbol", this.RotateEndSymbol);
        }

        #endregion
    }
}
