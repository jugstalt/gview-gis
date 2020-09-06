using System;

namespace gView.Framework.Symbology
{
    internal class SymbolTransformation
    {
        static public void Transform(float angle, float h, float v, out float x, out float y)
        {
            double c = Math.Cos(angle * Math.PI / 180.0);
            double s = Math.Sin(angle * Math.PI / 180.0);

            x = (float)(h * c + v * s);
            y = (float)(-h * s + v * c);
        }

        static public float[] Rotate(float angle, float x, float y)
        {
            double c = Math.Cos(angle * Math.PI / 180.0);
            double s = Math.Sin(angle * Math.PI / 180.0);

            float[] xx = new float[2];
            xx[0] = (float)(x * c + y * s);
            xx[1] = (float)(-x * s + y * c);

            return xx;
        }
    }
}
