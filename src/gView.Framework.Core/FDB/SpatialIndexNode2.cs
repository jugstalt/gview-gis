namespace gView.Framework.Core.FDB
{
    public class SpatialIndexNode2 : ISpatialIndexNode2
    {
        private int _NID = 0, _PID = 0;
        private short _page;

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

    }
}
