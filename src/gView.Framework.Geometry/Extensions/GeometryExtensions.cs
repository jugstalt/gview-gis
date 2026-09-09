#nullable enable

using gView.Framework.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace gView.Framework.Geometry.Extensions;

public static class GeometryExtensions
{
    extension(IGeometry? geometry)
    {
        public bool IsNullGeometry()
            => geometry switch
            {
                null => true,
                // NULL Geoemtry is sumetimes stored as GEOMETRYCOLLECTION EMPTY (WKT)
                IAggregateGeometry agg when agg.GeometryCount == 0 => true,
                _ => false,
            };
    }
}
