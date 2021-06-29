using gView.GraphicsEngine.Abstraction;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace gView.GraphicsEngine.GdiPlus
{
    public class DrawTextFormat : IDrawTextFormat
    {
        private StringFormat _stringFormat;

        public DrawTextFormat()
        {
            _stringFormat = new StringFormat();
            //_stringFormat.FormatFlags = System.Drawing.StringFormatFlags.DirectionRightToLeft;
        }

        public StringAlignment Alignment
        {
            get
            {
                return (StringAlignment)_stringFormat.Alignment;
            }
            set
            {
                _stringFormat.Alignment = (System.Drawing.StringAlignment)value;
            }
        }

        public StringAlignment LineAlignment
        {
            get
            {
                return (StringAlignment)_stringFormat.LineAlignment;
            }
            set
            {
                _stringFormat.LineAlignment = (System.Drawing.StringAlignment)value;
            }
        }

        public object EngineElement => _stringFormat;
    }
}
