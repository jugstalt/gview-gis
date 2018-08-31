using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using gView.Framework.Data;
using gView.Framework.Network;

namespace gView.Framework.Network.Algorthm
{
    public class GraphTable
    {
        private IGraphTableAdapter _adapter;
        private DataTable _table;

        public GraphTable(IGraphTableAdapter adapter)
        {
            _adapter = adapter;

            _table = new DataTable();
            _table.Columns.Add(new DataColumn("N1", typeof(int)));
            _table.Columns.Add(new DataColumn("N2", typeof(int)));
            _table.Columns.Add(new DataColumn("EID", typeof(int)));
            _table.Columns.Add(new DataColumn("LENGTH", typeof(double)));
            _table.Columns.Add(new DataColumn("GEOLEN", typeof(double)));

            _table.Columns.Add(new DataColumn("COST", typeof(double)));
            _table.Columns.Add(new DataColumn("PN", typeof(int)));
        }

        public GraphTableRows QueryN1(int n1)
        {
            if (_adapter != null)
                return _adapter.QueryN1(n1);
            return null;
        }

        public IGraphTableRow QueryN1ToN2(int n1, int n2)
        {
            if (_adapter != null)
                return _adapter.QueryN1ToN2(n1, n2);
            return null;
        }
        public IGraphEdge QueryEdge(int eid)
        {
            if (_adapter != null)
                return _adapter.QueryEdge(eid);
            return null;
        }

        public double QueryEdgeWeight(Guid weightGuid, int eid)
        {
            if (_adapter != null)
                return _adapter.QueryEdgeWeight(weightGuid, eid);
            return 0.0;
        }

        public bool SwitchState(int nid)
        {
            if (_adapter != null)
                return _adapter.SwitchState(nid);

            return false;
        }

        public int GetNodeFcid(int nid)
        {
            if (_adapter != null)
                return _adapter.GetNodeFcid(nid);

            return -1;
        }

        public NetworkNodeType GetNodeType(int nid)
        {
            if (_adapter != null)
                return _adapter.GetNodeType(nid);

            return NetworkNodeType.Unknown;
        }

        public Features QueryNodeEdgeFeatures(int n1)
        {
            if (_adapter == null)
                return new Features();
            return _adapter.QueryNodeEdgeFeatures(n1);
        }
        public Features QueryNodeFeatures(int n1)
        {
            if (_adapter == null)
                return new Features();
            return _adapter.QueryNodeFeatures(n1);
        }
    }
}
