using gView.Framework.Core.Geometry;

namespace gView.Framework.Core.Data
{
    /// <summary>
    /// Provides access to members and properties that return information about a feature object.
    /// </summary>
    /// <remarks>
    /// <c>IFeature</c> implements <see cref="Framework.IRow"/> to provide access to the object ID and field information.
    /// </remarks>
    public interface IFeature : IRow
    {
        /// <summary>
        /// The Shape of the feature.
        /// </summary>
        IGeometry Shape { get; set; }
    }
}