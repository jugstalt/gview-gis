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
    ///<summary>The Equidistant Conic projection.</summary>
    public class EquidistantConicProjection : ConicProjection
    {

        private readonly double _standardLatitude1;
        private readonly double _standardLatitude2;

        private const double _eccentricity = 0.822719;
        private const double _eccentricity2 = _eccentricity * _eccentricity;
        private const double _eccentricity4 = _eccentricity2 * _eccentricity2;
        private const double _eccentricity6 = _eccentricity2 * _eccentricity4;
        private const double _radius = 1;

        private Boolean _northPole;
        private double _f, _n, _rho0;

        public EquidistantConicProjection()
        {
            MinLatitude = ProjectionMath.ToRadians(10);
            MaxLatitude = ProjectionMath.ToRadians(70);
            MinLongitude = ProjectionMath.ToRadians(-90);
            MaxLongitude = ProjectionMath.ToRadians(90);
            _standardLatitude1 = ProjectionMath.ToDegrees(60); //???
            _standardLatitude2 = ProjectionMath.ToDegrees(20); //???

            Initialize(ProjectionMath.ToRadians(0), ProjectionMath.ToRadians(37.5), _standardLatitude1, _standardLatitude2);
        }

        public override Coordinate Project(Coordinate input, Coordinate output)
        {
            double lon = ProjectionMath.NormalizeLongitude(input.X - ProjectionLongitude);
            double lat = input.Y;

            double hold2 = Math.Pow(((1.0 - _eccentricity * Math.Sin(lat)) / (1.0 + _eccentricity * Math.Sin(lat))), 0.5 * _eccentricity);
            double hold3 = Math.Tan(ProjectionMath.PiFourth - 0.5 * lat);
            double hold1 = (hold3 == 0.0) ? 0.0 : Math.Pow(hold3 / hold2, _n);
            double rho = _radius * _f * hold1;
            double theta = _n * lon;

            output.X = rho * Math.Sin(theta);
            output.Y = _rho0 - rho * Math.Cos(theta);
            return output;
        }

        public override Coordinate InverseProject(Coordinate input, Coordinate output)
        {
            double theta, temp, rho, t, tphi, phi = 0, delta;

            theta = Math.Atan(input.X / (_rho0 - input.Y));
            output.X = (theta / _n) + ProjectionLongitude;

            temp = input.X * input.X + (_rho0 - input.Y) * (_rho0 - input.Y);
            rho = Math.Sqrt(temp);
            if (_n < 0)
                rho = -rho;
            t = Math.Pow((rho / (_radius * _f)), 1.0 / _n);
            tphi = ProjectionMath.PiHalf - 2.0 * Math.Atan(t);
            delta = 1.0;
            for (int i = 0; i < 100 && delta > 1.0e-8; i++)
            {
                temp = (1.0 - _eccentricity * Math.Sin(tphi)) / (1.0 + _eccentricity * Math.Sin(tphi));
                phi = ProjectionMath.PiHalf - 2.0 * Math.Atan(t * Math.Pow(temp, 0.5 * _eccentricity));
                delta = Math.Abs(Math.Abs(tphi) - Math.Abs(phi));
                tphi = phi;
            }
            output.Y = phi;
            return output;
        }

        private void Initialize(double rlong0, double rlat0, double standardLatitude1, double standardLatitude2)
        {
            base.Initialize();
            double t_standardLatitude1, m_standardLatitude1, t_standardLatitude2, m_standardLatitude2, t_rlat0;

            _northPole = rlat0 > 0.0;
            ProjectionLatitude = _northPole ? ProjectionMath.PiHalf : -ProjectionMath.PiHalf;

            t_standardLatitude1 = Math.Tan(ProjectionMath.PiFourth - 0.5 * standardLatitude1) / Math.Pow((1.0 - _eccentricity *
                Math.Sin(standardLatitude1)) / (1.0 + _eccentricity * Math.Sin(standardLatitude1)), 0.5 * _eccentricity);
            m_standardLatitude1 = Math.Cos(standardLatitude1) / Math.Sqrt(1.0 - _eccentricity2
                * Math.Pow(Math.Sin(standardLatitude1), 2.0));
            t_standardLatitude2 = Math.Tan(ProjectionMath.PiFourth - 0.5 * standardLatitude2) / Math.Pow((1.0 - _eccentricity *
                Math.Sin(standardLatitude2)) / (1.0 + _eccentricity * Math.Sin(standardLatitude2)), 0.5 * _eccentricity);
            m_standardLatitude2 = Math.Cos(standardLatitude2) / Math.Sqrt(1.0 - _eccentricity2
                * Math.Pow(Math.Sin(standardLatitude2), 2.0));
            t_rlat0 = Math.Tan(ProjectionMath.PiFourth - 0.5 * rlat0) /
                Math.Pow((1.0 - _eccentricity * Math.Sin(rlat0)) /
                (1.0 + _eccentricity * Math.Sin(rlat0)), 0.5 * _eccentricity);

            if (standardLatitude1 != standardLatitude2)
                _n = (Math.Log(m_standardLatitude1) - Math.Log(m_standardLatitude2)) / (Math.Log(t_standardLatitude1) - Math.Log(t_standardLatitude2));
            else
                _n = Math.Sin(standardLatitude1);

            _f = m_standardLatitude1 / (_n * Math.Pow(t_standardLatitude1, _n));
            ProjectionLongitude = rlong0;
            _rho0 = _radius * _f * Math.Pow(t_rlat0, _n);
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override String ToString()
        {
            return "Equidistant Conic";
        }

    }

}