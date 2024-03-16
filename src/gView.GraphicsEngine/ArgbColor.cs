using System;
using System.Globalization;
using System.Text.RegularExpressions;

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

        public static ArgbColor FromColorName(string colorName)
        {
            return colorName.ToLowerInvariant() switch
            {
                "black" => FromArgb(255, 0, 0, 0),
                "white" => FromArgb(255, 255, 255, 255),
                "red" => FromArgb(255, 255, 0, 0),
                "green" => FromArgb(255, 0, 128, 0),
                "blue" => FromArgb(255, 0, 0, 255),
                "yellow" => FromArgb(255, 255, 255, 0),
                "cyan" => FromArgb(255, 0, 255, 255),
                "magenta" => FromArgb(255, 255, 0, 255),
                "silver" => FromArgb(255, 192, 192, 192),
                "gray" => FromArgb(255, 128, 128, 128),
                "maroon" => FromArgb(255, 128, 0, 0),
                "olive" => FromArgb(255, 128, 128, 0),
                "purple" => FromArgb(255, 128, 0, 128),
                "teal" => FromArgb(255, 0, 128, 128),
                "navy" => FromArgb(255, 0, 0, 128),
                "lime" => FromArgb(0, 255, 0),
                "orange" => FromArgb(255, 165, 0),
                "brown" => FromArgb(165, 42, 42),
                "pink" => FromArgb(255, 192, 203),
                "gold" => FromArgb(255, 215, 0),
                "beige" => FromArgb(245, 245, 220),
                "coral" => FromArgb(255, 127, 80),
                "indigo" => FromArgb(75, 0, 130),
                "violet" => FromArgb(238, 130, 238),
                "turquoise" => FromArgb(64, 224, 208),
                "tan" => FromArgb(210, 180, 140),
                "skyblue" => FromArgb(135, 206, 235),
                "salmon" => FromArgb(250, 128, 114),
                "plum" => FromArgb(221, 160, 221),
                "orchid" => FromArgb(218, 112, 214),
                "mint" => FromArgb(189, 252, 201),
                "ivory" => FromArgb(255, 255, 240),
                "azure" => FromArgb(240, 255, 255),
                "lavender" => FromArgb(230, 230, 250),
                _ => throw new ArgumentException("Unknown color", nameof(colorName))
            };
        }

        public static ArgbColor FromString(string colorString)
        {
            if (!String.IsNullOrEmpty(colorString))
            {
                if (colorString.StartsWith("#"))
                {
                    return ArgbColor.FromHexString(colorString);
                }

                colorString = colorString.Replace(" ", ""); // remove spaces

                var rgbaMatch = Regex.Match(colorString, @"rgba\((\d+),(\d+),(\d+),(\d*\.?\d+)\)");
                var rgbMatch = Regex.Match(colorString, @"rgb\((\d+),(\d+), (\d+)\)");
                var hslaMatch = Regex.Match(colorString, @"hsla\((\d+),(\d*\.?\d+)%,(\d*\.?\d+)%,(\d*\.?\d+)\)");
                var hslMatch = Regex.Match(colorString, @"hsl\((\d+),(\d*\.?\d+)%,(\d*\.?\d+)%\)");

                if (rgbaMatch.Success)
                {
                    float a = float.Parse(rgbaMatch.Groups[4].Value, CultureInfo.InvariantCulture);
                    return ArgbColor.FromArgb(
                        Convert.ToInt32(a * 255),
                        int.Parse(rgbaMatch.Groups[1].Value, CultureInfo.InvariantCulture),
                        int.Parse(rgbaMatch.Groups[2].Value, CultureInfo.InvariantCulture),
                        int.Parse(rgbaMatch.Groups[3].Value, CultureInfo.InvariantCulture)
                    );
                }
                else if (rgbMatch.Success)
                {
                    return ArgbColor.FromArgb(
                        255,
                        int.Parse(rgbMatch.Groups[1].Value, CultureInfo.InvariantCulture),
                        int.Parse(rgbMatch.Groups[2].Value, CultureInfo.InvariantCulture),
                        int.Parse(rgbMatch.Groups[3].Value, CultureInfo.InvariantCulture)
                    );
                }
                else if (hslaMatch.Success || hslMatch.Success)
                {
                    float h = float.Parse(hslaMatch.Groups[1].Value, CultureInfo.InvariantCulture);
                    float s = float.Parse(hslaMatch.Groups[2].Value, CultureInfo.InvariantCulture) / 100f;
                    float l = float.Parse(hslaMatch.Groups[3].Value, CultureInfo.InvariantCulture) / 100f;
                    float a = hslaMatch.Success
                              ? float.Parse(hslaMatch.Groups[4].Value, CultureInfo.InvariantCulture)
                              : 1.0f;

                    var rgb = HslToRgb(h, s, l);

                    return ArgbColor.FromArgb(
                        Convert.ToInt32(a * 255),
                        rgb.R,
                        rgb.G,
                        rgb.B
                    );
                }
                else
                {
                    return ArgbColor.FromColorName(colorString);
                }
            }

            throw new FormatException("Unknown color name");
        }

        public static bool TryFromString(string colorString, out ArgbColor color)
        {
            try
            {
                color = ArgbColor.FromString(colorString);
                return true;
            }
            catch
            {
                color = default;
                return false;
            }
        }

        #region Helper

        private static (int R, int G, int B) HslToRgb(float h, float s, float l)
        {
            float r, g, b;

            if (s == 0)
            {
                r = g = b = l; // achromatisch
            }
            else
            {
                var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                var p = 2 * l - q;
                r = HueToRgb(p, q, h + 1f / 3f);
                g = HueToRgb(p, q, h);
                b = HueToRgb(p, q, h - 1f / 3f);
            }

            return (Convert.ToByte(r * 255), Convert.ToByte(g * 255), Convert.ToByte(b * 255));
        }

        private static float HueToRgb(float p, float q, float t)
        {
            if (t < 0f) t += 1f;
            if (t > 1f) t -= 1f;
            if (t < 1f / 6f) return p + (q - p) * 6f * t;
            if (t < 1f / 2f) return q;
            if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
            return p;
        }

        #endregion

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
