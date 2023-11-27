using gView.Framework.Core.MapServer;
using System.Collections.Generic;

namespace gView.Server.Models
{
    public class BrowseServicesIndexModel
    {
        public bool IsPublisher { get; set; }
        public bool IsManager { get; set; }
        public string Folder { get; set; }
        public string[] Folders { get; set; }
        public IEnumerable<IMapService> Services { get; set; }

        public string Message { get; set; }
        public string ServiceName { get; set; }
    }
}
