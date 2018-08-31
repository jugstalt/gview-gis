/*
Copyright 2006 Jerry Huxtable

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
using System;
using System.Globalization;
namespace Proj4Net.Units
{
    [Serializable]
    public class Unit : IEquatable<Unit>
    {

        //const long SerialVersionUid = -6704954923429734628L;

        public enum UnitTypes
        {
            AngleUnit, LengthUnit, AreaUnit, VolumeUnit
        }
        /*
        public const int AngleUnit = 0;
        public const int LengthUnit = 1;
        public const int AreaUnit = 2;
        public const int VolumeUnit = 3;
        */
        private readonly String _name;
        private readonly String _plural;
        private readonly String _abbreviation;
        private readonly double _value;
        private static readonly NumberFormatInfo _format;

        static Unit()
        {
            _format = (NumberFormatInfo)NumberFormatInfo.InvariantInfo.Clone();
            _format.NumberDecimalDigits = 2;
            //format.setGroupingUsed(false);
        }

        public Unit(String name, String plural, String abbreviation, double value)
        {
            _name = name;
            _plural = plural;
            _abbreviation = abbreviation;
            _value = value;
        }

        public string Name
        {
            get { return _name; }
        }

        public string Plural
        {
            get { return _plural; }
        }

        public string Abbreviation
        {
            get { return _abbreviation; }
        }

        public double Value
        {
            get { return _value; }
        }

        public double ToBase(double n)
        {
            return n * _value;
        }

        public double FromBase(double n)
        {
            return n / _value;
        }

        public Double Parse(String s)
        {
            double val;
            if (Double.TryParse(s, NumberStyles.Float, _format, out val))
                return val;

            throw new Exception();
        }

        public String Format(double n)
        {
            return String.Format(_format, "{0} {1}", n, _abbreviation);
        }

        public String Format(double n, Boolean abbrev)
        {
            if (abbrev)
                return String.Format(_format, "{0} {1}", n, _abbreviation);
            return String.Format(_format, "{0}", n);
        }

        public String Format(double x, double y, Boolean abbrev)
        {
            if (abbrev)
                return String.Format(_format, "{0}/{1} {2}", x, y, _abbreviation);
            return String.Format(_format, "{0}/{1}", x, y);
        }

        public String Format(double x, double y)
        {
            return Format(x, y, true);
        }

        public override String ToString()
        {
            return _plural;
        }

        public Boolean Equals(Unit o)
        {
            if (o != null)
            {
                return o._value == _value;
            }
            return false;
        }

    }
}