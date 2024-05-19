using gView.Framework.Core.IO;

namespace gView.Framework.Core.Data
{
    public interface IDatasetElement : IID, IStringID, IMetadata
    {
        string Title { get; set; }
        IClass Class { get; }
        //string Group { get ; set ; }

        int DatasetID { get; set; }

        event PropertyChangedHandler PropertyChanged;
        void FirePropertyChanged();
    }
}