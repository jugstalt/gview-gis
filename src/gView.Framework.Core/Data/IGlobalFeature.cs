using gView.Framework.Core.Geometry;

namespace gView.Framework.Core.Data
{
    public interface IGlobalFeature : IGlobalRow
    {
        /// <summary>
        /// The Shape of the feature.
        /// </summary>
        IGeometry Shape { get; set; }
    }
}