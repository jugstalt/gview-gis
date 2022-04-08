using gView.Framework.IO;
using System;

namespace gView.Framework.Network
{
    public interface IGraphWeight : IPersistable
    {
        string Name { get; }
        Guid Guid { get; }
        GraphWeightDataType DataType { get; }

        GraphWeightFeatureClasses FeatureClasses { get; }
    }
}
