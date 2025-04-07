#nullable enable

using gView.Framework.Core.Geometry;
using System.Linq;

namespace gView.Framework.Geometry.Extensions;

static public class DatumTransformationsExtensions
{
    public static IGeodeticDatum? GetTransformationDatumFor(
                this IDatumTransformations datumTransformations,
                IGeodeticDatum? geodeticDatum,
                bool equalName = false,
                bool equalParameters = true)
    {
        var transformation = datumTransformations.GetTransformationFor(geodeticDatum, equalName, equalParameters);

        return transformation?.TransformationDatum ?? geodeticDatum;
    }

    public static IDatumTransformation? GetTransformationFor(
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
            return null;
        }

        foreach (var datumTransformation in datumTransformations.Transformations
                                                                .Where(dt => dt.Use && dt.FromDatum != null && dt.TransformationDatum != null))
        {
            if (datumTransformation.FromDatum.IsEqual(geodeticDatum, equalName, equalParameters))
            {
                return datumTransformation;
            }
        }

        return null;
    }
}
