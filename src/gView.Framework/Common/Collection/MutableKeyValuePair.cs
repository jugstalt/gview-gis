namespace gView.Framework.Common.Collection;
internal class MutableKeyValuePair<TKey, TValue>
{
    public MutableKeyValuePair(TKey key, TValue value)
    {
        this.Key = key;
        this.Value = value;
    }

    public TKey Key { get; }
    public TValue Value { get; set; }
}
