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
using GeoAPI.Geometries;
using Proj4Net.Utility;

namespace Proj4Net.Projection
{


    public class MolleweideProjection : PseudoCylindricalProjection
    {

        public enum MollweideTypes
        {
            Mollweide = 0,
            Wagner4,
            Wagner5
        }
        /*
        public static final int MOLLEWEIDE = 0;
        public static final int WAGNER4 = 1;
        public static final int WAGNER5 = 2;
        */

        private const Int32 MaxIter = 10;
        private const double Tolerance = 1e-7;

        private readonly MollweideTypes _type = MollweideTypes.Mollweide;
        private double _cx, _cy, _cp;

        public MolleweideProjection()
            : this(ProjectionMath.PiHalf)
        {
        }

        public MolleweideProjection(MollweideTypes type)
        {
            _type = type;
            switch (type)
            {
                case MollweideTypes.Mollweide:
                    Init(Math.PI / 2);
                    break;
                case MollweideTypes.Wagner4:
                    Init(Math.PI / 3);
                    break;
                case MollweideTypes.Wagner5:
                    Init(Math.PI / 2);
                    _cx = 0.90977;
                    _cy = 1.65014;
                    _cp = 3.00896;
                    break;
            }
        }

        public MolleweideProjection(double p)
        {
            Init(p);
        }

        public void Init(double p)
        {
            double r, sp, p2 = p + p;

            sp = Math.Sin(p);
            r = Math.Sqrt(Math.PI * 2.0 * sp / (p2 + Math.Sin(p2)));
            _cx = 2.0 * r / Math.PI;
            _cy = r / sp;
            _cp = p2 + Math.Sin(p2);
        }

        public MolleweideProjection(double cx, double cy, double cp)
        {
            _cx = cx;
            _cy = cy;
            _cp = cp;
        }

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            double k, v;
            int i;

            k = _cp * Math.Sin(lpphi);
            for (i = MaxIter; i != 0; i--)
            {
                lpphi -= v = (lpphi + Math.Sin(lpphi) - k) / (1.0 + Math.Cos(lpphi));
                if (Math.Abs(v) < Tolerance)
                    break;
            }
            if (i == 0)
                lpphi = (lpphi < 0.0) ? -ProjectionMath.PiHalf : ProjectionMath.PiHalf;
            else
                lpphi *= 0.5;
            xy.X = _cx * lplam * Math.Cos(lpphi);
            xy.Y = _cy * Math.Sin(lpphi);
            return xy;
        }

        public override Coordinate ProjectInverse(double x, double y, Coordinate lp)
        {
            double lat, lon;

            lat = Math.Asin(y / _cy);
            lon = x / (_cx * Math.Cos(lat));
            lat += lat;
            lat = Math.Asin((lat + Math.Sin(lat)) / _cp);
            lp.X = lon;
            lp.Y = lat;
            return lp;
        }

        /// <summary>
        /// Gets if this projection is equal area
        /// </summary>
        public override Boolean IsEqualArea
        {
            get { return true; }
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override String ToString()
        {
            switch (_type)
            {
                case MollweideTypes.Wagner4:
                    return "Wagner IV";
                case MollweideTypes.Wagner5:
                    return "Wagner V";
            }
            return "Molleweide";
        }
    }
}