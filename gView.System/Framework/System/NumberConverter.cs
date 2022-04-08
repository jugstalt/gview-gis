namespace gView.Framework.system
{
    static public class NumberConverter
    {
        static public double ToDouble(this string value)
        {
            if (SystemInfo.IsWindows)
            {
                return double.Parse(value.Replace(",", "."), SystemInfo.Nhi);
            }

            return double.Parse(value.Replace(",", SystemInfo.Cnf.NumberDecimalSeparator));
        }

        static public float ToFloat(this string value)
        {
            if (SystemInfo.IsWindows)
            {
                return float.Parse(value.Replace(",", "."), SystemInfo.Nhi);
            }

            return float.Parse(value.Replace(",", SystemInfo.Cnf.NumberDecimalSeparator));
        }

        static public string ToDoubleString(this double d)
        {
            return d.ToString(Numbers.Nhi);
        }
    }
}
