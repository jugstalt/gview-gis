using gView.Framework.Data;

namespace gView.Interoperability.Server
{
    internal class MapThemeClass : IClass
    {
        private IDataset _dataset;
        private string _name;

        public MapThemeClass(IDataset dataset, string name)
        {
            _dataset = dataset;
            _name = name;
        }

        #region IClass Member

        public string Name
        {
            get { return _name; }
        }

        public string Aliasname
        {
            get { return _name; }
        }

        public IDataset Dataset
        {
            get { return _dataset; }
        }

        #endregion IClass Member
    }
}