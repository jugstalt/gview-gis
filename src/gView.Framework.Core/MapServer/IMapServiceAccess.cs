namespace gView.Framework.Core.MapServer
{
    public interface IMapServiceAccess
    {
        string Username { get; set; }
        string[] ServiceTypes { get; }

        void AddServiceType(string serviceType);
        void RemoveServiceType(string serviceType);

        bool IsAllowed(string serviceType);
    }
}
