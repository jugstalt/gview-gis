using gView.Core.Framework.Exceptions;
using gView.Framework.system;
using gView.Server.AppCode;
using gView.Server.Services.MapServer;
using System;
using System.Threading.Tasks;

namespace gView.Server.Services.Security
{
    public class AccessTokenAuthServiceOptions
    {
        public string Authority { get; set; }
        public string AccessTokenParameterName { get; set; }
        public bool AllowAccessTokenAuthorization { get; set; }
    }
}
