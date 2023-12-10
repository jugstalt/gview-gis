using gView.Framework.Core.Data;
using gView.Framework.Core.Common;
using System;
using System.Collections.Generic;

namespace gView.Framework.Data
{
    public class IDSelectionSetTemplate<T> : IDSetTemplate<T>
    {
        protected List<T> _IDs;

        public IDSelectionSetTemplate()
            : base()
        {
            _IDs = new List<T>();
        }
        public void Dispose()
        {
            Clear();
        }

        #region IIDSelectionSet Member

        public void Clear()
        {
            _IDs.Clear();
        }

        public void AddID(T ID)
        {
            if (_IDs.IndexOf(ID) != -1)
            {
                return;
            }

            _IDs.Add(ID);
        }

        public void AddIDs(List<T> IDs)
        {
            if (IDs == null)
            {
                return;
            }

            foreach (T id in IDs)
            {
                this.AddID(id);
            }
        }

        public void RemoveID(T ID)
        {
            if (_IDs.IndexOf(ID) == -1)
            {
                return;
            }

            _IDs.Remove(ID);
        }

        public void RemoveIDs(List<T> IDs)
        {
            if (IDs == null)
            {
                return;
            }

            foreach (T id in IDs)
            {
                this.RemoveID(id);
            }
        }

        override public List<T> IDs
        {
            get
            {
                return ListOperations<T>.Clone(_IDs); // _IDs.Clone();
            }
        }

        public void Combine(ISelectionSet selSet, CombinationMethod method)
        {
            if (selSet == null)
            {
                return;
            }

            if (!(selSet is IIDSelectionSet))
            {
                throw new Exception("Can't combine selectionset that are not implement the IIDSectionSet type..");
            }
            if (!(selSet is IDSetTemplate<T>))
            {
                throw new Exception("Can't combine selectionset with different templates...");
            }

            IIDSelectionSet idSelSet = selSet as IIDSelectionSet;

            switch (method)
            {
                case CombinationMethod.Union:
                    foreach (T id in ((IDSetTemplate<T>)idSelSet).IDs)
                    {
                        if (_IDs.IndexOf(id) == -1)
                        {
                            _IDs.Add(id);
                        }
                    }
                    break;
                case CombinationMethod.Difference:  // Remove from Current Selection
                    foreach (T id in ((IDSetTemplate<T>)idSelSet).IDs)
                    {
                        if (_IDs.IndexOf(id) != -1)
                        {
                            _IDs.Remove(id);
                        }
                    }
                    break;
                case CombinationMethod.Intersection:  // Select from Current Selection (nur die gleichen)
                    List<T> ids = ((IDSetTemplate<T>)idSelSet).IDs;
                    foreach (T id in ListOperations<T>.Clone(_IDs))
                    {
                        if (ids.IndexOf(id) == -1)
                        {
                            _IDs.Remove(id);
                        }
                    }
                    break;
                case CombinationMethod.SymDifference:
                    foreach (T id in ((IDSetTemplate<T>)idSelSet).IDs)
                    {
                        if (_IDs.IndexOf(id) != -1)
                        {
                            _IDs.Remove(id);
                        }
                        else
                        {
                            _IDs.Add(id);
                        }
                    }
                    break;
            }
        }
        #endregion

        #region ISelectionSet Member


        public int Count
        {
            get
            {
                if (_IDs == null)
                {
                    return 0;
                }

                return _IDs.Count;
            }
        }

        #endregion
    }
}
