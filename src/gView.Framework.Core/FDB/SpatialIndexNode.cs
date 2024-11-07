using gView.Framework.Core.Geometry;
using System;
using System.Collections.Generic;

namespace gView.Framework.Core.FDB
{
    public class SpatialIndexNode : ISpatialIndexNode, IComparable
    {
        private int _NID = 0, _PID = 0;
        private short _page;
        private IGeometry _geom;
        private List<int> _IDs;

        #region ISpatialIndexNode Member

        public int NID
        {
            get
            {
                return _NID;
            }
            set
            {
                _NID = value;
            }
        }

        public int PID
        {
            get
            {
                return _PID;
            }
            set
            {
                _PID = value;
            }
        }

        public IGeometry Rectangle
        {
            get
            {
                return _geom;
            }
            set
            {
                _geom = value;
            }
        }
        public List<int> IDs
        {
            get
            {
                return _IDs;
            }
            set
            {
                _IDs = value;
            }
        }

        public short Page
        {
            get
            {
                return _page;
            }
            set
            {
                _page = value;
            }
        }
        #endregion

        #region IComparable Member

        public int CompareTo(object obj)
        {
            return NID < ((ISpatialIndexNode)obj).NID ? -1 : 1;
            //return 0;
        }

        #endregion
    }
}
