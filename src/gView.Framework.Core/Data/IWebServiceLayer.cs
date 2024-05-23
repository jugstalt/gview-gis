namespace gView.Framework.Core.Data
{
    public interface IWebServiceLayer : ILayer
    {
        IWebServiceClass WebServiceClass { get; }
    }
}