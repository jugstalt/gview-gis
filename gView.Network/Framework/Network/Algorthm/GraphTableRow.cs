using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using gView.Framework.Network;
using gView.Framework.system;

namespace gView.Framework.Network.Algorthm
{
    public class GraphTableRow : IGraphTableRow
    {
        private int _N1, _N2, _EID;
        private double _LENGTH, _GEOLEN;
        //private DataRow _datarow = null;

        public GraphTableRow(int n1, int n2, int eid, double length, double geolen)
        {
            _N1 = n1;
            _N2 = n2;
            _EID = eid;
            _LENGTH = length;
            _GEOLEN = geolen;
        }
        internal GraphTableRow(DataRow datarow)
        {
            if (datarow != null)
            {
                _N1 = Convert.ToInt32(datarow["N1"]);
                _N2 = Convert.ToInt32(datarow["N2"]);
                _EID = Convert.ToInt32(datarow["EID"]);
                _LENGTH = Convert.ToDouble(datarow["LENGTH"]);
                _GEOLEN = Convert.ToDouble(datarow["GEOLEN"]);
            }
        }

        #region Properties
        public int N1 { get { return _N1; } }
        public int N2 { get { return _N2; } }
        public int EID { get { return _EID; } }
        public double LENGTH { get { return _LENGTH; } }
        public double GEOLEN { get { return _GEOLEN; } }

        //public double COST
        //{
        //    get
        //    {
        //        if (_datarow != null)
        //            return (double)_datarow["COST"];
        //        throw new Exception("GraphTableRow.DataRow=NULL!");
        //    }
        //    set
        //    {
        //        if (_datarow != null)
        //            _datarow["COST"] = value;
        //    }
        //}
        //public int PN
        //{
        //    get
        //    {
        //        if (_datarow != null)
        //            return (int)_datarow["PN"];
        //        throw new Exception("GraphTableRow.DataRow=NULL!");
        //    }
        //    set
        //    {
        //        if (_datarow != null)
        //            _datarow["PN"] = value;
        //    }
        //}

        //internal DataRow DataRow
        //{
        //    set { _datarow = value; }
        //}
        #endregion
    }

    public class GraphEdge : IGraphEdge
    {
        private int _eid, _fcid, _oid, _n1, _n2;

        public GraphEdge(int eid, int fcid, int oid, int n1, int n2)
        {
            _eid = eid;
            _fcid = fcid;
            _oid = oid;
            _n1 = n1;
            _n2 = n2;
        }

        public int Eid { get { return _eid; } }
        public int FcId { get { return _fcid; } }
        public int Oid { get { return _oid; } }

        public int N1 { get { return _n1; } }
        public int N2 { get { return _n2; } }
    }

    public class Switch : ISwitch
    {
        private int _nodeId;
        private bool _state;

        public Switch(int nodeId, bool state)
        {
            _nodeId = nodeId;
            _state = state;
        }

        #region ISwitch Member

        public int NodeId
        {
            get { return _nodeId; }
        }

        public bool SwitchState
        {
            get { return _state; }
        }

        #endregion
    }

    public class GraphWeight : IGraphWeight
    {
        private string _name;
        private Guid _guid = Guid.NewGuid();
        private GraphWeightDataType _dataType;
        private GraphWeightFeatureClasses _gwfcs = new GraphWeightFeatureClasses();

        public GraphWeight()
        {
        }
        public GraphWeight(string name, GraphWeightDataType dataType)
        {
            _name = name;
            _dataType = dataType;
        }

        #region IGraphWeight Member

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        [System.ComponentModel.Browsable(false)]
        public Guid Guid
        {
            get { return _guid; }
        }
        public GraphWeightDataType DataType
        {
            get { return _dataType; }
            set { _dataType = value; }
        }

        [System.ComponentModel.Browsable(false)]
        public GraphWeightFeatureClasses FeatureClasses
        {
            get { return _gwfcs; }
        }

        #endregion

        #region IPersistable Member

        public void Load(gView.Framework.IO.IPersistStream stream)
        {
            _name = (string)stream.Load("name", String.Empty);
            _guid = new Guid((string)stream.Load("guid", new Guid()));
            _dataType = (GraphWeightDataType)stream.Load("datatype", (int)GraphWeightDataType.Double);

            IGraphWeightFeatureClass gwfc;
            while ((gwfc = (IGraphWeightFeatureClass)stream.Load("IGraphWeightFeatureClass", null, new GraphWeightFeatureClass())) != null)
            {
                _gwfcs.Add(gwfc);
            }
        }

        public void Save(gView.Framework.IO.IPersistStream stream)
        {
            stream.Save("name", _name);
            stream.Save("guid", _guid.ToString());
            stream.Save("datatype", (int)_dataType);

            foreach (IGraphWeightFeatureClass gwfc in _gwfcs)
            {
                stream.Save("IGraphWeightFeatureClass", gwfc);
            }
        }

        #endregion
    }

    public class GraphWeightFeatureClass : IGraphWeightFeatureClass
    {
        private int _fcId;
        private string _fieldName;
        private ISimpleNumberCalculation _calc = null;

        internal GraphWeightFeatureClass()
        {
            _fcId = 0;
            _fieldName = String.Empty;
        }
        public GraphWeightFeatureClass(int fcId)
        {
            _fcId = fcId;
        }
        public GraphWeightFeatureClass(int fcId, string fieldName)
            : this(fcId)
        {
            _fieldName = fieldName;
        }

        #region IGraphWeightFeatureClass Member

        public int FcId
        {
            get { return _fcId; }
            set { _fcId = value; }
        }

        public string FieldName
        {
            get { return _fieldName; }
            set { _fieldName = value; }
        }

        public ISimpleNumberCalculation SimpleNumberCalculation
        {
            get { return _calc; }
            set { _calc = value; }
        }
        #endregion

        #region IPersistable Member

        public void Load(gView.Framework.IO.IPersistStream stream)
        {
            _fcId = (int)stream.Load("fcid", -1);
            _fieldName = (string)stream.Load("fieldname", String.Empty);
            _calc = stream.Load("calc", null) as ISimpleNumberCalculation;
        }

        public void Save(gView.Framework.IO.IPersistStream stream)
        {
            stream.Save("fcid", _fcId);
            stream.Save("fieldname", _fieldName);
            if (_calc != null)
                stream.Save("calc", _calc);
        }

        #endregion
    }

}
