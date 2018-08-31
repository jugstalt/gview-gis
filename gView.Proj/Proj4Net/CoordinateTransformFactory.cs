
namespace Proj4Net
{

    /// <summary>
    /// Creates <see cref="ICoordinateTransform"/>s
    /// from source and target <see cref="CoordinateReferenceSystem"/>s.
    /// </summary>
    /// <author>mbdavis</author>
    public class CoordinateTransformFactory
    {
        ///<summary>
        /// Creates a new factory.
        /// </summary>
        public CoordinateTransformFactory()
        {
        }

        /// <summary>
        /// Creates a transformation from a source CRS to a target CRS,
        /// following the logic in PROJ.4.
        /// The transformation may include any or all of inverse projection, datum transformation,
        /// and reprojection, depending on the nature of the coordinate reference systems 
        /// provided.
        /// </summary>
        /// <param name="sourceCRS">The source CoordinateReferenceSystem</param>
        /// <param name="targetCRS">The target CoordinateReferenceSystem</param>
        /// <returns>A tranformation from the source CRS to the target CRS</returns>
        public ICoordinateTransform CreateTransform(CoordinateReferenceSystem sourceCRS,
                                                    CoordinateReferenceSystem targetCRS)
        {
            return new BasicCoordinateTransform(sourceCRS, targetCRS);
        }
    }
}