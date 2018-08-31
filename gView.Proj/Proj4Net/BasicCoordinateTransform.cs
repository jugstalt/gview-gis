using System;
using GeoAPI.Geometries;
using Proj4Net.Datum;
using Proj4Net.Projection;
//using ICoordinateReferenceSystem = GeoAPI.CoordinateSystems.ICoordinateSystem;

namespace Proj4Net
{
    /// <summary>
    /// Represents the operation of transforming 
    /// a <see cref="Coordinate"/> from one <see cref="CoordinateReferenceSystem"/> 
    /// into a different one, using reprojection and datum conversion
    /// as required.
    /// <para/>
    /// Computing the transform involves the following steps:
    /// <list type="Bullet">
    /// <item>If the source coordinate is in a projected coordinate system,
    /// it is inverse-projected into a geographic coordinate system
    /// based on the source datum</item>
    /// <item>If the source and target <see cref="Datum"/>s are different,
    /// the source geographic coordinate is converted 
    /// from the source to the target datum
    /// as accurately as possible</item>
    /// <item>If the target coordinate system is a projected coordinate system, 
    /// the converted geographic coordinate is projected into a projected coordinate.</item>
    /// </list>
    /// Symbolically this can be presented as:
    /// <pre>
    /// [ SrcProjCRS {InverseProjection} ] SrcGeoCRS [ {Datum Conversion} ] TgtGeoCRS [ {Projection} TgtProjCRS ]
    /// </pre>
    /// <tt>BasicCoordinateTransform</tt> objects are stateful, 
    /// and thus are not thread-safe.
    /// However, they may be reused any number of times within a single thread.
    /// <para/>
    /// Information about the transformation procedure is pre-computed
    /// and cached in this object for efficient computation.
    /// </summary>
    /// <author>Martin Davis</author>
    /// <seealso cref="CoordinateTransformFactory"/>
    public class BasicCoordinateTransform : ICoordinateTransform
    {
        private readonly CoordinateReferenceSystem _sourceCRS;
        private readonly CoordinateReferenceSystem _targetCRS;

        // precomputed information
        private readonly Boolean _doInverseProjection = true;
        private readonly Boolean _doForwardProjection = true;
        private readonly Boolean _doDatumTransform;
        private readonly Boolean _transformViaGeocentric;
        private readonly GeocentricConverter _sourceGeoConv;
        private readonly GeocentricConverter _targetGeoConv;

        ///<summary>
        /// Creates a transformation from a source <see cref="CoordinateReferenceSystem"/> to a target one.
        ///</summary>
        ///<param name="sourceCRS">the source CRS to transform from</param>
        ///<param name="targetCRS">the target CRS to transform to</param>
        public BasicCoordinateTransform(CoordinateReferenceSystem sourceCRS,
          CoordinateReferenceSystem targetCRS)
        {
            if (sourceCRS == null)
                throw new ArgumentNullException("sourceCRS");
            if (targetCRS == null)
                throw new ArgumentNullException("targetCRS");
            
            _sourceCRS = sourceCRS;
            _targetCRS = targetCRS;

            // compute strategy for transformation at initialization time, to make transformation more efficient
            // this may include precomputing sets of parameters

            _doInverseProjection = (sourceCRS != CoordinateReferenceSystem.CS_GEO);
            _doForwardProjection = (targetCRS != CoordinateReferenceSystem.CS_GEO);
            _doDatumTransform = _doInverseProjection && _doForwardProjection
              && !sourceCRS.Datum.Equals(targetCRS.Datum);

            if (!_doDatumTransform) 
                return;

            var isEllipsoidEqual = sourceCRS.Datum.Ellipsoid.Equals(targetCRS.Datum.Ellipsoid);
            if (!isEllipsoidEqual)
                _transformViaGeocentric = true;
            if (sourceCRS.Datum.HasTransformToWGS84 || 
                targetCRS.Datum.HasTransformToWGS84)
                _transformViaGeocentric = true;

            if (!_transformViaGeocentric) 
                return;

            _sourceGeoConv = new GeocentricConverter(sourceCRS.Datum.Ellipsoid);
            _targetGeoConv = new GeocentricConverter(targetCRS.Datum.Ellipsoid);
        }

        public CoordinateReferenceSystem SourceCRS
        {
            get { return _sourceCRS; }
        }

        public CoordinateReferenceSystem TargetCRS
        {
            get { return _targetCRS; }
        }

        /// <summary>
        /// Tranforms a coordinate from the source <see cref="CoordinateReferenceSystem"/> to the target one.
        /// </summary>
        /// <param name="src">the input coordinate to be transformed</param>
        /// <param name="tgt">the transformed coordinate</param>
        /// <returns>the target coordinate which was passed in</returns>
        /// <exception cref="Proj4NetException">if a computation error is encountered</exception>
        public Coordinate Transform(Coordinate src, Coordinate tgt)
        {
            // NOTE: this method may be called many times, so needs to be as efficient as possible
            Coordinate geoCoord = new ProjCoordinate(0, 0);

            if (_doInverseProjection)
            {
                // inverse project to geographic
                _sourceCRS.Projection.InverseProjectRadians(src, geoCoord);
            }
            else
            {
                geoCoord.CoordinateValue = src;
            }

            //Adjust source prime meridian if specified other than Greenwich
            var primeMeridian = _sourceCRS.Projection.PrimeMeridian;
            if (primeMeridian.Name != NamedMeridian.Greenwich)
                primeMeridian.InverseAdjust(geoCoord);

            if (_doDatumTransform)
            {
                DatumTransform(geoCoord);
            }

            //Adjust target prime meridian if specified other than Greenwich
            primeMeridian = _targetCRS.Projection.PrimeMeridian;
            if (primeMeridian.Name != NamedMeridian.Greenwich)
                primeMeridian.Adjust(geoCoord);

            if (_doForwardProjection)
            {
                // project from geographic to planar
                _targetCRS.Projection.ProjectRadians(geoCoord, tgt);
            }
            else
            {
                tgt.CoordinateValue = geoCoord;
            }

            return tgt;
        }

        /// <summary>
        /// Converts from source to target datum
        /// </summary>
        /// <param name="pt">the point containing the input and output values</param>
        private void DatumTransform(Coordinate pt)
        {
            /* -------------------------------------------------------------------- */
            /*      Short cut if the datums are identical.                          */
            /* -------------------------------------------------------------------- */
            if (_sourceCRS.Datum.Equals(_targetCRS.Datum))
                return;

            /* -------------------------------------------------------------------- */
            /*      Apply grid shift if required                                    */ 
            /* -------------------------------------------------------------------- */
            if (_sourceCRS.Datum.TransformType == Datum.Datum.DatumTransformType.GridShift)
            {
                _sourceCRS.Datum.ApplyGridShift(pt, false);
            }

            /* ==================================================================== */
            /*      Do we need to go through geocentric coordinates?                */
            /* ==================================================================== */
            if (_transformViaGeocentric)
            {
                /* -------------------------------------------------------------------- */
                /*      Convert to geocentric coordinates.                              */
                /* -------------------------------------------------------------------- */
                _sourceGeoConv.ConvertGeodeticToGeocentric(pt);

                /* -------------------------------------------------------------------- */
                /*      Convert between datums.                                         */
                /* -------------------------------------------------------------------- */
                if (_sourceCRS.Datum.HasTransformToWGS84)
                {
                    _sourceCRS.Datum.TransformFromGeocentricToWgs84(pt);
                }
                if (_targetCRS.Datum.HasTransformToWGS84)
                {
                    _targetCRS.Datum.TransformToGeocentricFromWgs84(pt);
                }

                /* -------------------------------------------------------------------- */
                /*      Convert back to geodetic coordinates.                           */
                /* -------------------------------------------------------------------- */
                _targetGeoConv.ConvertGeocentricToGeodetic(pt);
            }

            /* -------------------------------------------------------------------- */
            /*      Apply grid shift if required                                    */
            /* -------------------------------------------------------------------- */
            if (_sourceCRS.Datum.TransformType == Datum.Datum.DatumTransformType.GridShift)
            {
                _sourceCRS.Datum.ApplyGridShift(pt, true);
            }

        }

    }
}
