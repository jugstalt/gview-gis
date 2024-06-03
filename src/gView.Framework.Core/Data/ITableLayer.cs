namespace gView.Framework.Core.Data
{
    public interface ITableLayer : IDatasetElement
    {
        ITableClass TableClass { get; }
    }
}