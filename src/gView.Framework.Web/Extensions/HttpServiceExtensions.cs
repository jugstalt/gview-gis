using gView.Framework.Web.Abstraction;
using System.Text.Json;
using System.Threading.Tasks;

namespace gView.Framework.Web.Extensions;

static public class HttpServiceExtensions
{
    async static public Task<T> GetAsync<T>(this IHttpService http, string url)
        => JsonSerializer.Deserialize<T>(await http.GetStringAsync(url));

}
