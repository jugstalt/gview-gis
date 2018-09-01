using gView.MapServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.Models
{
    public class HomeIndexModel
    {
        public IEnumerable<IMapService> Services { get; set; }
    }
}
