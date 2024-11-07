namespace gView.Framework.Core.Data
{
    public interface IClass
    {
        string Name { get; }
        string Aliasname { get; }

        IDataset Dataset { get; }
    }
}