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

namespace Proj4Net.Projection
{
    /// <summary>
    /// Azimuthal mode enumeration
    /// </summary>
    public enum AzimuthalMode
    {
        /// <summary>
        /// North pole
        /// </summary>
        NorthPole = 1,

        /// <summary>
        /// South pole
        /// </summary>
        SouthPole = 2,

        /// <summary>
        /// Equator
        /// </summary>
        Equator = 3,

        /// <summary>
        /// Oblique
        /// </summary>
        Oblique = 4
    }

    ///<summary>
    /// The base class for all azimuthal map projections
    ///</summary>
    public abstract class AzimuthalProjection : Projection
    {

        protected AzimuthalMode _mode;
        protected double _sinphi0, _cosphi0;
        private double _mapRadius = 90.0;

        protected AzimuthalProjection()
            : this(ProjectionMath.ToRadians(45.0), ProjectionMath.ToRadians(45.0))
        {
        }

        protected AzimuthalProjection(double projectionLatitude, double projectionLongitude)
        {
            ProjectionLatitude = projectionLatitude;
            ProjectionLongitude = projectionLongitude;
            Initialize();
        }

        public override void Initialize()
        {
            base.Initialize();
            if (Math.Abs(Math.Abs(ProjectionLatitude) - ProjectionMath.PiHalf) < EPS10)
                _mode = ProjectionLatitude < 0.0 ? AzimuthalMode.SouthPole : AzimuthalMode.NorthPole;
            else if (Math.Abs(ProjectionLatitude) > EPS10)
            {
                _mode = AzimuthalMode.Oblique;
                _sinphi0 = Math.Sin(ProjectionLatitude);
                _cosphi0 = Math.Cos(ProjectionLatitude);
            }
            else
                _mode = AzimuthalMode.Equator;
        }

        public override Boolean Inside(double lon, double lat)
        {
            return ProjectionMath.GreatCircleDistance(
                       ProjectionMath.ToRadians(lon), ProjectionMath.ToRadians(lat), ProjectionLongitude,
                       ProjectionLatitude) < ProjectionMath.ToRadians(_mapRadius);
        }

        ///<summary>
        /// Gets or sets the map radius (in degrees). 180 shows a hemisphere, 360 shows the whole globe.
        ///</summary>
        public Double MapRadius
        {
            get { return _mapRadius; }
            set { _mapRadius = value; }
        }

        public AzimuthalMode Mode
        {
            get { return _mode; }
        }

    }

}