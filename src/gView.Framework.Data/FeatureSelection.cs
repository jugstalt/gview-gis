using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using System;
using System.Threading.Tasks;

namespace gView.Framework.Data
{
    public class FeatureSelection : FeatureHighlighting, IFeatureSelection
    {
        ISelectionSet _selectionSet = new IDSelectionSet();

        public FeatureSelection() { }
        public FeatureSelection(IFeatureClass featureClass)
            : base(featureClass)
        {
        }
        public FeatureSelection(IFeatureLayer layer)
            : base(layer)
        {
        }

        #region IFeatureSelection Member

        public event FeatureSelectionChangedEvent FeatureSelectionChanged;
        public event BeforeClearSelectionEvent BeforeClearSelection;

        public ISelectionSet SelectionSet
        {
            get
            {
                return _selectionSet;
            }
            set
            {
                if (_selectionSet != null && _selectionSet is IDSelectionSet)
                {
                    ((IDSelectionSet)_selectionSet).Dispose();
                }

                _selectionSet = value;
            }
        }

        async public Task<bool> Select(IQueryFilter filter, CombinationMethod methode)
        {
            if (this.FeatureClass != null)
            {
                if (this.FilterQuery != null && !String.IsNullOrEmpty(this.FilterQuery.WhereClause))
                {
                    filter.WhereClause = String.IsNullOrEmpty(filter.WhereClause) ? this.FilterQuery.WhereClause : filter.WhereClause + " AND " + this.FilterQuery.WhereClause;
                }
                ISelectionSet selSet = await this.FeatureClass.Select(filter);

                if (methode == CombinationMethod.New)
                {
                    _selectionSet = selSet;
                }
                else
                {
                    if (_selectionSet == null)
                    {
                        if (selSet is SpatialIndexedIDSelectionSet)
                        {
                            _selectionSet = new SpatialIndexedIDSelectionSet(
                                ((SpatialIndexedIDSelectionSet)_selectionSet).Bounds);
                        }
                        else
                        {
                            _selectionSet = new IDSelectionSet();
                        }
                    }

                    _selectionSet.Combine(selSet, methode);
                }

                return true;
            }
            return false;
        }

        public void ClearSelection()
        {
            if (_selectionSet != null)
            {
                BeforeClearSelection?.Invoke(this);

                _selectionSet.Clear();
            }
        }

        public void FireSelectionChangedEvent()
        {
            FeatureSelectionChanged?.Invoke(this);
        }

        #endregion
    }
}
