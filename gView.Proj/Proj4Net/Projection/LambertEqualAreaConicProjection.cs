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


    public class LambertEqualAreaConicProjection : AlbersProjection
    {

        public LambertEqualAreaConicProjection()
            : this(false)
        {
            ;
        }

        public LambertEqualAreaConicProjection(Boolean south)
        {
            MinLatitude = ProjectionMath.ToRadians(0);
            MaxLatitude = ProjectionMath.ToRadians(90);
            ProjectionLatitude1 = south ? -ProjectionMath.PiFourth : ProjectionMath.PiFourth;
            ProjectionLatitude2 = south ? -ProjectionMath.PiHalf : ProjectionMath.PiHalf;
            Initialize();
        }

        public override String ToString()
        {
            return "Lambert Equal Area Conic";
        }

    }

}