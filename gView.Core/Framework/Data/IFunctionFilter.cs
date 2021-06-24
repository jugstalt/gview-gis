namespace gView.Framework.Data
{
    public interface IFunctionFilter : IQueryFilter
    {
        string Function { get; }

        string FunctionAlias { get; }
    }
}
