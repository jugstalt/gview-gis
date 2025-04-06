#nullable enable

using gView.Framework.Core.Common;
using gView.Framework.Core.IO;

namespace gView.Framework.Core.Geometry;

public interface IDatumTransformation : IPersistable, IClone<IDatumTransformation>
{
    public IGeodeticDatum FromDatum { get; }
    public IGeodeticDatum ToDatum { get; }

    public IGeodeticDatum TransformationDatum { get; }
}
