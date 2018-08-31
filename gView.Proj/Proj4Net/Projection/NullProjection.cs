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

namespace Proj4Net.Projection
{
    public class NullProjection : Projection
    {
        public NullProjection()
        {
            Initialize();
        }

        public override Coordinate Project(double x, double y, Coordinate dst)
        {
            dst.X = x;
            dst.Y = y;
            return dst;
        }

        public override Coordinate Project(Coordinate src, Coordinate dst)
        {
            dst.X = src.X;
            dst.Y = src.Y;
            return dst;
        }

        public override Coordinate ProjectInverse(double x, double y, Coordinate dst)
        {
            dst.X = x;
            dst.Y = y;
            return dst;
        }

        public override Coordinate ProjectInverse(Coordinate src, Coordinate dst)
        {
            dst.X = src.X;
            dst.Y = src.Y;
            return dst;
        }

        /*
        public Shape projectPath(Shape path, AffineTransform t, boolean filled)
        {
            if (t != null)
                t.createTransformedShape(path);
            return path;
        }

        public Shape getBoundingShape()
        {
            return null;
        }

        public boolean isRectilinear()
        {
            return true;
        }
        */
        public override String ToString()
        {
            return "Null";
        }
    }
}