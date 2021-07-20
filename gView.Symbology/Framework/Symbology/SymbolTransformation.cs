using System;

namespace gView.Framework.Symbology
{
    internal class SymbolTransformation
    {
        const double ToRad = Math.PI / 180.0;

        static public void Transform(float angle, float h, float v, out float x, out float y)
        {
            double c = Math.Cos(angle * ToRad);
            double s = Math.Sin(angle * ToRad);

            x = (float)(h * c + v * s);
            y = (float)(-h * s + v * c);
        }

        static public float[] Rotate(float angle, float x, float y)
        {
            double c = Math.Cos(angle * ToRad);
            double s = Math.Sin(angle * ToRad);

            float[] xx = new float[2];
            xx[0] = (float)(x * c + y * s);
            xx[1] = (float)(-x * s + y * c);

            return xx;
        }
    }
}
