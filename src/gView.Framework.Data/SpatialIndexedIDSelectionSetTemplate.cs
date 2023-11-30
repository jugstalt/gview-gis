using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Common;
using System;
using System.Collections.Generic;

namespace gView.Framework.Data
{
    public class SpatialIndexedIDSelectionSetTemplate<T> : IDSetTemplate<T>
    {
        private Dictionary<long, IndexList<T>> _NIDs;
        private BinarySearchTree2 _tree;

        public SpatialIndexedIDSelectionSetTemplate(IEnvelope bounds)
        {
            _tree = new BinarySearchTree2(bounds, 50, 200, 0.55, null);
            _NIDs = new Dictionary<long, IndexList<T>>();
            _NIDs.Add((long)0, new IndexList<T>());
        }

        #region ISpatialIndexedSelectionSet Members

        public void Clear()
        {
            foreach (List<T> list in _NIDs.Values)
            {
                list.Clear();
            }
            _NIDs.Clear();
        }
        public void AddID(T ID, IGeometry geometry)
        {
            IndexList<T> ids;

            long NID = (geometry != null) ? _tree.InsertSINode(geometry.Envelope) : 0;
            _tree.AddNodeNumber(NID);

            if (!_NIDs.TryGetValue(NID, out ids))
            {
                ids = new IndexList<T>();
                _NIDs.Add(NID, ids);
            }
            if (ids.IndexOf(ID) == -1)
            {
                ids.Add(ID);
            }
        }

        public long NID(T id)
        {
            foreach (long nid in _NIDs.Keys)
            {
                List<T> ids = (List<T>)_NIDs[nid];
                if (ids == null)
                {
                    continue;
                }

                if (ids.IndexOf(id) != -1)
                {
                    return nid;
                }
            }
            return 0;
        }

        public List<T> IDsInEnvelope(IEnvelope envelope)
        {
            IndexList<T> ret = new IndexList<T>();
            if (_tree != null && envelope != null)
            {
                foreach (long NID in _tree.CollectNIDs(envelope))
                {
                    IndexList<T> list;

                    if (_NIDs.TryGetValue(NID, out list))
                    {
                        ret.AddRange(list);
                    }
                }
            }
            else
            {
                foreach (long NID in _NIDs.Keys)
                {
                    ret.AddRange((List<T>)_NIDs[NID]);
                }
            }
            return ret;
        }

        #endregion

        public IEnvelope Bounds
        {
            get { return _tree.Bounds; }
        }
        private IEnvelope NodeEnvelope(T id)
        {
            foreach (long NID in _NIDs.Keys)
            {
                if (((List<T>)_NIDs[NID]).IndexOf(id) >= 0)
                {
                    return _tree[NID];
                }
            }
            return null;
        }
        #region ISelectionSet Members

        public void AddID(T ID)
        {
            this.AddID(ID, null);
        }

        public void AddIDs(List<T> IDs)
        {
            foreach (T ID in IDs)
            {
                this.AddID(ID, null);
            }
        }

        public void RemoveID(T ID)
        {
            foreach (List<T> list in _NIDs.Values)
            {
                if (list.IndexOf(ID) != -1)
                {
                    list.Remove(ID);
                }
            }
        }

        public void RemoveIDs(List<T> IDs)
        {
            foreach (T ID in IDs)
            {
                RemoveID(ID);
            }
        }

        override public List<T> IDs
        {
            get { return this.IDsInEnvelope(null); }
        }

        public void Combine(ISelectionSet selSet, CombinationMethod method)
        {
            if (!(selSet is IIDSelectionSet))
            {
                throw new Exception("Can't combine selectionset that are not implement the IIDSectionSet type..");
            }
            if (!(selSet is IDSetTemplate<T>))
            {
                throw new Exception("Can't combine selectionset with different templates...");
            }

            IIDSelectionSet idSelSet = selSet as IIDSelectionSet;
            SpatialIndexedIDSelectionSetTemplate<T> spSelSet = null;
            if (selSet is SpatialIndexedIDSelectionSetTemplate<T>)
            {
                spSelSet = (SpatialIndexedIDSelectionSetTemplate<T>)selSet;
            }

            List<T> _IDs = this.IDs;
            switch (method)
            {
                case CombinationMethod.Union:
                    foreach (T id in ((IDSetTemplate<T>)idSelSet).IDs)
                    {
                        if (_IDs.IndexOf(id) == -1)
                        {
                            if (spSelSet != null)
                            {
                                this.AddID(id, spSelSet.NodeEnvelope(id) /*spSelSet.NID(id)*/);
                            }
                            else
                            {
                                this.AddID(id);  // to NodeID = 0
                            }
                        }
                    }
                    break;
                case CombinationMethod.Difference:  // Remove from Current Selection
                    foreach (T id in ((IDSetTemplate<T>)idSelSet).IDs)
                    {
                        if (_IDs.IndexOf(id) != -1)
                        {
                            this.RemoveID(id);
                        }
                    }
                    break;
                case CombinationMethod.Intersection:  // Select from Current Selection (nur die gleichen)
                    List<T> ids = ((IDSetTemplate<T>)idSelSet).IDs;
                    foreach (T id in _IDs)
                    {
                        if (ids.IndexOf(id) == -1)
                        {
                            this.RemoveID(id);
                        }
                    }
                    break;
                case CombinationMethod.SymDifference:
                    foreach (T id in ((IDSetTemplate<T>)idSelSet).IDs)
                    {
                        if (_IDs.IndexOf(id) != -1)
                        {
                            this.RemoveID(id);
                        }
                        else
                        {
                            if (spSelSet != null)
                            {
                                this.AddID(id, spSelSet.NodeEnvelope(id));
                            }
                            else
                            {
                                this.AddID(id);
                            }
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
                if (_NIDs == null)
                {
                    return 0;
                }

                int count = 0;
                foreach (long nid in _NIDs.Keys)
                {
                    if (_NIDs[nid] == null)
                    {
                        continue;
                    }

                    count += _NIDs[nid].Count;
                }
                return count;
            }
        }

        #endregion
    }
}
