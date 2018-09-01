using gView.Framework.Carto;
using gView.MapServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.Models
{
    public class HomeServiceModel
    {
        public string Server { get; set; }
        public string OnlineResource { get; set; }
        public IServiceMap ServiceMap { get; set; }
        public IEnumerable<IServiceRequestInterpreter> Interpreters { get; set; }

        public string ReplaceRequest(string request)
        {
            return request?.Replace("{server}", Server).Replace("{onlineresource}", OnlineResource).Replace("{service}",ServiceMap.Name);
        }
    }
}
