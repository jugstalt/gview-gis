using gView.GraphicsEngine;

namespace gView.Framework.Common
{
    public class ColorConverter2
    {
        static public ArgbColor ConvertFrom(string colString)
        {
            colString = colString.Trim().ToLower();

            try
            {
                if (colString.StartsWith("#"))
                {
                    colString = colString.Substring(1);
                    string r, g, b;
                    if (colString.Length == 3)
                    {
                        r = colString[0].ToString() + colString[0].ToString();
                        g = colString[0].ToString() + colString[0].ToString();
                        b = colString[0].ToString() + colString[0].ToString();
                    }
                    else
                    {
                        r = colString[0].ToString() + colString[1].ToString();
                        g = colString[2].ToString() + colString[3].ToString();
                        b = colString[4].ToString() + colString[5].ToString();
                    }

                    return ArgbColor.FromArgb(
                        int.Parse(r, global::System.Globalization.NumberStyles.HexNumber),
                        int.Parse(g, global::System.Globalization.NumberStyles.HexNumber),
                        int.Parse(b, global::System.Globalization.NumberStyles.HexNumber));
                }
                else if (colString.StartsWith("rgb("))
                {
                    string[] rgb = colString.Replace("rgb(", "").Replace(")", "").Trim().Split(',');
                    if (rgb.Length == 3)
                    {
                        return ArgbColor.FromArgb(
                            int.Parse(rgb[0]),
                            int.Parse(rgb[1]),
                            int.Parse(rgb[2]));

                    }
                }
                else
                {
                    string[] rgb = colString.Trim().Split(',');
                    if (rgb.Length == 3)
                    {
                        return ArgbColor.FromArgb(
                            int.Parse(rgb[0]),
                            int.Parse(rgb[1]),
                            int.Parse(rgb[2]));
                    }
                    else if (rgb.Length == 4)
                    {
                        return ArgbColor.FromArgb(
                            int.Parse(rgb[0]),
                            int.Parse(rgb[1]),
                            int.Parse(rgb[2]),
                            int.Parse(rgb[3]));
                    }
                }
            }
            catch { }

            return ArgbColor.Red;
        }
    }
}
