using System;

namespace gView.GraphicsEngine
{
    public struct ArgbColor
    {
        public byte A { get; private set; }
        public byte R { get; private set; }
        public byte G { get; private set; }
        public byte B { get; private set; }

        public int ToArgb() => (this.A << 24) | (this.R << 16) | (this.G << 8) | this.B;

        public bool IsTransparent => this.A == 0;

        public static ArgbColor Empty => ArgbColor.FromArgb(0, 0, 0, 0);
        public static ArgbColor Transparent => ArgbColor.FromArgb(0, 255, 255, 255);
        public static ArgbColor White => ArgbColor.FromArgb(255, 255, 255);
        public static ArgbColor Black => ArgbColor.FromArgb(0, 0, 0);
        public static ArgbColor LightGray => ArgbColor.FromArgb(200, 200, 200);
        public static ArgbColor Gray => ArgbColor.FromArgb(128, 128, 128);
        public static ArgbColor Red => ArgbColor.FromArgb(255, 0, 0);
        public static ArgbColor Green => ArgbColor.FromArgb(0, 255, 0);
        public static ArgbColor Blue => ArgbColor.FromArgb(0, 0, 255);
        public static ArgbColor AliceBlue => ArgbColor.FromArgb(240, 248, 255);
        public static ArgbColor Yellow => ArgbColor.FromArgb(255, 255, 0);
        public static ArgbColor Cyan => ArgbColor.FromArgb(0, 255, 255);
        public static ArgbColor Orange => ArgbColor.FromArgb(255, 165, 0);

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

        public static ArgbColor FromArgb(int argb)
        {
            return ArgbColor.FromArgb((byte)(argb >> 24),
                                      (byte)(argb >> 16),
                                      (byte)(argb >> 8),
                                      (byte)(argb));
        }

        public static ArgbColor FromHexString(string hex)
        {
            if (hex.StartsWith("#"))
            {
                hex = hex.Substring(1);
            }

            if (hex.Length == 3)
            {
                hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
            }
            else if (hex.Length == 4)
            {
                hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}{hex[3]}{hex[3]}";
            }

            byte a = 255, r, g, b;

            if (hex.Length == 8)
            {
                a = Convert.ToByte(hex.Substring(0, 2), 16);
                hex = hex.Substring(2);
            }

            r = Convert.ToByte(hex.Substring(0, 2), 16);
            g = Convert.ToByte(hex.Substring(2, 2), 16);
            b = Convert.ToByte(hex.Substring(4, 2), 16);

            return new ArgbColor { A = a, R = r, G = g, B = b };
        }

        public override bool Equals(object obj)
        {
            if (obj is ArgbColor)
            {
                return ((ArgbColor)obj).ToArgb() == this.ToArgb();
            }

            return false;
        }

        public bool EqualBase(ArgbColor col)
            => this.R == col.R && this.G == col.G && this.B == col.B;

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return String.Join("; ", A < 255 ? new int[] { A, R, G, B } : new int[] { R, G, B });
        }
    }
}
