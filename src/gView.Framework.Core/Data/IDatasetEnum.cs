namespace gView.Framework.Core.Data
{
    public interface IDatasetEnum
    {
        void Reset();
        IDataset Next { get; }
    }
}