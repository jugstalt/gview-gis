using gView.Framework.Core.IO;
using gView.Framework.Core.system;

namespace gView.Framework.Core.Network
{
    public interface IGraphWeightFeatureClass : IPersistable
    {
        int FcId { get; }
        string FieldName { get; set; }
        ISimpleNumberCalculation SimpleNumberCalculation { get; set; }
    }
}
