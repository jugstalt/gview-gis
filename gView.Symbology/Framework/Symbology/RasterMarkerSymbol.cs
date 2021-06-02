using gView.Framework.Carto;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.Symbology.UI;
using gView.Framework.system;
using gView.Framework.UI;
using gView.Symbology.Framework.Symbology.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace gView.Framework.Symbology
{
    [gView.Framework.system.RegisterPlugIn("230881F2-F9E4-4593-BD25-5B614B9CB503")]
    public sealed class RasterMarkerSymbol : LegendItem, IPropertyPage, IPointSymbol, ISymbolRotation
    {
        private float _xOffset = 0, _yOffset = 0, _angle = 0, _rotation = 0, _hOffset = 0, _vOffset = 0;
        private float _sizeX = 10f, _sizeY = 10f;
        private string _filename = String.Empty;
        private Image _image = null;

        [UseFilePicker()]
        public string Filename
        {
            get
            {
                return _filename;
            }
            set
            {
                _filename = value;
            }
        }

        public float SizeX
        {
            get { return _sizeX; }
            set { _sizeX = value; }
        }
        public float SizeY
        {
            get { return _sizeY; }
            set { _sizeY = value; }
        }

        #region IPropertyPage Member

        public object PropertyPageObject()
        {
            return null;
        }

        public object PropertyPage(object initObject)
        {
            string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Assembly uiAssembly = Assembly.LoadFrom(appPath + @"/gView.Win.Symbology.UI.dll");

            IPropertyPanel p = uiAssembly.CreateInstance("gView.Framework.Symbology.UI.PropertyForm_SimplePointSymbol") as IPropertyPanel;
            if (p != null)
            {
                return p.PropertyPanel(this);
            }

            return null;
        }

        #endregion

        #region IPointSymbol Member

        public void DrawPoint(IDisplay display, IPoint point)
        {
            if (!String.IsNullOrEmpty(_filename))
            {
                float sizeX = _sizeX, sizeY = _sizeY;

                if(display.IsLegendItemSymbol())
                {
                    sizeX = Math.Min(_sizeY, display.iWidth);
                    sizeY = Math.Min(_sizeY, display.iHeight);
                }
                float x = _xOffset - sizeX / 2;
                float y = _yOffset - sizeY / 2;

                try
                {
                    display.GraphicsContext.TranslateTransform((float)point.X, (float)point.Y);
                    display.GraphicsContext.RotateTransform(_angle + _rotation);

                    var rect = new Rectangle((int)x, (int)y, (int)sizeX, (int)sizeY);

                    try
                    {
                        if (_image == null)
                        {
                            if (_filename.StartsWith("resource:"))
                            {
                                _image = Image.FromStream(new MemoryStream(display.Map.ResourceContainer[_filename.Substring(9)]));
                            }
                            else
                            {
                                _image = Image.FromFile(_filename);
                            }
                        }

                        if (_image != null)
                        {
                            display.GraphicsContext.DrawImage(
                                    _image,
                                    rect,
                                    new Rectangle(0, 0, _image.Width, _image.Height),
                                    GraphicsUnit.Pixel);
                        }
                    }
                    catch
                    {
                    }
                }
                finally
                {
                    display.GraphicsContext.ResetTransform();
                }
            }
        }

        #endregion

        #region ISymbol Member

        public void Draw(IDisplay display, IGeometry geometry)
        {
            if (display != null && geometry is IPoint)
            {
                double x = ((IPoint)geometry).X;
                double y = ((IPoint)geometry).Y;
                display.World2Image(ref x, ref y);
                IPoint p = new gView.Framework.Geometry.Point(x, y);
                DrawPoint(display, p);
            }
            else if (geometry is IMultiPoint)
            {

                for (int i = 0, to = ((IMultiPoint)geometry).PointCount; i < to; i++)
                {
                    IPoint p = ((IMultiPoint)geometry)[i];
                    Draw(display, p);
                }
            }
        }

        public void Release()
        {
            if (_image != null)
            {
                _image.Dispose();
                _image = null;
            }
        }

        [Browsable(false)]
        public string Name
        {
            get { return "Raster Marker Symbol"; }
        }

        #endregion

        #region IClone2 Member

        public object Clone(CloneOptions options)
        {
            var display = options?.Display;

            if (display == null)
            {
                return Clone();
            }

            float fac =  ReferenceScaleHelper.CalcPixelUnitFactor(options);

            RasterMarkerSymbol marker = new RasterMarkerSymbol();
            marker.Angle = Angle;
            marker.HorizontalOffset = HorizontalOffset * fac;
            marker.VerticalOffset = VerticalOffset * fac;
            marker.SizeX = _sizeX * fac;
            marker.SizeY = _sizeY * fac;
            marker.Filename = _filename;
            marker.LegendLabel = _legendLabel;

            return marker;
        }

        #endregion

        #region ISymbolTransformation Member

        [Browsable(false)]
        public float HorizontalOffset
        {
            get { return _hOffset; }
            set
            {
                _hOffset = value;
                SymbolTransformation.Transform(_angle, _hOffset, _vOffset, out _xOffset, out _yOffset);
            }
        }

        [Browsable(false)]
        public float VerticalOffset
        {
            get { return _vOffset; }
            set
            {
                _vOffset = value;
                SymbolTransformation.Transform(_angle, _hOffset, _vOffset, out _xOffset, out _yOffset);
            }
        }

        [Browsable(false)]
        public float Angle
        {
            get { return _angle; }
            set
            {
                _angle = value;
                SymbolTransformation.Transform(_angle, _hOffset, _vOffset, out _xOffset, out _yOffset);
            }
        }

        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            base.Load(stream);

            Filename = (string)stream.Load("fn", String.Empty);
            HorizontalOffset = (float)stream.Load("x", 0f);
            VerticalOffset = (float)stream.Load("y", 0f);
            Angle = (float)stream.Load("a", 0f);
            SizeX = (float)stream.Load("sx", 10f);
            SizeY = (float)stream.Load("sy", 10f);
        }

        public void Save(IPersistStream stream)
        {
            base.Save(stream);

            stream.Save("fn", Filename);
            stream.Save("x", HorizontalOffset);
            stream.Save("y", VerticalOffset);
            stream.Save("a", Angle);
            stream.Save("sx", _sizeX);
            stream.Save("sy", _sizeY);
        }

        #endregion

        #region ISymbolRotation Member

        [Browsable(false)]
        public float Rotation
        {
            get
            {
                return _rotation;
            }
            set
            {
                _rotation = value;
            }
        }

        #endregion

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
