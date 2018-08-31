using GeoAPI.Geometries;

namespace Proj4Net
{
    /// <summary>
    /// An interface for the operation of transforming 
    /// a <see cref="Coordinate"/> from one <see cref="CoordinateReferenceSystem"/> 
    /// into a different one.
    ///</summary>
    public interface ICoordinateTransform
    {

        /// <summary>
        /// The <see cref="CoordinateReferenceSystem"/> of the input coordinates
        /// </summary>
        CoordinateReferenceSystem SourceCRS { get; }

        /// <summary>
        /// The <see cref="CoordinateReferenceSystem"/> of the output coordinates
        /// </summary>
        CoordinateReferenceSystem TargetCRS { get; }

        ///<summary>
        ///Tranforms a coordinate from the source <see cref="CoordinateReferenceSystem"/> to the target one.
        ///</summary>
        ///<param name="src">The input coordinate to be transformed</param>
        ///<param name="tgt">The transformed coordinate</param>
        ///<returns>the target coordinate which was passed in</returns>
        ///<exception cref="Proj4NetException">If a computation error is encountered</exception>
        Coordinate Transform(Coordinate src, Coordinate tgt);

    }
}
