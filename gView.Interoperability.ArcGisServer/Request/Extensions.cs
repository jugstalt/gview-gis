using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Interoperability.ArcGisServer.Request
{
    static class Extensions
    {
        static public int[] ToSize(this string val)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(val) || !val.Contains(","))
                    return new int[] { 400, 400 };

                var size = val.Split(',');
                return new int[] { int.Parse(size[0]), int.Parse(size[1]) };
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error parsing size parameter: " + val, ex);
            }
        }

        static public double[] ToBBox(this string val)
        {
            try
            {
                var bbox = val.Split(',');
                return new double[] {
                    NumberConverter.ToDouble(bbox[0]),
                    NumberConverter.ToDouble(bbox[1]),
                    NumberConverter.ToDouble(bbox[2]),
                    NumberConverter.ToDouble(bbox[3])
                };
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error parsing bbox parameter: " + val, ex);
            }
        }
    }
}
