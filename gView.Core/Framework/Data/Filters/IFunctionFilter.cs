namespace gView.Framework.Data.Filters
{
    public interface IFunctionFilter : IQueryFilter
    {
        string Function { get; }

        string FunctionAlias { get; }
    }
}
