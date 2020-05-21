using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.Services.MapServer
{
    public class InternetMapServerCapabilites
    {
        public InternetMapServerCapabilites()
        {
            AllowDeployment = true;
        }
        public bool AllowDeployment { get; set; }
    }
}
