using System.Threading.Tasks;

namespace gView.Framework.IO
{
    public interface IPersistStream : IErrorReport
    {
        object Load(string key);
        object Load(string key, object defVal);
        object Load(string key, object defVal, object objectInstance);

        Task<T> LoadAsync<T>(string key, T objectInstance, T defaultValue = default(T))
            where T : IPersistableLoadAsync;

        Task<T> LoadPluginAsync<T>(string key, T unknownPlugin = default(T))
            where T : IPersistableLoadAsync;

        void Save(string key, object val);
        void SaveEncrypted(string key, string val);
        //void Save(string key, object val, object objectInstance);
        //void WriteStream(string path);
        //void ReadStream(string path);
    }
}
