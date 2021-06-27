using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine
{
    public struct ArgbColor
    {
        public byte A { get; private set; }
        public byte R { get; private set; }
        public byte G { get; private set; }
        public byte B { get; private set; }

        public static ArgbColor FromArgb(int alpha, ArgbColor baseColor)
        {
            return new ArgbColor()
            {
                A = (byte)alpha,
                R = baseColor.R,
                G = baseColor.G,
                B = baseColor.B
            };
        }
        
        public static ArgbColor FromArgb(int red, int green, int blue)
        {
            return new ArgbColor()
            {
                A = 255,
                R = (byte)red,
                G = (byte)green,
                B = (byte)blue
            };
        }
        
        public static ArgbColor FromArgb(int alpha, int red, int green, int blue)
        {
            return new ArgbColor()
            {
                A = (byte)alpha,
                R = (byte)red,
                G = (byte)green,
                B = (byte)blue
            };
        }
    }
}
