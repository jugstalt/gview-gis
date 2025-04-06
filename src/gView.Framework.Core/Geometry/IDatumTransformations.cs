#nullable enable

using gView.Framework.Core.Common;
using gView.Framework.Core.IO;

namespace gView.Framework.Core.Geometry;

public interface IDatumTransformations : IPersistable, IClone<IDatumTransformations>
{
    IDatumTransformation[]? Transformations { get; }
}