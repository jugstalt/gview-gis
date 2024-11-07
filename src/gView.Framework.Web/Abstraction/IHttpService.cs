using gView.Framework.Web.Models;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace gView.Framework.Web.Abstraction
{
    public interface IHttpService
    {
        Task<byte[]> GetDataAsync(string url, RequestAuthorization authentication = null, int timeOutSeconds = 20);
        Task<string> GetStringAsync(string url, RequestAuthorization authorization = null, int timeOutSeconds = 20, Encoding encoding = null);

        Task<string> PostFormUrlEncodedStringAsync(string url,
                                                   string postData,
                                                   RequestAuthorization authorization = null,
                                                   int timeOutSeconds = 20);

        Task<byte[]> PostFormUrlEncodedAsync(string url,
                                             byte[] postData,
                                             RequestAuthorization authorization = null,
                                             int timeOutSeconds = 20);

        Task<byte[]> PostDataAsync(string url,
                                   byte[] postData,
                                   RequestAuthorization authorization = null,
                                   int timeOutSeconds = 20);

        Task<string> PostJsonAsync(string url,
                                   string json,
                                   RequestAuthorization authorization = null,
                                   int timeOutSeconds = 20);

        Task<string> PostXmlAsync(string url,
                                  string xml,
                                  RequestAuthorization authorization = null,
                                  int timeOutSeconds = 20,
                                  Encoding encoding = null);

        WebProxy GetProxy(string server);

        string AppendParametersToUrl(string url, string parameters);

        string ApplyUrlRedirection(string url);

        bool Legacy_AlwaysDownloadFrom(string filename);

        HttpClient Create(string url);
    }
}
