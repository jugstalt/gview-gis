using gView.Framework.Core.Data;
using gView.Framework.Data.Metadata;
using System;

namespace gView.Framework.Data
{
    public class DataElementID : DatasetElementMetadata, IID, IStringID
    {
        private int _id = 0;
        private string _sid = null;

        #region IID Member

        public int ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        #endregion

        #region IStringID Member

        public string SID
        {
            get
            {
                if (String.IsNullOrEmpty(_sid))
                {
                    return _id.ToString();
                }

                return _sid;
            }
            set
            {
                _sid = value;
            }
        }

        public bool HasSID
        {
            get { return !String.IsNullOrEmpty(_sid); }
        }
        #endregion
    }
}
