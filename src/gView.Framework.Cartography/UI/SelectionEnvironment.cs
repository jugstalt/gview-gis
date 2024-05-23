using gView.Framework.Core.Data;
using gView.Framework.Core.UI;
using System.Collections.Generic;

namespace gView.Framework.Cartography.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class SelectionEnvironment : ISelectionEnvironment
    {
        private List<IDatasetElement> _layers;

        public SelectionEnvironment()
        {
            _layers = new List<IDatasetElement>();
        }

        #region ISelectionEnvironment Member

        public void RemoveAll()
        {
            _layers.Clear();
        }

        public void AddToSelectableElements(IDatasetElement element)
        {
            if (_layers.IndexOf(element) != -1)
            {
                return;
            }

            if (!(element is IFeatureSelection))
            {
                return;
            }

            _layers.Add(element);
        }

        public List<IDatasetElement> SelectableElements
        {
            get
            {
                List<IDatasetElement> e = new List<IDatasetElement>();
                foreach (IDatasetElement element in _layers)
                {
                    e.Add(element);
                }
                return e;
            }
        }

        public void RemoveFromSelectableElements(IDatasetElement element)
        {
            if (_layers.IndexOf(element) == -1)
            {
                return;
            }

            _layers.Remove(element);
        }

        #endregion
    }
}
