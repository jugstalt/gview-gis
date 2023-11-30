using gView.Framework.Core.Data;
using System.Collections.Generic;

namespace gView.Framework.Core.UI
{
    public interface ISelectionEnvironment
    {
        List<IDatasetElement> SelectableElements { get; }

        void AddToSelectableElements(IDatasetElement element);
        void RemoveFromSelectableElements(IDatasetElement element);
        void RemoveAll();
    }


}