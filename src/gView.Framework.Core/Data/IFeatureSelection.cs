using gView.Framework.Core.Data.Filters;
using System.Threading.Tasks;

namespace gView.Framework.Core.Data
{
    public interface IFeatureSelection
    {
        event FeatureSelectionChangedEvent FeatureSelectionChanged;
        event BeforeClearSelectionEvent BeforeClearSelection;

        ISelectionSet SelectionSet { get; set; }
        Task<bool> Select(IQueryFilter filter, CombinationMethod methode);
        void ClearSelection();
        void FireSelectionChangedEvent();
    }
}