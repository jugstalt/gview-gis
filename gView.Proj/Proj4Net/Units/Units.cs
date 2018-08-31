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
using Proj4Net.Utility;

namespace Proj4Net.Units
{

    public static class Units
    {

        // Angular units
        public readonly static Unit Degrees = new DegreeUnit();
        public readonly static Unit Radians = new Unit("radian", "radians", "rad", 1d * ProjectionMath.RadiansToDegrees);
        public readonly static Unit ArcMinutes = new Unit("arc minute", "arc minutes", "min", 1 / 60.0);
        public readonly static Unit ArcSeconds = new Unit("arc second", "arc seconds", "sec", 1 / 3600.0);

        // Distance units

        // Metric units
        public readonly static Unit Kilometres = new Unit("kilometre", "kilometres", "km", 1000);
        public readonly static Unit Metres = new Unit("metre", "metres", "m", 1);
        public readonly static Unit Decimetres = new Unit("decimetre", "decimetres", "dm", 0.1);
        public readonly static Unit Centimetres = new Unit("centimetre", "centimetres", "cm", 0.01);
        public readonly static Unit Millimetres = new Unit("millimetre", "millimetres", "mm", 0.001);

        // International units
        public readonly static Unit NauticalMiles = new Unit("nautical mile", "nautical miles", "kmi", 1852);
        public readonly static Unit Miles = new Unit("mile", "miles", "mi", 1609.344);
        public readonly static Unit Chains = new Unit("chain", "chains", "ch", 20.1168);
        public readonly static Unit Yards = new Unit("yard", "yards", "yd", 0.9144);
        public readonly static Unit Feet = new Unit("foot", "feet", "ft", 0.3048);
        public readonly static Unit Inches = new Unit("inch", "inches", "in", 0.0254);

        // U.S. units
        public readonly static Unit UsMiles = new Unit("U.S. mile", "U.S. miles", "us-mi", 1609.347218694437);
        public readonly static Unit UsChains = new Unit("U.S. chain", "U.S. chains", "us-ch", 20.11684023368047);
        public readonly static Unit UsYards = new Unit("U.S. yard", "U.S. yards", "us-yd", 0.914401828803658);
        public readonly static Unit UsFeet = new Unit("U.S. foot", "U.S. feet", "us-ft", 0.304800609601219);
        public readonly static Unit UsInches = new Unit("U.S. inch", "U.S. inches", "us-in", 1.0 / 39.37);

        // Miscellaneous units
        public readonly static Unit Fathoms = new Unit("fathom", "fathoms", "fath", 1.8288);
        public readonly static Unit Links = new Unit("link", "links", "link", 0.201168);

        public readonly static Unit Points = new Unit("point", "points", "point", 0.0254 / 72.27);

        public static Unit[] PredifinedUnits = {
                                                    Degrees,
                                                    Kilometres, Metres, Decimetres, Centimetres, Millimetres,
                                                    Miles, Yards, Feet, Inches,
                                                    UsMiles, UsYards, UsFeet, UsInches,
                                                    NauticalMiles
                                               };

        public static Unit FindUnit(String name)
        {
            for (int i = 0; i < PredifinedUnits.Length; i++)
            {
                if (name.Equals(PredifinedUnits[i].Name) || name.Equals(PredifinedUnits[i].Plural) || name.Equals(PredifinedUnits[i].Abbreviation))
                    return PredifinedUnits[i];
            }
            return Metres;
        }

        public static double Convert(double value, Unit from, Unit to)
        {
            if (from == to)
                return value;
            return to.FromBase(from.ToBase(value));
        }

    }
}