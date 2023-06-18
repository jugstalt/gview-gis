namespace gView.Cmd.Core;
internal class RequiredCommandParameter<T> : CommandParameter<T>
{
    public RequiredCommandParameter(string name)
        : base(name)
    {
        IsRequired = true;
    }
}
