using gView.Framework.Core.IO;
using gView.GraphicsEngine;
using gView.GraphicsEngine.Abstraction;

namespace gView.Framework.Symbology
{
    public class ColorGradient : IPersistable
    {
        private ArgbColor _col1, _col2;
        private float _angle = 0f;

        public ColorGradient(ArgbColor col1, ArgbColor col2)
        {
            _col1 = col1;
            _col2 = col2;
        }
        public ColorGradient(ColorGradient gradient)
        {
            if (gradient == null)
            {
                return;
            }

            _col1 = gradient.Color1;
            _col2 = gradient.Color2;
            _angle = gradient.Angle;
        }

        public ArgbColor Color1
        {
            get { return _col1; }
            set { _col1 = value; }
        }
        public ArgbColor Color2
        {
            get { return _col2; }
            set { _col2 = value; }
        }
        public float Angle
        {
            get { return _angle; }
            set { _angle = value; }
        }

        public IBrush CreateNewLinearGradientBrush(CanvasRectangleF rect)
        {
            return Current.Engine.CreateLinearGradientBrush(rect, _col1, _col2, _angle);
        }

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            this.Color1 = ArgbColor.FromArgb((int)stream.Load("color1", ArgbColor.Red.ToArgb()));
            this.Color2 = ArgbColor.FromArgb((int)stream.Load("color2", ArgbColor.Blue.ToArgb()));
            this.Angle = (float)stream.Load("angle", 0f);
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("color1", this.Color1.ToArgb());
            stream.Save("color2", this.Color2.ToArgb());
            stream.Save("angle", this.Angle);
        }

        #endregion

        public override string ToString()
        {
            return "Color Gradient";
        }
    }
}
