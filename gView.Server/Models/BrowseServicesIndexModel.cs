using gView.MapServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.Models
{
    public class BrowseServicesIndexModel
    {
        public bool IsPublisher { get; set; }
        public bool IsManager { get; set; }
        public string Folder { get; set; }
        public string[] Folders { get; set; }
        public IEnumerable<IMapService> Services { get; set; }

        public string ErrorMessage { get; set; }
        public string ServiceName { get; set; }
    }
}
