using gView.Framework.Core.MapServer;
using System;
using System.Collections.Generic;

namespace gView.Server.Models
{
    public class BrowseServicesServiceModel
    {
        public string Server { get; set; }
        public string OnlineResource { get; set; }
        public IMapService MapService { get; set; }
        public IEnumerable<IServiceRequestInterpreter> Interpreters { get; set; }

        /// <summary>Resource types (MapServer, FeatureServer, NetworkServer) the service offers for GeoServices (REST)</summary>
        public ISet<string> GeoServiceTypes { get; set; } = new HashSet<string>();

        /// <summary>Anonymous users have access to the service</summary>
        public bool IsPublic { get; set; }

        /// <summary>Number of layers of the service, null if the service map could not be loaded</summary>
        public int? LayerCount { get; set; }

        public DateTime? RunningSinceUtc { get; set; }

        /// <summary>The user may delete the service</summary>
        public bool IsPublisher { get; set; }

        /// <summary>The user may open Manage (admin network, manage user)</summary>
        public bool CanManage { get; set; }
    }
}
