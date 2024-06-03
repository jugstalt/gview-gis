using gView.Framework.Core.Common;
using System;

namespace gView.Framework.Data
{
    public class NullLayer : Layer, IErrorMessage
    {
        private int _id_ = -1, _datasetID_ = -1;
        private bool _IsWebTheme = false;
        private string _ID = String.Empty, _className = String.Empty;

        public NullLayer()
            : base()
        {
        }

        public int PersistLayerID
        {
            get { return _id_; }
            set { _id_ = value; }
        }
        public int PersistDatasetID
        {
            get { return _datasetID_; }
            set { _datasetID_ = value; }
        }
        public bool PersistIsWebTheme
        {
            get { return _IsWebTheme; }
            set { _IsWebTheme = value; }
        }
        public string PersistWebThemeID
        {
            get { return _ID; }
            set { _ID = value; }
        }
        public string PersistClassName
        {
            get { return _className; }
            set { _className = value; }
        }

        public string LastErrorMessage { get; set; } = "";
    }
}
