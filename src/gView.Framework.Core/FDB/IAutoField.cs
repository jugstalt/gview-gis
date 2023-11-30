using gView.Framework.Core.Data;
using System.Threading.Tasks;

namespace gView.Framework.Core.FDB
{
    public interface IAutoField : IField
    {
        string AutoFieldName { get; }
        string AutoFieldDescription { get; }

        string AutoFieldPrimayName { get; }
        FieldType AutoFieldType { get; }

        Task<bool> OnInsert(IFeatureClass fc, IFeature feature);
        Task<bool> OnUpdate(IFeatureClass fc, IFeature feature);
    }
}
