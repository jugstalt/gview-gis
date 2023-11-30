namespace gView.Framework.Core.Data.Filters
{
    public interface IFunctionFilter : IQueryFilter
    {
        string Function { get; }

        string FunctionAlias { get; }
    }
}
