using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Core.Symbology;
using gView.GraphicsEngine;
using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.Extensions;
using System;
using System.ComponentModel;

namespace gView.Framework.Symbology
{
    [RegisterPlugInAttribute("A06F8B12-394E-4F8E-8B9A-6025F96F6F4F")]
    public class BlockoutTextSymbol : SimpleTextSymbol
    {
        protected IBrush _outlinebrush;
        protected IPen _borderPen;
        protected float _padding = 0f;

        public BlockoutTextSymbol()
            : base()
        {
            _outlinebrush = Current.Engine.CreateSolidBrush(ArgbColor.Yellow);
            _borderPen = Current.Engine.CreatePen(ArgbColor.Transparent, 0f);

            this.SymbolSmoothingMode = SymbolSmoothing.AntiAlias;
        }

        protected BlockoutTextSymbol(IFont font, ArgbColor color, ArgbColor outlinecolor,
                                      float padding = 0f, ArgbColor? borderColor = null, float borderWidth = 0f)
            : base(font, color)
        {
            _outlinebrush = Current.Engine.CreateSolidBrush(outlinecolor);
            _borderPen = Current.Engine.CreatePen(borderColor ?? ArgbColor.Transparent, borderWidth);
            _padding = padding;

            this.SymbolSmoothingMode = SymbolSmoothing.AntiAlias;
        }

        public override string ToString()
        {
            return this.Name;
        }

        [Browsable(true)]
        [UseColorPicker()]
        public ArgbColor ColorOutline
        {
            get { return _outlinebrush.Color; }
            set { _outlinebrush.Color = value; }
        }

        /// <summary>
        /// Extra space (pixels) between the text and the background box's edge - mirrors
        /// ArcGIS Pro's balloon-callout margin, which pads the box out from the pixel-tight
        /// rectangle this symbol used to draw. Grows the box symmetrically on every side (see
        /// <see cref="DrawAtPoint"/>) and is included in <see cref="Margin"/> so the collision
        /// box always matches what's actually drawn.
        /// </summary>
        [Browsable(true)]
        public float Padding
        {
            get { return _padding; }
            set { _padding = value; }
        }

        /// <summary>
        /// Border drawn around the background box, e.g. ArcGIS Pro's balloon-callout background
        /// symbol outline. 0 (the default) draws no border - same "0 = off" convention as
        /// <see cref="GlowingTextSymbol.GlowingWidth"/>.
        /// </summary>
        [Browsable(true)]
        [UseColorPicker()]
        public ArgbColor BorderColor
        {
            get { return _borderPen.Color; }
            set { _borderPen.Color = value; }
        }

        [Browsable(true)]
        public float BorderWidth
        {
            get { return _borderPen.Width; }
            set { _borderPen.Width = value; }
        }

        /// <summary>
        /// Whether the border is actually drawn - engines can enforce their own minimum pen
        /// width (e.g. Skia clamps <c>CreatePen(color, 0f)</c> up to a hairline ~0.8px instead of
        /// a true 0), so a default/unset border must still be judged transparent, not "width
        /// &gt; 0", or every BlockoutTextSymbol would silently grow its collision box by that
        /// hairline even with no border configured. Shared by <see cref="Margin"/> (must only
        /// count what's really drawn) and <see cref="DrawAtPoint"/> (must only draw what Margin
        /// accounted for).
        /// </summary>
        private bool BorderIsVisible =>
            _borderPen != null && _borderPen.Width > 0f && !_borderPen.Color.IsTransparent;

        /// <summary>
        /// DrawAtPoint's box is <c>MeasureText(...).AddPadding(_font)</c> - larger than the bare
        /// measured text on engines that measure pixel-exact (Skia). Reuses that exact padding
        /// (rather than re-deriving the DPI/point-size math here) and takes the larger of its two
        /// axes (width padding &gt; height's) as a single, slightly-generous per-side margin - 0
        /// on engines where AddPadding is already a no-op (e.g. GDI+, which measures with padding
        /// built in). <see cref="Padding"/> and half the border's width (a stroke straddles its
        /// path, so only half of it extends beyond the fill rect) are added on top.
        /// </summary>
        protected override float Margin
        {
            get
            {
                float margin = _padding + (BorderIsVisible ? _borderPen.Width / 2f : 0f);

                if (_font == null)
                {
                    return margin;
                }

                var padding = new CanvasSizeF(0f, 0f).AddPadding(_font);
                return margin + Math.Max(padding.Width, padding.Height) / 2f;
            }
        }

        #region ISymbol Members

        override public void Release()
        {
            base.Release();
            if (_outlinebrush != null)
            {
                _outlinebrush.Dispose();
            }

            _outlinebrush = null;

            if (_borderPen != null)
            {
                _borderPen.Dispose();
            }

            _borderPen = null;
        }

        [Browsable(false)]
        override public string Name
        {
            get { return "Blockout Text Symbol"; }
        }

        #endregion ISymbol Members

        #region IClone2 Members

        public override object Clone()
        {
            BlockoutTextSymbol tSym = _font != null && _brush != null && _outlinebrush != null ?
                new BlockoutTextSymbol(Current.Engine.CreateFont(_font.Name, _font.Size, _font.Style), _brush.Color, _outlinebrush.Color, _padding, _borderPen?.Color, _borderPen?.Width ?? 0f) :
                new BlockoutTextSymbol();

            tSym.HorizontalOffset = HorizontalOffset;
            tSym.VerticalOffset = VerticalOffset;
            tSym.Angle = Angle;
            tSym._align = _align;
            tSym.Smoothingmode = this.Smoothingmode;
            tSym.MinFontSize = this.MinFontSize;
            tSym.MaxFontSize = this.MaxFontSize;
            tSym.SymbolSpacingType = this.SymbolSpacingType;
            tSym.SymbolSpacingX = this.SymbolSpacingX;
            tSym.SymbolSpacingY = this.SymbolSpacingY;
            tSym.IncludesSuperScript = this.IncludesSuperScript;
            tSym.SecondaryTextSymbolAlignments = this.SecondaryTextSymbolAlignments;

            return tSym;
        }

        override public object Clone(CloneOptions options)
        {
            var display = options?.Display;

            if (display == null)
            {
                return this.Clone();
            }

            float fac = 1;
            if (options.ApplyRefScale)
            {
                fac = ReferenceScaleHelper.RefscaleFactor(
                    (float)(display.ReferenceScale / display.MapScale),
                    _font.Size,
                    MinFontSize,
                    MaxFontSize);

                fac = options.LabelRefScaleFactor(fac);
            }
            fac *= options.DpiFactor;

            BlockoutTextSymbol tSym = new BlockoutTextSymbol(Current.Engine.CreateFont(_font.Name, Math.Max(_font.Size * fac / display.Screen.LargeFontsFactor, 2f), _font.Style), _brush.Color, _outlinebrush.Color, _padding, _borderPen?.Color, _borderPen?.Width ?? 0f);
            tSym.HorizontalOffset = HorizontalOffset * fac;
            tSym.VerticalOffset = VerticalOffset * fac;
            tSym.Angle = Angle;
            tSym._align = _align;
            tSym.Smoothingmode = this.Smoothingmode;
            tSym.MinFontSize = this.MinFontSize;
            tSym.MaxFontSize = this.MaxFontSize;
            tSym.SymbolSpacingType = this.SymbolSpacingType;
            tSym.SymbolSpacingX = this.SymbolSpacingX;
            tSym.SymbolSpacingY = this.SymbolSpacingY;
            tSym.IncludesSuperScript = this.IncludesSuperScript;
            tSym.SecondaryTextSymbolAlignments = this.SecondaryTextSymbolAlignments;

            return tSym;
        }

        #endregion IClone2 Members

        #region IPersistable Members

        override public void Load(IPersistStream stream)
        {
            base.Load(stream);

            this.ColorOutline = ArgbColor.FromArgb((int)stream.Load("outlinecolor", ArgbColor.Yellow.ToArgb()));
            this.Padding = (float)stream.Load("padding", 0f);
            this.BorderColor = ArgbColor.FromArgb((int)stream.Load("bordercolor", ArgbColor.Transparent.ToArgb()));
            this.BorderWidth = (float)stream.Load("borderwidth", 0f);
        }

        override public void Save(IPersistStream stream)
        {
            base.Save(stream);

            stream.Save("outlinecolor", this.ColorOutline.ToArgb());
            stream.Save("padding", this.Padding);
            stream.Save("bordercolor", this.BorderColor.ToArgb());
            stream.Save("borderwidth", this.BorderWidth);
        }

        #endregion IPersistable Members

        override protected int DrawingLevels { get { return 2; } }

        override protected void DrawAtPoint(IDisplay display, IPoint point, string text, float angle, IDrawTextFormat format, int level)
        {
            if (_font != null)
            {
                //point.X+=_xOffset;
                //point.Y+=_yOffset;

                try
                {
                    display.Canvas.TranslateTransform(new CanvasPointF((float)point.X, (float)point.Y));
                    if (angle != 0 || _angle != 0 || _rotation != 0)
                    {
                        var transformRotation = angle + _angle + _rotation;

                        if (display.DisplayTransformation.UseDisplayRotation)
                        {
                            transformRotation -= (float)display.DisplayTransformation.DisplayRotation;
                        }

                        display.Canvas.RotateTransform(transformRotation);
                    }

                    if (level < 0 || level == 0)
                    {
                        var size = display.Canvas.MeasureText(text, _font).AddPadding(_font);
                        var rect = new CanvasRectangleF(0f, 0f, size.Width, size.Height);

                        switch (format.Alignment)
                        {
                            case StringAlignment.Center:
                                rect.Offset(-size.Width / 2, 0f);
                                break;

                            case StringAlignment.Far:
                                rect.Offset(-size.Width, 0f);
                                break;
                        }
                        switch (format.LineAlignment)
                        {
                            case StringAlignment.Center:
                                rect.Offset(0f, -size.Height / 2);
                                break;

                            case StringAlignment.Far:
                                rect.Offset(0f, -size.Height);
                                break;
                        }

                        rect = rect.AddOffsetPadding(_font, format);

                        // Grow symmetrically around the already-positioned box - same reasoning as
                        // SimpleTextSymbol.AnnotationPolygon: padding must extend evenly on every
                        // side regardless of which edge/corner the alignment anchors to the
                        // feature. Kept in sync with Margin, which reports the same amount.
                        if (_padding > 0f)
                        {
                            rect.Left -= _padding;
                            rect.Top -= _padding;
                            rect.Width += 2f * _padding;
                            rect.Height += 2f * _padding;
                        }

                        display.Canvas.SmoothingMode = (SmoothingMode)this.Smoothingmode;
                        display.Canvas.FillRectangle(_outlinebrush, rect);
                        if (BorderIsVisible)
                        {
                            display.Canvas.DrawRectangle(_borderPen, rect);
                        }
                        display.Canvas.SmoothingMode = SmoothingMode.None;
                    }

                    if (level < 0 || level == 1)
                    {
                        display.Canvas.TextRenderingHint = (this.Smoothingmode == SymbolSmoothing.None) ?
                            TextRenderingHint.SystemDefault :
                            TextRenderingHint.AntiAlias;

                        DrawString(display.Canvas, text, _font, _brush, _xOffset, _yOffset, format);

                        display.Canvas.TextRenderingHint = TextRenderingHint.SystemDefault;
                    }
                }
                finally
                {
                    display.Canvas.ResetTransform();
                }
            }
        }
    }
}