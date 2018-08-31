using System;
using GeoAPI.Geometries;

namespace Proj4Net.Utility
{
    public static class CoordinateChecker
    {
        public static bool HasValidXandYOrdinate(Coordinate c)
        {
            if (Double.IsNaN(c.X) || Double.IsInfinity(c.X))
            {
                return false;
            }

            return !Double.IsNaN(c.Y) && !Double.IsInfinity(c.Y);
        }

        public static bool HasValidZOrdinate(Coordinate c)
        {
            return !Double.IsNaN(c.Z) && !Double.IsInfinity(c.Z);
        }

        /// <summary>
        /// Returns a boolean indicating if the X ordinate value of the 
        /// provided <paramref name="one"/> as an ordinate is equal to the X ordinate
        /// value of <paramref name="another"/>. Because we are working with floating
        /// point numbers the ordinates are considered equal if the difference
        /// between them is less than the specified tolerance.
        /// </summary>
        public static Boolean AreXOrdinatesEqual(/*this*/ Coordinate one, Coordinate another,
                                                 double argTolerance)
        {
            // Subtract the _x ordinate values and then see if the difference
            // between them is less than the specified tolerance. If the difference
            // is less, return true.
            var difference = Math.Abs(another.X - one.X);

            return (difference <= argTolerance);
        }

        /// <summary>
        /// Returns a boolean indicating if the Y ordinate value of the 
        /// provided <paramref name="one"/> as an ordinate is equal to the Y ordinate
        /// value of <paramref name="another"/>. Because we are working with floating
        /// point numbers the ordinates are considered equal if the difference
        /// between them is less than the specified tolerance.
        /// </summary>
        public static Boolean AreYOrdinatesEqual(/*this*/ Coordinate one, Coordinate another,
                                                 double argTolerance)
        {
            // Subtract the _y ordinate values and then see if the difference
            // between them is less than the specified tolerance. If the difference
            // is less, return true.
            var difference = Math.Abs(another.Y - one.Y);

            return (difference <= argTolerance);
        }

        /// <summary>
        /// Returns a boolean indicating if the Y ordinate value of the 
        /// provided <paramref name="one"/> as an ordinate is equal to the Y ordinate
        /// value of <paramref name="another"/>. Because we are working with floating
        /// point numbers the ordinates are considered equal if the difference
        /// between them is less than the specified tolerance.
        /// <para/>
        /// If both Z ordinate values are Double.NaN this method will return
        /// true. If one Z ordinate value is a valid double value and one is
        /// <see cref="Double.NaN"/>, this method will return false.
        /// </summary>
        /// 
        public static Boolean AreZOrdinatesEqual(/*this*/ Coordinate one, Coordinate another,
                                                 double argTolerance)
        {
            // We have to handle Double.NaN values here, because not every
            // ProjCoordinate will have a valid Z Value.
            if (Double.IsNaN(one.Z))
            {
                if (Double.IsNaN(another.Z))
                {
                    // Both the _z ordinate values are Double.NaN. Return true.
                    return true;
                }

                // We've got one _z ordinate with a valid value and one with
                // a Double.NaN value. Return false.
                return false;
            }

            // We have a valid _z ordinate value in this ProjCoordinate object.
            if (Double.IsNaN(another.Z))
            {
                // We've got one _z ordinate with a valid value and one with
                // a Double.NaN value. Return false.
                return false;
            }

            // If we get to this point in the method execution, we have to
            // _z ordinates with valid values, and we need to do a regular 
            // comparison. This is done in the remainder of the method.

            // Subtract the _z ordinate values and then see if the difference
            // between them is less than the specified tolerance. If the difference
            // is less, return true.
            double difference = Math.Abs(another.Z - one.Z);

            return (difference <= argTolerance);
        }

        public static void ClearZ(/*this*/ Coordinate geoCoord)
        {
            geoCoord.Z = double.NaN;
        }
    }
}