using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Geometry;
using gView.Framework.Network;

namespace gView.Framework.Network.Build
{
    public class NetworkNode
    {
        private int _id;
        private IPoint _point;
        private int _firstGraphRow = -1;
        private int _lastGraphRow = -1;
        private bool _switchAble = false, _switchState = false;
        private int _fcId = -1, _fId = -1;
        private NetworkNodeType _type = NetworkNodeType.Unknown;

        public NetworkNode(int id, IPoint point)
        {
            _id = id;
            _point = point;
        }
        public NetworkNode(int id, IPoint point, int fcId, int fId)
            : this(id, point)
        {
            _fcId = fcId;
            _fId = fId;
        }

        #region Properties

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public IPoint Point
        {
            get { return _point; }
            set { _point = value; }
        }

        public int FirstGraphRow
        {
            get { return _firstGraphRow; }
            set { _firstGraphRow = value; }
        }
        public int LastGraphRow
        {
            get { return _lastGraphRow; }
            set { _lastGraphRow = value; }
        }

        public bool SwitchAble
        {
            get { return _switchAble; }
            set { _switchAble = value; }
        }
        public bool SwitchState
        {
            get { return _switchState; }
            set { _switchState = value; }
        }
        public int FeatureclassId
        {
            get { return _fcId; }
            set { _fcId = value; }
        }
        public int FeatureId
        {
            get { return _fId; }
            set { _fId = value; }
        }
        public NetworkNodeType Type
        {
            get { return _type; }
            set { _type = value; }
        }
        #endregion
    }
}
