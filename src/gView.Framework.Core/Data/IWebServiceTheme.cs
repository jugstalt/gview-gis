namespace gView.Framework.Core.Data
{
    public interface IWebServiceTheme : IFeatureLayer
    {
        string LayerID { get; }
        bool Locked { get; set; }
        IWebServiceClass ServiceClass { get; }
    }
}