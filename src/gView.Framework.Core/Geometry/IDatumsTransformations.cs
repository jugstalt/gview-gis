#nullable enable

using gView.Framework.Core.IO;

namespace gView.Framework.Core.Geometry;

public interface IDatumsTransformations
{
    IDatumsTransformation[]? Transformations { get; }
}