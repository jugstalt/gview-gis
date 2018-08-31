using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using System.Threading;
using gView.Framework.system;
using gView.Framework.Geometry;
using gView.Framework.Carto;

namespace gView.Framework.Data
{
    public class FeatureCache
    {
        static private Dictionary<Guid, CachedFeatureCollection> _collections = new Dictionary<Guid, CachedFeatureCollection>();
        static private object lockThis = new object();

        static public ICachedFeatureCollection CreateCachedFeatureCollection(Guid guid, IQueryFilter filter)
        {
            return CreateCachedFeatureCollection(guid, filter, -1);
        }
        static public ICachedFeatureCollection CreateCachedFeatureCollection(Guid guid, IQueryFilter filter, int LifeTime)
        {
            lock (lockThis)
            {
                CachedFeatureCollection collection;
                if (_collections.TryGetValue(guid, out collection))
                {
                    return null;
                }
                else
                {
                    collection = new CachedFeatureCollection(guid, filter);
                    _collections.Add(guid, collection);
                    return collection;
                }
            }
        }

        static internal ICachedFeatureCollection GetCachedFeatureCollection(Guid guid)
        {
            CachedFeatureCollection collection;
            if (_collections.TryGetValue(guid, out collection))
            {
                return collection;
            }
            return null;
        }

        static public void RemoveFeatureCollection(Guid guid)
        {
            _collections.Remove(guid);
        }
        static public void RemoveFeatureCollection(ICachedFeatureCollection collection)
        {
            Guid guid=new Guid();
            bool found = false;
            foreach (Guid g in _collections.Keys)
            {
                if (_collections[g] == collection)
                {
                    guid = g;
                    found = true;
                }
            }
            if (found) RemoveFeatureCollection(guid);
        }
        static public ICachedFeatureCollection GetUsableFeatureCollection(Guid guid, IQueryFilter filter)
        {
            CachedFeatureCollection collection = GetCachedFeatureCollection(guid) as CachedFeatureCollection;
            if (collection == null || !collection.UsableWith(filter)) return null;

            if (collection.Released)
            {
                return collection;
            }
            else
            {
                //
                // Wait for release!!! (Hope there is no deadlocking)
                //

                if (Thread.CurrentThread != collection._myThread)
                {
                    ManualResetEvent resetEvent = new ManualResetEvent(false);
                    collection._resetEvents.Add(resetEvent);
                    resetEvent.WaitOne(10000, false);
                    if (collection.Released) return collection;
                }
                else
                {
                    throw new Exception("Can't use unreleased cached featurecollection\nWaiting thread is identical with work thread!\nDeadlocking situation!");
                }
            }
            return null;
        }
        static public void ReleaseCachedFeatureCollection(ICachedFeatureCollection cachedFeatureColledion)
        {
            CachedFeatureCollection collection = cachedFeatureColledion as CachedFeatureCollection;
            if (collection != null)
                collection.Released = true;
        }

        static public bool UseCachingForDataset(Guid datasetGUID)
        {
            // für WMS/WFS und ArcXML Feature Queries
            if (datasetGUID == new Guid("538F0731-31FE-493a-B063-10A2D37D6E6D") ||
                datasetGUID == new Guid("3B26682C-BF6E-4fe8-BE80-762260ABA581"))
                return true;

            return false;
        }

        #region Classes
        class CachedFeatureCollection : ICachedFeatureCollection
        {
            private Guid _guid;
            private IQueryFilter _filter;
            
            private Dictionary<long, List<IFeature>> _features;
            private BinarySearchTree2 _tree = null;

            public CachedFeatureCollection(Guid guid, IQueryFilter filter)
            {
                _guid = guid;
                _filter = filter;
                _myThread = Thread.CurrentThread;

                if (_filter is ISpatialFilter &&
                    ((ISpatialFilter)_filter).Geometry != null)
                {
                    _tree = new BinarySearchTree2(((ISpatialFilter)_filter).Geometry.Envelope,
                        20, 200, 0.55, null);
                }

                _features = new Dictionary<long, List<IFeature>>();
                _features.Add((long)0, new List<IFeature>());
            }

            private bool _released = false;
            internal List<ManualResetEvent> _resetEvents = new List<ManualResetEvent>();
            internal Thread _myThread;
            public bool Released
            {
                get { return _released; }
                set
                {
                    _released = true;

                    foreach (ManualResetEvent resetEvent in _resetEvents)
                    {
                        resetEvent.Set();
                    }
                }
            }

            #region ICachedFeatureCollection Member

            public void AddFeature(IFeature feature)
            {
                if(feature==null) return;

                long NID = (feature.Shape != null && _tree != null) ?
                    _tree.InsertSINode(feature.Shape.Envelope) : 0;
                if(_tree!=null) _tree.AddNodeNumber(NID);

                List<IFeature> features;
                if (!_features.TryGetValue(NID, out features))
                {
                    features = new List<IFeature>();
                    _features.Add(NID, features);
                }
                features.Add(feature);
            }

            public Guid CollectionGUID
            {
                get { return _guid; }
            }

            public IQueryFilter QueryFilter
            {
                get { return _filter.Clone() as IQueryFilter; }
            }

            public bool UsableWith(IQueryFilter filter)
            {
                if (_filter == null || filter == null) return false;

                // Type überprüfen
                if (_filter.GetType() != filter.GetType()) return false;

                #region WhereClause
                if (_filter.WhereClause != filter.WhereClause)
                    return false;
                #endregion

                #region Subfields
                if (_filter.SubFields != "*")
                {
                    if (filter.SubFields == "*") return false;

                    string[] subFields_o = _filter.SubFields.Split(' ');
                    string[] subFields_n = filter.SubFields.Split(' ');

                    foreach (string fn in subFields_n)
                    {
                        string shortNameN = Field.shortName(fn);
                        bool found=false;
                        foreach (string fo in subFields_o)
                        {
                            if (Field.shortName(fo) == shortNameN)
                            {
                                found = true;
                                break;
                            }
                        }
                        if (!found) return false;
                    }
                }
                #endregion

                #region Geometry Envelope
                if (_filter is ISpatialFilter && filter is ISpatialFilter)
                {
                    IGeometry geomN = ((ISpatialFilter)filter).Geometry;
                    IGeometry geomO = ((ISpatialFilter)_filter).Geometry;

                    if (geomN == null || geomO == null) return false;

                    if (((ISpatialFilter)_filter).SpatialRelation == spatialRelation.SpatialRelationMapEnvelopeIntersects &&
                        ((ISpatialFilter)filter).SpatialRelation == spatialRelation.SpatialRelationMapEnvelopeIntersects)
                    {
                        return geomO.Envelope.Contains(geomN.Envelope);
                    }
                    else
                    {
                        return SpatialAlgorithms.Algorithm.Contains(geomO, geomN);
                    }
                }
                #endregion

                return true;
            }

            public IFeatureCursor FeatureCursor()
            {
                return new CachedFeatureCursor(GetFeatures(), null);
            }

            public IFeatureCursor FeatureCursor(IQueryFilter filter)
            {
                return new CachedFeatureCursor(GetFeatures(filter), filter);
            }

            #endregion

            private List<IFeature> GetFeatures()
            {
                List<IFeature> features = new List<IFeature>();

                foreach (long nid in _features.Keys)
                    features.AddRange(_features[nid]);

                return features;
            }
            private List<IFeature> GetFeatures(IQueryFilter filter)
            {
                if (_tree != null)
                {
                    List<IFeature> features = new List<IFeature>();

                    if (filter is ISpatialFilter &&
                        ((ISpatialFilter)filter).Geometry != null)
                    {
                        List<long> nids = _tree.CollectNIDs(((ISpatialFilter)filter).Geometry.Envelope);
                        foreach (long nid in nids)
                        {
                            features.AddRange(_features[nid]);
                        }
                    }
                    else
                    {
                        GetFeatures();
                    }
                    return features;
                }
                else
                {
                    return _features[(long)0];
                }
            }
        }

        class CachedFeatureCursor : IFeatureCursor
        {
            private List<IFeature> _features;
            private int _pos=0;
            private IQueryFilter _filter;

            public CachedFeatureCursor(List<IFeature> features, IQueryFilter filter)
            {
                _features = features;
                _filter = filter;
            }
            #region IFeatureCursor Member

            public IFeature NextFeature
            {
                get 
                {
                    while (true)
                    {
                        if (_features == null || _features.Count <= _pos) return null;

                        IFeature feature = _features[_pos++];
                        if (feature == null) return null;

                        if (_filter is ISpatialFilter)
                        {
                            if (!gView.Framework.Geometry.SpatialRelation.Check(_filter as ISpatialFilter, feature.Shape))
                                continue;
                        }

                        return feature;
                    }
                }
            }

            #endregion

            #region IDisposable Member

            public void Dispose()
            {

            }

            #endregion
        }
        #endregion
    }

    public abstract class CacheableFeatureClass : ISelectionCache
    {
        private Guid _selectionGUID = Guid.NewGuid();
        private Guid _mapEnvelopeGUID = Guid.NewGuid();
        private Guid _getFeatureGUID = Guid.NewGuid();

        private IFeatureCursor GetFeatures(Guid guid, IQueryFilter filter)
        {
            ICachedFeatureCollection collection = FeatureCache.GetUsableFeatureCollection(guid, filter);
            if (collection != null)
            {
                return collection.FeatureCursor(filter);
            }
            else
            {
                FeatureCache.RemoveFeatureCollection(guid);

                collection = FeatureCache.CreateCachedFeatureCollection(guid, filter);
                return new CachingFeatureCursor(collection, this.FeatureCursor(filter));
            }
        }

        #region FeatureClass Members
        virtual public IFeatureCursor GetFeatures(IQueryFilter filter)
        {
            if (filter is ISpatialFilter &&
                ((ISpatialFilter)filter).SpatialRelation == spatialRelation.SpatialRelationMapEnvelopeIntersects)
            {
                return GetFeatures(_mapEnvelopeGUID, filter);
            }
            else
            {
                return GetFeatures(_getFeatureGUID, filter);
            }
        }
        virtual public ICursor Search(IQueryFilter filter)
        {
            return GetFeatures(_getFeatureGUID, filter);
        }
        virtual public IFields Fields
        {
            get
            {
                return new Fields();
            }
        }
        virtual public ISelectionSet Select(IQueryFilter filter)
        {
            FeatureCache.RemoveFeatureCollection(_selectionGUID);
            if (this.IDFieldName != String.Empty && this.FindField(this.IDFieldName) != null)
            {
                filter.SubFields = this.IDFieldName;

                filter.AddField(this.ShapeFieldName);
                filter.AddField(this.IDFieldName);
                using (IFeatureCursor cursor = this.GetFeatures(_selectionGUID, filter))
                {
                    if (cursor != null)
                    {
                        IFeature feat;

                        IDSelectionSet selSet = new IDSelectionSet();
                        while ((feat = cursor.NextFeature) != null)
                        {
                            selSet.AddID(feat.OID);
                        }
                        return selSet;
                    }
                }
            }
            else
            {
                int count = 0;
                using (IFeatureCursor cursor = this.GetFeatures(_selectionGUID, filter))
                {
                    if (cursor == null) return null;

                    IFeature feature;
                    while ((feature = cursor.NextFeature) != null)
                        count++;
                }
                return new QueryFilteredSelectionSet(filter, count);
            }
            return null;
        }

        public abstract string IDFieldName { get; }
        public abstract string ShapeFieldName { get; }
        public abstract IField FindField(string fieldname);
        #endregion

        protected abstract IFeatureCursor FeatureCursor(IQueryFilter filter);

        #region ISelectionCache Member

        public IFeatureCursor GetSelectedFeatures()
        {
            return GetSelectedFeatures(null);
        }

        public IFeatureCursor GetSelectedFeatures(IDisplay display)
        {
            ICachedFeatureCollection collection = FeatureCache.GetCachedFeatureCollection(this._selectionGUID);
            if (collection == null) return null;

            if (display != null)
            {
                SpatialFilter filter = new SpatialFilter();
                filter.SpatialRelation = spatialRelation.SpatialRelationMapEnvelopeIntersects;
                filter.Geometry = display.Envelope;
                filter.FilterSpatialReference = display.SpatialReference;

                return collection.FeatureCursor(filter);
            }
            else
            {
                return collection.FeatureCursor();
            }
        }

        #endregion
    }

    internal class CachingFeatureCursor : IFeatureCursor
    {
        private IFeatureCursor _cursor = null;
        private ICachedFeatureCollection _cfCollection = null;
        private bool _released = false;

        public CachingFeatureCursor(ICachedFeatureCollection cfCollection, IFeatureCursor cursor)
        {
            _cursor = cursor;
            _cfCollection = cfCollection;
        }

        #region IFeatureCursor Member

        public IFeature NextFeature
        {
            get
            {
                if (_cursor == null) return null;

                IFeature feature = _cursor.NextFeature;
                if (_cfCollection != null)
                {
                    if (feature != null)
                    {
                        _cfCollection.AddFeature(feature);
                    }
                    else
                    {
                        FeatureCache.ReleaseCachedFeatureCollection(_cfCollection);
                        _released = true;
                    }
                }
                return feature;
            }
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {
            if (_cursor != null)
            {
                _cursor.Dispose();
                _cursor = null;

                if (!_released)
                    FeatureCache.RemoveFeatureCollection(_cfCollection);
            }
        }

        #endregion
    }
}
