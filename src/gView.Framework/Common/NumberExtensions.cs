﻿using System;

namespace gView.Framework.Common
{
    static public class NumberExtensions
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

        static public string ToFloatString(this float d)
        {
            return d.ToString(Numbers.Nhi);
        }

        static public string ToDoubleString(this double d)
        {
            return d.ToString(Numbers.Nhi);
        }

        static public string ToDecimalString(this decimal d)
        {
            return d.ToString(Numbers.Nhi);
        }

        static public bool EqualWithTolerance(this double a, double b, double tolerance = 1e-8)
        {
            return Math.Abs(a - b) < tolerance;
        }
    }
}
