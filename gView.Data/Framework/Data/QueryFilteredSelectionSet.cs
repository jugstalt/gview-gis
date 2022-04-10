using gView.Framework.Data.Cursors;
using gView.Framework.Data.Filters;
using System;
using System.Threading.Tasks;

namespace gView.Framework.Data
{
    public class QueryFilteredSelectionSet : IQueryFilteredSelectionSet
    {
        private IQueryFilter _filter = null;
        private int _count = 0;

        private QueryFilteredSelectionSet() { }

        async static public Task<QueryFilteredSelectionSet> CreateAsync(IFeatureClass fClass, IQueryFilter filter)
        {
            var set = new QueryFilteredSelectionSet();

            if (fClass == null)
            {
                return set;
            }

            set._filter = filter.Clone() as IQueryFilter;
            if (set._filter == null)
            {
                return set;
            }

            try
            {
                using (IFeatureCursor cursor = await fClass.GetFeatures(filter))
                {
                    if (cursor == null)
                    {
                        return set;
                    }

                    IFeature feature;
                    while ((feature = await cursor.NextFeature()) != null)
                    {
                        set._count++;
                    }
                }
            }
            catch
            {
                set.Clear();
            }

            return set;
        }

        public QueryFilteredSelectionSet(IQueryFilter filter, int count)
        {
            _filter = filter;
            _count = count;
        }
        #region IQueryFilteredSelectionSet Member

        public IQueryFilter QueryFilter
        {
            get { return _filter; }
        }

        #endregion

        #region ISelectionSet Member

        public void Clear()
        {
            _filter = null;
            _count = 0;
        }

        public int Count
        {
            get { return _count; }
        }

        public void Combine(ISelectionSet selSet, CombinationMethod method)
        {
            throw new Exception("A combination for featureselection is not allowed for this featuretype (no ID Field is defined...)");
        }

        #endregion
    }
}
