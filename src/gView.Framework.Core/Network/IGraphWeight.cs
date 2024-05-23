using gView.Framework.Core.IO;
using System;

namespace gView.Framework.Core.Network
{
    public interface IGraphWeight : IPersistable
    {
        string Name { get; set; }
        Guid Guid { get; }
        GraphWeightDataType DataType { get; set; }

        GraphWeightFeatureClasses FeatureClasses { get; }
    }
}
