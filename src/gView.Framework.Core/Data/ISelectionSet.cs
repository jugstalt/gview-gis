namespace gView.Framework.Core.Data
{
    public interface ISelectionSet
    {
        void Clear();
        int Count { get; }
        void Combine(ISelectionSet selSet, CombinationMethod method);
    }
}