namespace gView.Framework.Core.MapServer
{
    public interface IMapServerInstanceService
    {
        string ServiceRequest(string service, string request, string InterpreterGUID, string usr, string pwd);
        string ServiceRequest2(string OnlineResource, string service, string request, string InterpreterGUID, string usr, string pwd);

        bool AddMap(string mapName, string MapXML, string usr, string pwd);
        bool RemoveMap(string mapName, string usr, string pwd);

        string Ping();

        string GetMetadata(string mapName, string usr, string pwd);
        bool SetMetadata(string mapName, string metadata, string usr, string pwd);

        bool ReloadMap(string mapName, string usr, string pwd);
    }
}
