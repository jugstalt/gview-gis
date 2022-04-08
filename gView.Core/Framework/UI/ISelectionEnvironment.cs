using System.Collections.Generic;
using gView.Framework.Data;

namespace gView.Framework.UI
{
    public interface ISelectionEnvironment
    {
        List<IDatasetElement> SelectableElements { get; }

        void AddToSelectableElements(IDatasetElement element);
        void RemoveFromSelectableElements(IDatasetElement element);
        void RemoveAll();
    }

    
}