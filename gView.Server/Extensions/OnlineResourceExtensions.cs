using System.Security.Principal;
using System.Text;

namespace gView.Server.Extensions;

static public class OnlineResourceExtensions
{
    static public string AppendWmsServerPath(this string onlineResource,
                                             IIdentity identity,
                                             string service,
                                             string folder)
    {
        StringBuilder sb = new StringBuilder();

        sb.Append(onlineResource);

        // TODO: identity = urltoken => /geoservices(__token__)/...
        sb.Append("/geoservices/rest/services/");
        if (!string.IsNullOrEmpty(folder))
        {
            sb.Append("/");
            sb.Append(folder);
        }

        sb.Append("/");
        sb.Append(service);
        sb.Append("/MapServer/WmsServer");

        return sb.ToString();
    }
}
