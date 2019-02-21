using gView.MapServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.Models
{
    public class BrowseServicesIndexModel
    {
        public string Folder { get; set; }
        public string[] Folders { get; set; }
        public IEnumerable<IMapService> Services { get; set; }
    }
}
