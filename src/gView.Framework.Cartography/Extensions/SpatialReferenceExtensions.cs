using gView.Framework.Core.Common;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static gView.Framework.Cartography.Map;

namespace gView.Framework.Cartography.Extensions;

static internal class SpatialReferenceExtensions
{
    static public bool IsValidMapSpatialReference(this ISpatialReference spatialReference)
        => spatialReference?
                .Parameters?
                .Any(p => !String.IsNullOrEmpty(p)) == true;

    static public ISpatialReference MakeValid(this ISpatialReference spatialReference, ConcurrentBag<ErrorMessage> errorMessages = null)
    {
        if (spatialReference == null) return null;

        if (!spatialReference.IsValidMapSpatialReference())
        {
            if(String.IsNullOrEmpty(spatialReference.Name))
            {
                errorMessages?.AddWarningMessage($"Invalid SpatialReference with no parameters and name found");
                return spatialReference;
            }

            errorMessages?.AddWarningMessage($"Invalid SpatialReference with no parameters found: {spatialReference.Name}. Parameters will loaded from proj.db");
            return SpatialReference.FromID(spatialReference.Name) ?? spatialReference;
        }

        return spatialReference;
    }
}
