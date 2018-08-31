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

namespace Proj4Net
{
    ///<summary>
    /// Signals that an erroneous situation has occured during the computation of a projected coordinate system value. 
    ///</summary>
    public class ProjectionException : Proj4NetException
    {

        public ProjectionException()
        {
        }

        public ProjectionException(String message)
            : base(message)
        {
        }

        public ProjectionException(Projection.Projection proj, String message)
            : this(proj + ": " + message)
        {
        }
    }
}