#nullable enable

using gView.Framework.Core.Geometry;

namespace gView.Framework.Geometry.Extensions;

static internal class DatumTransformationsExtensions
{
    public static IGeodeticDatum? GetTransformationFor(
                this IDatumTransformations datumTransformations,
                IGeodeticDatum? geodeticDatum,
                bool equalName = false,
                bool equalParameters = true)
    {
        // find equal FromDatum in Transformations and return 
        // otherwise return geodeticDatum
        if (datumTransformations?.Transformations is null
            || geodeticDatum is null)
        {
            return geodeticDatum;
        }

        foreach (var datumTransformation in datumTransformations.Transformations)
        {
            if (datumTransformation.FromDatum.IsEqual(geodeticDatum, equalName, equalParameters))
            {
                return datumTransformation.TransformationDatum;
            }
        }

        return geodeticDatum;
    }
}
