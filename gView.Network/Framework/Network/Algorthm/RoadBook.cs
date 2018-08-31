using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Framework.Network;

namespace gView.Framework.Network.Algorthm
{
    public class RoadBook
    {
        private INetworkFeatureClass _nfc;

        public RoadBook(INetworkFeatureClass nfc)
        {
            _nfc = nfc;
        }

        public Items PathDescription(Dijkstra dijkstra, int targetNode)
        {
            Items items = new Items();

            Dijkstra.Nodes nodes = dijkstra.DijkstraPathNodes(targetNode);
            if (nodes == null)
                return items;

            foreach (Dijkstra.Node node in nodes)
            {
                IFeature feature = _nfc.GetEdgeFeatureAttributes(node.EId, new string[] { "*" });
                if (feature == null)
                    continue;

                items.Add(new Item(node.Id, node.EId, node.Dist, feature));
            }

            return items;
        }

        #region Classes
        public class Item
        {
            private int _nId, _eid;
            private double _dist = 0.0;
            private IFeature _feature;

            public Item(int nid, int eid)
            {
                _nId = nid;
                _eid = eid;
            }
            public Item(int nid, int eid, double dist)
                : this(nid, eid)
            {
                _dist = dist;
            }
            public Item(int nid, int eid, double dist, IFeature feature)
                : this(nid, eid, dist)
            {
                _feature = feature;
            }

            #region Properties
            public int NId { get { return _nId; } }
            public int EId { get { return _eid; } }
            public double Distance { get { return _dist; } }
            public IFeature Feature { get { return _feature; } }
            #endregion
        }
        public class Items : List<Item>
        {
            public string[] AttributeNames
            {
                get
                {
                    List<string> attributeNames = new List<string>();
                    foreach (Item item in this)
                    {
                        IFeature feature = item.Feature;
                        if (feature == null)
                            continue;

                        foreach (FieldValue fv in feature.Fields)
                        {
                            if (attributeNames.Contains(fv.Name))
                                continue;
                            attributeNames.Add(fv.Name);
                        }
                    }
                    return attributeNames.ToArray();
                }
            }
        }
        #endregion
    }
}