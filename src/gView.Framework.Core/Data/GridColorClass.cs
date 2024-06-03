using gView.Framework.Core.IO;
using gView.GraphicsEngine;

namespace gView.Framework.Core.Data
{
    public class GridColorClass : IPersistable
    {
        public double MinValue;
        public double MaxValue;
        public ArgbColor Color;
        public string Legend;

        public GridColorClass(double minValue, double maxValue, ArgbColor color)
        {
            MinValue = minValue;
            MaxValue = maxValue;
            Color = color;
        }

        static public ArgbColor FindColor(double Value, GridColorClass[] classes)
        {
            if (classes == null)
            {
                return ArgbColor.White;
            }

            foreach (GridColorClass cc in classes)
            {
                if (Value >= cc.MinValue &&
                    Value <= cc.MaxValue)
                {
                    return cc.Color;
                }
            }

            return ArgbColor.Transparent;
        }

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            MinValue = (double)stream.Load("MinValue", 0.0);
            MaxValue = (double)stream.Load("MaxValue", 0.0);
            Color = ArgbColor.FromArgb((int)stream.Load("Color", ArgbColor.White.ToArgb()));
            Legend = (string)stream.Load("Legend", string.Empty);
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("MinValue", MinValue);
            stream.Save("MaxValue", MaxValue);
            stream.Save("Color", Color.ToArgb());
            stream.Save("Legend", Legend);
        }

        #endregion
    }
}